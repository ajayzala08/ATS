using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class SubmissionViewModel
    {
        public decimal id { get; set; }
        public string name { get; set; }
        public string client { get; set; }
        public string skill { get; set; }
        public string number { get; set; }
        public string email { get; set; }
        public string totexp { get; set; }
        public string np { get; set; }
        public string ctc { get; set; }
        public string recruiter { get; set; }
        public string subdate { get; set; }
    }
}