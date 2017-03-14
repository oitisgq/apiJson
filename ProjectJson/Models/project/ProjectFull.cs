using System.Collections.Generic;

namespace ProjectJson.Models.Project
{
    public class ProjectFull
    {
        // public Project project { get; set; }

        public DefectDensity defectDensity { get; set; }

        public DefectAverangeTime defectAverangeTime { get; set; }
        public DefectAverangeTime defectAverangeTimeHigh { get; set; }
        public IList<DefectAverangeTimeGroupSeverity> defectAverangeTimeGroupSeverity { get; set; }

        public DefectReopened defectReopened { get; set; }
        public DetectableInDev detectableInDev { get; set; }
        public StatusLastDays statusLastDays { get; set; }
        public IList<Status> statusGroupMonth { get; set; }
        public IList<DefectStatus> defectStatus { get; set; }
        public IList<DefectStatus> defectsGroupOrigin { get; set; }

    }
}