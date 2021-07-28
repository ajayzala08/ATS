using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class UserModel
    {
        public int EId { get; set; }
        
        public Nullable<decimal> ECode { get; set; }
        
        public string EFullname { get; set; }
        public string EGender { get; set; }
        
        
        public string EEmail { get; set; }
        
        public Nullable<decimal> EContNo { get; set; }
        
        public Nullable<decimal> EEmergencyContNo { get; set; }
        public Nullable<System.DateTime> EDOB { get; set; }
        [Required]
        public string EUserName { get; set; }
        [Required]
        public string EPassword { get; set; }
        [Required]
        [Compare("EPassword",ErrorMessage="Password Not Match")]
        public string ERetype { get; set; }
        public string EAddress { get; set; }
        public string ECAddress { get; set; }
        
        public decimal EPostal { get; set; }
        public string EEtype { get; set; }
        public string ECompany_Name { get; set; }
        public string EDepartment { get; set; }
        public string EDesignation { get; set; }
        public string ELocation { get; set; }
        public string EBloodGroup { get; set; }
        public Nullable<System.DateTime> EOfferDate { get; set; }
        public Nullable<System.DateTime> EJoinDate { get; set; }
        public Nullable<System.DateTime> EExitDate { get; set; }
        public string ERole { get; set; }
        public string EBankName { get; set; }
        public string EAccountNo { get; set; }
        public string EBranch { get; set; }
        public string EIFC_Code { get; set; }
        public string EPFAccount { get; set; }
        public string EPAN_No { get; set; }
        public Nullable<decimal> EAadhar_no { get; set; }
        public Nullable<double> ESalary { get; set; }
        public Nullable<double> EPF { get; set; }
        public Nullable<double> EPT { get; set; }
        public Nullable<double> EESI { get; set; }
        public Boolean EPFApply { get; set; }
        public Boolean EPTApply { get; set; }
        public Boolean EESIApply { get; set; }

        public string Ephoto { get; set; }
        public int eactive { get; set; }
        public int edelete { get; set; }

    }
}