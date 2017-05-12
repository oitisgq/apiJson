using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class defectDetail
    {
        public string project { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string cycle { get; set; }
        public int CT { get; set; }
        public string ctName { get; set; }
        public string ctSystem { get; set; }
        public string defectSystem { get; set; }
        public string devManuf { get; set; }
        public string testManuf { get; set; }
        public string forwardedTo { get; set; }
        public string severity { get; set; }
        public string source { get; set; }
        public string nature { get; set; }
        public string status { get; set; }
        public string dtOpening { get; set; }
        public string dtForecastingSolution { get; set; }
        public string detectableInDev { get; set; }
        public int qtyReopened { get; set; }
        public int qtyImpactedCTs { get; set; }
        public int qtyPingPong { get; set; }
        public double aging { get; set; }
        public string agingDisplay { get; set; }
        public string Comments { get; set; }
    }
}
