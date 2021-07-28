using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class ARViewModel
    {
        public int ARID { get; set; }
        public int EmpCode { get; set; }
        public string EmpName { get; set; }
        public string EmpDesignation { get; set; }
        public string EmpDepartment { get; set; }
        public DateTime EmpDate { get; set; }
        public DateTime Enddate { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string EmpComment { get; set; }
        public string ManagerComment { get; set; }

    }
}