using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class SalesClientCallModel
    {
        public decimal id { get; set; }
        public DateTime dt { get; set; }
        public string client { get; set; }
        public string poc { get; set; }
        public string agenda { get; set; }
        
    }
}