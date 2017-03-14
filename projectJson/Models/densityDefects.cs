using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class densityDefects
    {
        public string date { get; set; }
        public string devManufacturing { get; set; }
        public string system { get; set; }
        public string project { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public int qtyDefects { get; set; }
        public int qtyCTs { get; set; }
        public double density { get; set; }
    }
}