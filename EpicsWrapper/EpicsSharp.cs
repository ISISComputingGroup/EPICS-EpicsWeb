using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSI.EpicsClient2;

namespace EpicsWrapper
{
    public class EpicsSharp
    {
        EpicsClient _client;
        Dictionary<String, EpicsChannel> _channels; //Does it make sense to cache PV channels?

        public EpicsSharp()
        {
            _client = new EpicsClient();
            _client.Configuration.WaitTimeout = 5000;
            _channels = new Dictionary<string, EpicsChannel>();
        }

        public void SetSearchAddresses(string addresses)
        {
            _client.Configuration.SearchAddress = addresses;
        }

        public string GetSimplePvAsString(string pvname)
        {
            if (!_channels.ContainsKey(pvname))
            {
                EpicsChannel channel = _client.CreateChannel<string>(pvname);
                _channels[pvname] = channel;
            }

            try
            {
                return _channels[pvname].Get<string>();
            }
            catch (Exception er)
            {
                return er.Message;
            }
            return "test";
        }

        public string GetWaveformPvAsString(string pvname)
        {
            if (!_channels.ContainsKey(pvname))
            {
                EpicsChannel channel = _client.CreateChannel<sbyte[]>(pvname);
                _channels[pvname] = channel;
            }

            try
            {
                sbyte[] temp = _channels[pvname].Get<sbyte[]>();

                string data = "";

                for (int i = 0; i < temp.Length; ++i)
                {
                    if (temp[i] < 0)
                    {
                        //This may be a problem if extended ascii codes are used!
                    }
                    data += Convert.ToChar(temp[i]);
                }

                return data.TrimEnd('\0');

            }
            catch (Exception er)
            {
                return er.Message;
            }

            return "test";
        }
    }
}
