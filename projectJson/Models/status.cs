using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace projectJson.Models
{
    public class status
    {
        public string project { get; set; }

        public string subproject { get; set; }

        public string delivery { get; set; }

        public string date { get; set; }
        public string dateOrder { get; set; }

        public int active { get; set; }

        public int activeUAT { get; set; }

        public int planned { get; set; }

        public int realized { get; set; }

        public int productivity { get; set; }

        public double GAP { get; set; }

        public int approvedTI { get; set; }

        public int approvedUAT { get; set; }


        public int activeAcum { get; set; }

        public int activeUATAcum { get; set; }

        public int plannedAcum { get; set; }

        public int realizedAcum { get; set; }

        public int productivityAcum { get; set; }

        public double GAPAcum { get; set; }

        public int approvedTIAcum { get; set; }

        public int approvedUATAcum { get; set; }


        public double percPlanned { get; set; }

        public double percRealized { get; set; }

        public double percGAP { get; set; }

        public double percApprovedTI { get; set; }

        public double percApprovedUAT { get; set; }
    }
}