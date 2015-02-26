using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using EpicsWrapper;
using System.Globalization;

namespace PVWebService
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class PVWebService : System.Web.Services.WebService
    {
        static EpicsWrapper.EpicsSharp _epics = new EpicsSharp();
        static bool addressesSet = false;
        static Dictionary<string, CachedPV> cache = new Dictionary<string, CachedPV>();
        static TimeSpan expires = new TimeSpan(0, 0, 30);

        public PVWebService()
        {
            if (!addressesSet)
            {
                //Not fully thread safe, but it does not matter if the address get set multiple times really
                addressesSet = true;
                _epics.SetSearchAddresses("127.255.255.255;ROKE.ND.RL.AC.UK:5066");
            }
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string Hello()
        {
            return "Hello!";
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetPV(string pv)
        {
            try
            {
                EpicsWrapper.EpicsReturnValue retVal;

                if (cache.Keys.Contains(pv) && cache[pv].LastUpdated + expires > DateTime.UtcNow)
                {
                    retVal = cache[pv].Value;
                }
                else
                {
                    retVal = _epics.GetPV(pv, false);
                    addToCache(pv, retVal);
                }

                return ConvertToJson(retVal);
            }
            catch (Exception err)
            {
                EpicsReturnValue retVal = new EpicsReturnValue();
                retVal.Name = pv;
                retVal.Error = err.Message;
                return ConvertToJson(retVal);
            }
        }

        private static void addToCache(string pv, EpicsWrapper.EpicsReturnValue retVal)
        {
            CachedPV c = new CachedPV();
            c.Value = retVal;
            c.LastUpdated = DateTime.UtcNow;

            if (!cache.Keys.Contains(pv))
            {
                try
                {
                    cache.Add(pv, c);
                }
                catch (Exception e)
                {
                    //Just in case there is a race condition
                }
            }
            else
            {
                cache[pv] = c;
            }
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetWaveformDehexedDecompressed(string pv)
        {
            try
            {
                EpicsWrapper.EpicsReturnValue retVal;

                if (cache.Keys.Contains(pv) && cache[pv].LastUpdated + expires > DateTime.UtcNow)
                {
                    retVal = cache[pv].Value;
                }
                else
                {
                    retVal = _epics.GetPV(pv, true);
                    retVal.Value = unzip(dehex(retVal.Value.ToString()));
                    addToCache(pv, retVal);
                }

                return ConvertToJson(retVal);
            }
            catch (Exception err)
            {
                EpicsReturnValue retVal = new EpicsReturnValue();
                retVal.Name = pv;
                retVal.Error = err.Message;
                return ConvertToJson(retVal);
            }
        }

        private static string ConvertToJson(EpicsReturnValue retVal)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                var serializedResult = serializer.Serialize(retVal);

                return serializedResult;
            }
            catch (Exception err)
            {
                return "{}";
            }
        }

        private static string unzip(byte[] s)
        {
            return Ionic.Zlib.ZlibStream.UncompressString(s);
        }

        private static byte[] dehex(String hexed)
        {
            List<byte> bytes = new List<byte>();

            for (int i = 0; i < hexed.Length; i += 2)
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
