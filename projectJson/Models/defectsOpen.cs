using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class defectsOpen
    {
        public string project { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public int defect { get; set; }
        public string status { get; set; }
        public string forwardedTo { get; set; }
        public string defectSystem { get; set; }
        public string severity { get; set; }
        public double aging { get; set; }
        public int pingPong { get; set; }
    }
}
