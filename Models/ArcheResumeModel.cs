using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class ArcheResumeModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal contact { get; set; }
        public string email { get; set; }
        public DateTime? dob { get; set; }
        public string skill { get; set; }
        public string location { get; set; }
        public decimal? experience { get; set; }
        public int? noticeperiod { get; set; }
        public float? ctc { get; set; }
        public string resume { get; set; }
    }
}