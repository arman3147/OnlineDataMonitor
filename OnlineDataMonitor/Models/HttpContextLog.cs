using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineDataMonitor.Models
{
    public class HttpContextLog
    {
        public Guid ID { get; set; }
        public string UpstreamGatewayURL { get; set; }
        public string RequestHeaders { get; set; }
        public string RequestBody { get; set; }
        public string ResponseHeaders { get; set; }
        public string ResponseBody { get; set; }
        public string RequestTime { get; set; }
    }
}