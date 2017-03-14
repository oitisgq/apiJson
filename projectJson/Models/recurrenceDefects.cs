using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class recurrenceDefects
    {
        public string date { get; set; }
        public string devManufacturing { get; set; }
        public string system { get; set; }
        public string project { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public int qty { get; set; }
    }
}