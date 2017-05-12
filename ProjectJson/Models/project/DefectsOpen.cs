using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models.Project
{
    public class DefectsOpen
    {
        public int defect { get; set; }
        public string status { get; set; }
        public string forwardedTo { get; set; }
        public string defectSystem { get; set; }
        public string severity { get; set; }
        public double aging { get; set; }
        public string agingDisplay { get; set; }
        public int pingPong { get; set; }
    }
}
