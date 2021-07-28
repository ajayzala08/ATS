using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class LeaveBalanceModel
    {
        public string name { get; set; }
        public string title { get; set; }
        public float total { get; set; }
        public float totused { get; set; }
        public float totbal { get; set; }

    }
}