using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class ARView
    {
        public int employeeid { get; set; }
        public DateTime? starttime { get; set; }
        public DateTime? endtime { get; set; }
        public int? status { get; set; }
        public int? artype { get; set; }
        public int? approvalid { get; set; }
        public string employeecomment { get; set; }
        public string managercomment { get; set; }
        public int mpd_id { get; set; }

    }
}