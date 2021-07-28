using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class SalesClientWiseMonthlyRevenueModel
    {
        public decimal srid { get; set; }
        public string srmonth { get; set; }
        public string sryear { get; set; }
        public decimal srclient { get; set; }
        public int srcurrenthc { get; set; }
        public float srtotgp { get; set; }
        public float sravggpadded { get; set; }
        public int srstart { get; set; }
        public int srattrition { get; set; }
        public int srbd { get; set; }
        public int sractualstart { get; set; }
        public float srnettotgp { get; set; }
        public float srnettotgpadded { get; set; }
        public string srtypeofemployement { get; set; }
    }
}