using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class LeaveModel
    {
        public decimal lid { get; set; }
        public string ltype { get; set; }
        public float lnoofdays { get; set; }
        public string linwords { get; set; }
        public string lreason { get; set; }
        public DateTime lstartdate { get; set; }
        public DateTime lenddate { get; set; }
        public string lcreateby { get; set; }
        public DateTime lcreateon { get; set; }

        public string ltlstatus { get; set; }
        public DateTime ltldate { get; set; }
        public string lmstatus { get; set; }
        public DateTime lmdate { get; set; }
        public string ladminstatus { get; set; }
        public DateTime ladmindate { get; set; }



    }
}