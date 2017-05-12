using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models.Project
{
    public class ProductivityXDefectsGroupWeekly
    {
        public string fullWeekNumber { get; set; }

        public int productivity { get; set; }
        public int realized { get; set; }
        public double realizedAverage { get; set; }

        public int qtyDefectsAmb { get; set; }
        public int qtyDefectsCons { get; set; }
        public int qtyDefectsTot { get; set; }

        public int qtyDefectsAmbAcum { get; set; }
        public int qtyDefectsConsAcum { get; set; }
        public int qtyDefectsTotAcum { get; set; }
    }
}
