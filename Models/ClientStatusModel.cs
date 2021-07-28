using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class ClientStatusModel
    {
        public decimal nid { get; set; }
        public string nclient { get; set; }
        public string npoc { get; set; }
        public string nstatus { get; set; }
        public string nremark1 { get; set; }
        public string nremark2 { get; set; }
    }
}