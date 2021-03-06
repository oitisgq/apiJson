﻿using System;
namespace ProjectJson.Models.Project
{
    public class Project
    {
        public Int32 id { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public string subprojectDelivery { get; set; }
        public string name { get; set; }
        public string objective { get; set; }
        public string classification { get; set; }
        public string state { get; set; }
        public string release { get; set; }
        public string GP { get; set; }
        public string N3 { get; set; }
        public string UN { get; set; }
        public string trafficLight { get; set; }
        public string rootCause { get; set; }
        public string actionPlan { get; set; }
        public string informative { get; set; }
        public string attentionPoints { get; set; }
        public string attentionPointsOfIndicators { get; set; }
        public string IterationsActive { get; set; }
        public string IterationsSelected { get; set; }
        public Int32 gap { get; set; }
    }
}