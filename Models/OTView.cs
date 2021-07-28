using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class OTView
    {
        public int employeeid { get; set; }
        public string starttime { get; set; }
        public string endtime { get; set; }
        public int? status { get; set; }
        public int? approvalid { get; set; }
        public string employeecomment { get; set; }
        public string managercomment { get; set; }
        public int ot_id { get; set; }
    }
}