using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class RequirementModel
    {
        public decimal jid { get; set; }
        public string jobcode { get; set; }
        public decimal jclientid { get; set; }
        public string jskill { get; set; }
        public int jposition { get; set; }
        public string jlocation { get; set; }
        public string jendclient { get; set; }
        public string jassignuser { get; set; }
        public float jtotmin { get; set; }
        public float jtotmax { get; set; }
        public float jrelmin { get; set; }
        public float jrelmax { get; set; }
        public decimal jbillrate { get; set; }
        public decimal jpayrate { get; set; }
        public string jcategory { get; set; }
        public string jtype { get; set; }
        public string jemployementtyp { get; set; }
        public string jpoc { get; set; }
        public decimal jpocno { get; set; }
        public string jjd { get; set; }
        public string jstatus { get; set; }
        public string jmandatoryskill { get; set; }

    }
}