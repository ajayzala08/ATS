using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class SalesMonthlyReportModel
    {
        public decimal smrid { get; set; }
        public string smrtime { get; set; }
        public int client { get; set; }
        public int position { get; set; }
        public int business { get; set; }
        public int submission { get; set; }
        public int intreceived { get; set; }
        public int feedbackpending { get; set; }
        public int noshow { get; set; }
        public int offer { get; set; }
        public int bd { get; set; }
        public int join { get; set; }
        public int passthrough { get; set; }
        public int bulkdeal { get; set; }
        public int poextend { get; set; }
        public int attrition { get; set; }
        public decimal totrevenue { get; set; }
        public string remark { get; set; }
        public string createby { get; set; }
        public DateTime createon { get; set; }
    }
}