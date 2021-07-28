using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATS2019_2.Models;
using System.IO;

namespace ATS2019_2.Controllers
{
    public class RegisterController : Controller
    {
        // GET: Register
        public ActionResult Registration()
        {
            return View();
        }

        public ActionResult getemployeeid()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var id = (from c in context.User_Master where c.E_Role == "Contract" select c).ToList().Max(c => c.E_Code).ToString();

                decimal nextid = decimal.Parse(id) + decimal.Parse("1");

                decimal ch = checkemployeecode(nextid);
                return Json(ch, JsonRequestBehavior.AllowGet);
            }
            
        }

        public decimal checkemployeecode(decimal nid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var cnt = (from c in context.User_Master where c.E_Code == nid select c).Count();
                if (cnt > 0)
                {
                    nid = nid + decimal.Parse("1");
                    checkemployeecode(nid);
                }
                return nid;
            }
        }
        [HttpPost]
        public ActionResult Registration(UserModel umm, HttpPostedFileBase userimage)
        {
            if (ModelState.IsValid)
            {
                using (var context = new ATS2019_dbEntities())
                {
                    Boolean finduser = (from c in context.User_Master where (c.E_Code == umm.ECode) || (c.E_Fullname == umm.EFullname) select c).Any();
                    if (!finduser)
                    {
                        User_Master um = new User_Master();
                        um.E_Code = umm.ECode;
                        um.E_Fullname = umm.EFullname;
                        um.E_Gender = umm.EGender;
                        um.E_Email = umm.EEmail;
                        um.E_DOB = umm.EDOB;
                        um.E_ContNo = umm.EContNo;
                        um.E_EEmergencyContNo = umm.EEmergencyContNo;
                        um.E_UserName = umm.EUserName;
                        um.E_Password = umm.EPassword;
                        um.E_Address = umm.EAddress;
                        um.E_CAddress = umm.ECAddress;
                        um.E_Postal = umm.EPostal;
                        um.E_Etype = umm.EEtype;
                        um.E_Company_Name = umm.ECompany_Name;
                        um.E_Department = umm.EDepartment;
                        um.E_Designation = umm.EDesignation;
                        um.E_Location = umm.ELocation;
                        um.E_OfferDate = umm.EOfferDate;
                        um.E_JoinDate = umm.EJoinDate;
                        um.E_Role = umm.ERole;
                        um.E_BloodGroup = umm.EBloodGroup;
                        um.E_BankName = umm.EBankName;
                        um.E_AccountNo = umm.EAccountNo;
                        um.E_Branch = umm.EBranch;
                        um.E_IFC_Code = umm.EIFC_Code;
                        um.E_PF_Account = umm.EPFAccount;
                        um.E_PAN_No = umm.EPAN_No;
                        um.E_Aadhar_no = umm.EAadhar_no;
                        um.E_Salary = umm.ESalary;
                        um.E_PF = umm.EPF;
                        um.E_PT = umm.EPT;
                        um.E_ESI = umm.EESI;
                        if (umm.EPFApply)
                        {
                            um.E_PF_Apply = Convert.ToInt16("1");
                        }
                        else
                        {
                            um.E_PF_Apply = Convert.ToInt16("0");
                        }
                        if (umm.EPTApply)
                        {
                            um.E_PT_Apply = Convert.ToInt16("1");
                        }
                        else
                        {
                            um.E_PT_Apply = Convert.ToInt16("0");
                        }
                        if (umm.EESIApply)
                        {
                            um.E_ESI_Apply = Convert.ToInt16("1");
                        }
                        else
                        {
                            um.E_ESI_Apply = Convert.ToInt16("0");
                        }
                        if (userimage != null)
                        {
                            FileInfo fi = new FileInfo(userimage.FileName);
                            string[] filedes = fi.Name.Split('.');
                            string userimagename = umm.ECode + "." + filedes[1].ToString();
                            um.E_Photo = userimagename;
                            userimage.SaveAs(Server.MapPath("~/UserImage/") + userimagename);
                        }
                        else
                        {
                            um.E_Photo = "Dummy.jpg";
                        }
                        um.E_Is_Active = Convert.ToInt16("1");
                        um.E_Is_Delete = Convert.ToInt16("0");

                        um.E_Created_On = System.DateTime.Now;
                        um.E_Created_By = "Username";


                        context.User_Master.Add(um);
                        context.SaveChanges();
                        CreateActivityLog(umm.EFullname, "User Register", System.DateTime.Now);
                        ModelState.Clear();
                        return RedirectToAction("login", "Login");
                        
                    }
                    else
                    {
                        ViewBag.msg = "User already exists !";
                        return View();
                    }
                }
            }
            else
            {
                return View();

            }
        }
        public void CreateActivityLog(string un, string act, DateTime dt)
        {
            using (var context = new ATS2019_dbEntities())
            {
                User_Activity_Master uam = new User_Activity_Master();
                uam.Username = un;
                uam.Activity = act;
                uam.ActivityOn = dt;
                context.User_Activity_Master.Add(uam);
                context.SaveChanges();
            }
        }
    }
}