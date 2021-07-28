using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class ViewTobejoinModel
    {
        public decimal oid { get; set; }
        public string name { get; set; }
        public string client { get; set; }
        public string location { get; set; }
        public string type { get; set; }
        public string skill { get; set; }
        public DateTime seldate { get; set; }
        public DateTime offdate { get; set; }
        public DateTime joindate { get; set; }
        public string recruitername { get; set; }

        public string status { get; set; }

    }
}