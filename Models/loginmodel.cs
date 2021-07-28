using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class loginmodel
    {
        [Required(ErrorMessage ="Enter username")]
        public string username { get; set; }
        [Required(ErrorMessage ="Enter pasword")]
        public string pwd { get; set; }
        public Boolean rememberme { get; set; }
    }
}