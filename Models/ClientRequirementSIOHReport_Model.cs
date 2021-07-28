using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class ClientRequirementSIOHReport_Model
    {
        public decimal rid { get; set; }
        public string client { get; set; }
        public string jobcode { get; set; }
        public string skill { get; set; }
        public string type { get; set; }
        public string location { get; set; }
        public int position { get; set; }
        public int submission { get; set; }
        public int interview { get; set; }
        public int offer { get; set; }
        public int hire { get; set; }
        public int bd { get; set; }
        public string status { get; set; }
        public string createdon { get; set; }
    }
}