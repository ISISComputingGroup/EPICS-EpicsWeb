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
                    //_epics.SetSearchAddresses("130.246.49.66;130.246.58.66:5066;130.246.49.58:5068");
                    _epics.SetSearchAddresses(addresses);
                }

                String name = _pvroot;
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
                    string name = _pvroot + valuesReq[i];
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

        private void getBlocks()
        {
            try
            {
                List<String> rawgroups = new List<string>(_epics.GetWaveformPvAsString(_pvroot + ":GROUPINGS").Split(';'));
                List<Group> groups = new List<Group>();

                foreach (string s in rawgroups)
                {
                    if (s.StartsWith("|"))
                    {
                        groups.Add(new Group(s.Replace("|", "").Trim()));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            groups[groups.Count - 1].Blocks.Add(s);
                        }
                    }
                }

                if (groups.Count == 1 && groups[0].Name == "NONE")
                {
                    //No groups so use simple formatting
                    lblBlocks.Text = "<ul>";

                    foreach (string name in groups[0].Blocks)
                    {
                        //Get the value
                        string value = _epics.GetSimplePvAsString(name);
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
                        string value = _epics.GetSimplePvAsString(name);
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
    }
}