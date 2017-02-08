using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace projectJson.Models
{
    public class projects
    {
        public string id { get; set; }
        public string devManufacturing { get; set; }
        public string system { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public string name { get; set; }
        public string classification { get; set; }
        public string release { get; set; }
        public Int32 release_ano { get; set; }
        public Int32 release_mes { get; set; }
    }
}