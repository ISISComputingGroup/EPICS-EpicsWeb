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
                    _epics.SetSearchAddresses(addresses);
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
                    value = _epics.GetSimplePvAsString(name.Replace("::", ":"));
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

        class Group
        {
            public string Name;
            public List<string> Blocks = new List<string>();

            public Group(string name)
            {
                Name = name;
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
                List<Group> groups = new List<Group>();

                String hexed = _epics.GetWaveformPvAsString("IN:" + _instrument + ":CS:BLOCKSERVER:GROUPINGS");
                string xml = Unzip(dehex(hexed));

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNode root = doc.DocumentElement;

                XmlNodeList nodeList = root.SelectNodes("./group");

                foreach (XmlNode grp in nodeList)
                {
                    Group temp = new Group(grp.Attributes["name"].Value);

                    XmlNodeList subnodeList = grp.SelectNodes("./block");

                    foreach (XmlNode blk in subnodeList)
                    {
                        temp.Blocks.Add(blk.Attributes["name"].Value);
                    }

                    groups.Add(temp);
                }

                if (groups.Count == 1 && groups[0].Name == "NONE")
                {
                    //No groups so use simple formatting
                    lblBlocks.Text = "<ul>";

                    foreach (string name in groups[0].Blocks)
                    {
                        //Get the value
                        string value = _epics.GetSimplePvAsString("IN:" + _instrument + "CS:SB:" + name);
                        lblBlocks.Text += "<li>" + name + ": " + value + "</li>";
                    }

                    lblBlocks.Text += "</ul>";
                    return;
                }

                lblBlocks.Text = "";

                foreach (Group g in groups)
                {
                    if (g.Name == "NONE")
                    {
                        lblBlocks.Text += "<span style=\"font-weight:bold;\">Other</span><ul>";
                    }
                    else
                    {
                        lblBlocks.Text += "<span style=\"font-weight:bold;\">" + g.Name + "</span><ul>";
                    }

                    foreach (String name in g.Blocks)
                    {
                        string value = _epics.GetSimplePvAsString("IN:" + _instrument + ":CS:SB:" + name);
                        lblBlocks.Text += "<li>" + name + ": " + value + "</li>";
                    }
                    lblBlocks.Text += "</ul>";
                }
            }
            catch
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