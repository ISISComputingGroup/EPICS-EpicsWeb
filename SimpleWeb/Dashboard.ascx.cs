using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Configuration;
using System.Text;
using System.IO.Compression;
using System.Globalization;
using System.Xml;
using System.Web.Script.Serialization;

namespace SimpleWeb
{
    public partial class Dashboard : System.Web.UI.UserControl
    {
        EpicsWrapper.EpicsSharp _epics;
        string _instrument;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["instrument"] != null)
            {
                _instrument = Session["instrument"].ToString().ToUpper();

                if (_epics == null)
                {
                    _epics = new EpicsWrapper.EpicsSharp();

                    string addresses = ConfigurationManager.AppSettings[_instrument.ToUpper()];
                    //_epics.SetSearchAddresses("130.246.49.66;130.246.58.66:5066;130.246.49.58:5068");
                    _epics.SetSearchAddresses(addresses + ";127.255.255.255;ROKE.ND.RL.AC.UK:5066");
                }

                String name = _instrument;
                if (name.StartsWith("NDX"))
                {
                    name = name.Remove(0, 3);
                }

                lblName.Text = name;

                if (getRunInfo())
                {
                    getBlocks();
                    getErrorCount();
                    lblUpdated.Text = "Last Updated: " + DateTime.Now.ToString("HH:mm:ss");
                }

                //Should we clean up the channels?
                _epics.CloseChannelsAndDisconnect();
            }
        }

        private Boolean getRunInfo()
        {
            List<string> labelsReq = new List<string>();
            List<string> valuesReq = new List<string>();

            string runinfofile = ConfigurationManager.AppSettings["RunInfoList"];

            using (StreamReader sr = new StreamReader(runinfofile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split('=');
                    labelsReq.Add(parts[0] + ":");
                    valuesReq.Add(parts[1]);
                }
            }

            for (int i = 0; i < labelsReq.Count; ++i)
            {
                string value = "";

                if (!String.IsNullOrEmpty(valuesReq[i]))
                {
                    string name = "IN:" + _instrument + valuesReq[i];
                    value = _epics.GetPV(name.Replace("::", ":"), false).StrValue;
                }
                lstRunInfo.Items.Add(labelsReq[i] + " " + value);
            }

            return true;
        }

        private void getErrorCount()
        {
            try
            {
            }
            catch
            {
            }
        }

        private static string Unzip(byte[] s)
        {
            return Ionic.Zlib.ZlibStream.UncompressString(s);           
        }

        private void getBlocks()
        {
            try
            {
                String hexed = _epics.GetPV("IN:" + _instrument + ":CS:BLOCKSERVER:GROUPS", true).Value.ToString();
                string json = Unzip(dehex(hexed));

                var serializer = new JavaScriptSerializer();
                List<Group> groups = serializer.Deserialize<List<Group>>(json);

                if (groups.Count == 1 && groups[0].name == "NONE")
                {
                    //No groups so use simple formatting
                    lblBlocks.Text = "<ul>";

                    foreach (string name in groups[0].blocks)
                    {
                        //Get the value
                        string value = _epics.GetPV("IN:" + _instrument + "CS:SB:" + name, false).StrValue;
                        lblBlocks.Text += "<li>" + name + ": " + value + "</li>";
                    }

                    lblBlocks.Text += "</ul>";
                    return;
                }

                lblBlocks.Text = "";

                foreach (Group g in groups)
                {
                    if (g.name == "NONE")
                    {
                        lblBlocks.Text += "<span style=\"font-weight:bold;\">Other</span><ul>";
                    }
                    else
                    {
                        lblBlocks.Text += "<span style=\"font-weight:bold;\">" + g.name + "</span><ul>";
                    }

                    foreach (String name in g.blocks)
                    {
                        string value = _epics.GetPV("IN:" + _instrument + ":CS:SB:" + name, false).StrValue;
                        lblBlocks.Text += "<li>" + name + ": " + value + "</li>";
                    }
                    lblBlocks.Text += "</ul>";
                }
            }
            catch (Exception err)
            {
                lblBlocks.Text = "";
            }
        }

        private static byte[] dehex(String hexed)
        {
            List<byte> bytes = new List<byte>();

            for (int i = 0; i < hexed.Length; i+=2)
            {
                string hex = hexed.Substring(i, 2);
                int num = int.Parse(hex, NumberStyles.AllowHexSpecifier);
                char cnum = (char)num;
                bytes.Add((byte)num);
            }

            return bytes.ToArray();
        }
    }
}