using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class ProjectXGrouper
    {
        public Int32 id { get; set; }
        public Int32 project { get; set; }
        public string subproject { get; set; }
        public string delivery { get; set; }
        public Int32 grouper { get; set; }
    }
}