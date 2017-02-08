using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace projectJson.Models
{
    public class defectReopened
    {
        public string date { get; set; }
        public string devManufacturing { get; set; }
        public string system { get; set; }
        public string project { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public int qtyTotal { get; set; }
        public int qtyReopened { get; set; }
        public double percent { get; set; }
        public double percentReference { get; set; }
        public double qtyReference { get; set; }
    }
}