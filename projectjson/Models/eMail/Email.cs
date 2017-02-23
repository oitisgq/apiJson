using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace projectJson.Models.eMail
{
    public class Email
    {
        public string from { get; set; } = "sgq@oi.net.br";
        public string to { get; set; } = "joao.frade@oi.net.br";

        // public List<string> to { get; set; }

        // public List<string> cc { get; set; }

        public string subject { get; set; }

        public string body { get; set; }

        // public HttpPostedFileBase attachment { get; set; }
    }
}