using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class ClientModel
    {
        public decimal cid { get; set; }
        public string cname { get; set; }
        public string caddress { get; set; }
        public string cperson1 { get; set; }
        public decimal ccnt1 { get; set; }
        public string cemail1 { get; set; }
        public string cperson2 { get; set; }
        public decimal ccnt2 { get; set; }
        public string cemail2 { get; set; }
        public string ccategory { get; set; }
        public string ctype { get; set; }
        public string csegment { get; set; }
        public string cmargintype { get; set; }
        public decimal cmargin { get; set; }
        public int cisactive { get; set; }
        public int cisdelete { get; set; }


    }
}