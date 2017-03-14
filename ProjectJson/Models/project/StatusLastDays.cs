using System;
using System.Collections.Generic;

namespace ProjectJson.Models.Project
{
    public class StatusLastDays
    {
        public IList<Status> last05Days { get; set; }
        public IList<Status> last30Days { get; set; }
    }
}