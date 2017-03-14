using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class ctsImpactedXDefects
    {
        public string project { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public string date { get; set; }

        public int qtyDefectsAmb { get; set; }
        public int qtyDefectsCons { get; set; }
        public int qtyDefectsTot { get; set; }
        public int qtyCtsImpacted { get; set; }

        public int qtyDefectsAmbAcum { get; set; }
        public int qtyDefectsConsAcum { get; set; }
        public int qtyDefectsTotAcum { get; set; }
        public int qtyCtsImpactedAcum { get; set; }
    }
}