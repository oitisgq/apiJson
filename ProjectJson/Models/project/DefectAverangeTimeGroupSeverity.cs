namespace ProjectJson.Models.Project
{
    public class DefectAverangeTimeGroupSeverity
    {
        public string severity { get; set; }
        public int qtyDefects { get; set; }
        public double qtyHours { get; set; }
        public double averageHours { get; set; } // QtyHours / QtyDefects
    }
}