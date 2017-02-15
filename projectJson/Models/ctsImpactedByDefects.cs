using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace projectJson.Models
{
    public class ctsImpactedByDefects
    {
        public string date { get; set; }
        public string name { get; set; }
        public int qtyCtsImpacted { get; set; }
        public int qtyDefects { get; set; }
    }
}