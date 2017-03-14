using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectJson.Models
{
    public class bptValidPlanoEvid
    {
        public string id { get; set; }
        public string release { get; set; }
        public string classification { get; set; }
        public string name { get; set; }
        public string bpt { get; set; }
        public string plano_val_tecnica { get; set; }
        public string plano_motiv_rej_tec { get; set; }
        public string plano_comentarios_rej_tec { get; set; }
        public string plano_val_cliente { get; set; }
        public string plano_motiv_rej_cliente { get; set; }
        public string plano_comentarios_rej_cliente { get; set; }
        public string evidencia_val_tecnica { get; set; }
        public string evidencia_motiv_rej_tec { get; set; }
        public string evidencia_comentarios_rej_tec { get; set; }
        public string evidencia_val_cliente { get; set; }
        public string evidencia_motiv_rej_cliente { get; set; }
        public string evidencia_comentarios_rej_cliente { get; set; }
        public string components { get; set; }
    }
}