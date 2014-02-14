using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Configuration;

namespace SimpleWeb
{
    public partial class Dashboard : System.Web.UI.UserControl
    {
        EpicsWrapper.EpicsSharp _epics;
        string _pvroot;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PvRoot"] != null)
            {
                _pvroot = Session["PvRoot"].ToString();

                if (_epics == null)
                {
                    _epics = new EpicsWrapper.EpicsSharp();

                    string addresses = ConfigurationManager.AppSettings["AddressList"];
                    //_epics.SetSearchAddresses("130.246.49.66;130.246.49.66:5066;130.246.49.66:5068");
                    _epics.SetSearchAddresses(addresses);
                }

                String name = _pvroot.ToUpper();
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
                    value = _epics.GetSimplePvAsString(_pvroot + valuesReq[i]);
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

        private void getBlocks()
        {
            List<String> blocknames = new List<string>(_epics.GetWaveformPvAsString("NDW613:BLOCKS").Split(';'));

            List<String> blockvalues = new List<string>();

            lblBlocks.Text = "<ul>";

            foreach(string name in blocknames)
            {
                //Get the value
                string value = _epics.GetSimplePvAsString(name);
                //blockvalues.Add(_epics.GetSimplePvAsString(name));

                lblBlocks.Text += "<li>" + name + ": " + value + "</li>";
            }

            lblBlocks.Text += "</ul>";




            //try
            //{
            //    List<String> blocks = getBlocksFromSeci(getSeciConnection(lblName.Text), getInvisible);

            //    lblBlocks.Text = "";
            //    lblBlocksTest.Text = "";

            //    if (blocks.Count > 0)
            //    {
            //        lblBlocks.Text += "<span style=\"font-weight:bold;\">Blocks</span>";

            //        Dictionary<String, List<String>> groups = new Dictionary<string, List<string>>();
            //        groups["None"] = new List<string>();

            //        for (int i = 0; i < blocks.Count; ++i)
            //        {
            //            if (blocks[i].Contains("<Group="))
            //            {
            //                int lastindex = blocks[i].LastIndexOf('=');
            //                String group = blocks[i].Substring(lastindex + 1, blocks[i].Length - lastindex - 2);
            //                if (groups.ContainsKey(group))
            //                {
            //                    groups[group].Add(blocks[i].Substring(0, blocks[i].IndexOf("<Group=")));
            //                }
            //                else
            //                {
            //                    groups[group] = new List<string>();
            //                    groups[group].Add(blocks[i].Substring(0, blocks[i].IndexOf("<Group=")));
            //                }
            //            }
            //            else
            //            {
            //                groups["None"].Add(blocks[i]);
            //            }
            //        }

            //        foreach (String key in groups.Keys)
            //        {
            //            if (key != "None")
            //            {
            //                lblBlocksTest.Text += "<span style=\"font-weight:bold;\">" + key + "</span><ul>";
            //                foreach (String value in groups[key])
            //                {
            //                    lblBlocksTest.Text += "<li>" + value + "</li>";
            //                }
            //                lblBlocksTest.Text += "</ul>";
            //            }
            //        }

            //        if (groups.Keys.Count > 1 && groups["None"].Count > 0)
            //        {
            //            lblBlocksTest.Text += "<span style=\"font-weight:bold;\">Other</span><ul>";

            //            foreach (String value in groups["None"])
            //            {
            //                lblBlocksTest.Text += "<li>" + value + "</li>";
            //            }
            //            lblBlocksTest.Text += "</ul>";
            //        }
            //        else
            //        {
            //            lblBlocksTest.Text += "<ul>";
            //            foreach (String value in groups["None"])
            //            {
            //                lblBlocksTest.Text += "<li>" + value + "</li>";
            //            }
            //            lblBlocksTest.Text += "</ul>";
            //        }
            //    }
            //}
            //catch
            //{
            //    lblBlocks.Text = "";
            //    lblBlocksTest.Text = "";
            //}
        }
    }
}