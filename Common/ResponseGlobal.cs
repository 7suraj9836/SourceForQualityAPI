using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Common
{
    public class ResponseGlobal
    {
        public int ResponseCode { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
    }
}
