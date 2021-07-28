using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class SIOSModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int submission { get; set; }
        public int interview { get; set; }
        public int offer { get; set; }
        public int start { get; set; }
    }
}