using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSI.EpicsClient2;
using System.Threading.Tasks;

namespace EpicsWrapper
{

    public class EpicsReturnValue
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
        public string Error { get; set; }
        public string StrValue { get; set; }    //For waveform StrValue is always null to save space
    }

    public class EpicsSharp
    {
        EpicsClient _client;
        Dictionary<String, EpicsChannel> _channels; //Does it make sense to cache PV channels?
        private Object channelsLock = new Object();

        public EpicsSharp()
        {
            _client = new EpicsClient();
            _client.Configuration.WaitTimeout = 2000;
            _channels = new Dictionary<string, EpicsChannel>();
        }

        public void SetSearchAddresses(string addresses)
        {
            _client.Configuration.SearchAddress = addresses;
        }

        public void CloseChannelsAndDisconnect()
        {
            _client.Dispose();
        }

        //public async Task<EpicsReturnValue> GetPVAsync(string pvname, bool waveformAsString)
        //{
        //    EpicsReturnValue retVal = new EpicsReturnValue();
        //    retVal.Name = pvname;

        //    try
        //    {
        //        if (!_channels.ContainsKey(pvname))
        //        {
        //            createChannel(pvname);
        //        }

        //        Type chtype = _channels[pvname].ChannelDefinedType;
        //        uint numelem = _channels[pvname].ChannelDataCount;

        //        if (numelem > 1 && chtype == typeof(byte))
        //        {
        //            //It is probably a waveform of some sort
        //            Task<byte[]> wave = _channels[pvname].GetAsync<byte[]>();

        //            byte[] waveContents = await wave;

        //            retVal.Type = "Waveform";
        //            if (waveformAsString)
        //            {
        //                retVal.Value = waveformToString(waveContents);
        //            }
        //            else
        //            {
        //                retVal.Value = wave;
        //            }

        //        }
        //        else if (chtype == typeof(Enum))
        //        {
        //            //For enums (bo, bi, mbbi, mbbo) get the numerical value and the string value (may be empty)
        //            Task<string> sval = _channels[pvname].GetAsync<string>();
        //            Task<int> val = _channels[pvname].GetAsync<int>();

        //            await Task.WhenAll(sval, val);

        //            retVal.Type = chtype.Name.ToString();
        //            retVal.Value = await val;
        //            retVal.StrValue = await sval;
        //        }
        //        else
        //        {
        //            //Simple value
        //            Task<object> val = _channels[pvname].GetAsync<object>();
        //            retVal.Type = chtype.Name.ToString();
        //            retVal.Value = await val;
        //            retVal.StrValue = retVal.Value.ToString();
        //        }
        //    }
        //    catch (Exception er)
        //    {
        //        retVal.Error = er.Message;
        //    }

        //    return retVal;
        //}

        public EpicsReturnValue GetPV(string pvname, bool waveformAsString)
        {
            EpicsReturnValue retVal = new EpicsReturnValue();
            retVal.Name = pvname;

            try
            {

                //Double checking to ensure thread safety
                if (!_channels.ContainsKey(pvname))     //First check
                {
                    lock (channelsLock)
                    {
                        if (!_channels.ContainsKey(pvname))     //Second check
                        {
                            createChannel(pvname);
                        }
                    }
                }

                Type chtype = _channels[pvname].ChannelDefinedType;
                uint numelem = _channels[pvname].ChannelDataCount;

                object value = null;
                if (numelem > 1 && chtype == typeof(byte))
                {
                    //It is probably a waveform of some sort
                    byte[] val = _channels[pvname].Get<byte[]>();
                    retVal.Type = "Waveform";
                    if (waveformAsString)
                    {
                        retVal.Value = waveformToString(val);
                    }
                    else
                    {
                        retVal.Value = val;
                    }
                }
                else if (chtype == typeof(Enum))
                {
                    //For enums (bo, bi, mbbi, mbbo) get the numerical value and the string value (may be empty)
                    string svalue = _channels[pvname].Get<string>();
                    value = _channels[pvname].Get<int>();
                    retVal.Type = chtype.Name.ToString();
                    retVal.Value = value;
                    retVal.StrValue = svalue;
                }
                else
                {
                    //Simple value
                    value = _channels[pvname].Get<object>();
                    retVal.Type = chtype.Name.ToString();
                    retVal.Value = value;
                    retVal.StrValue = value.ToString();
                }
            }
            catch (Exception er)
            {
                retVal.Error = er.Message;
            }

            return retVal;
        }

        private void createChannel(string pvname)
        {
            EpicsChannel channel = _client.CreateChannel(pvname);
            channel.Connect();
            //Check the type and number of elements
            if (channel.ChannelDefinedType == typeof(byte) && channel.ChannelDataCount > 1)
            {
                //It is a waveform so recreate the channel
                channel.Dispose();
                channel = _client.CreateChannel<byte[]>(pvname);
                channel.Connect();
            }

            _channels[pvname] = channel;
        }

        private static string waveformToString(byte[] temp)
        {
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

    }
}
