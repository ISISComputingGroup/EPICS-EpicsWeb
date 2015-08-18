using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PVWebService
{
    public class CachedPV
    {
        public EpicsWrapper.EpicsReturnValue Value { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}