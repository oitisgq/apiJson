using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class Grouper
    {
        public Int32 id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string executiveSummary { get; set; }
        public string trafficLight { get; set; }
    }
}