using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATS2019_2.Models;
using System.Web.Security;

namespace ATS2019_2.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult login()
        {
            loginmodel lm = new loginmodel();
            HttpCookie cook = Request.Cookies["authcookie"];
            if (cook != null)
            {
                lm.username = cook["username"] != null ? cook["username"] :"";
                lm.pwd = cook["pwd"] != null ? cook["pwd"] : "";
                lm.rememberme = true;
            }

            return View(lm);
        }
        [HttpPost]
        public ActionResult checklogin(loginmodel lm)
        {
            using (var context= new ATS2019_dbEntities())
            {
                try
                {
                   
                        Boolean finduserexistance = context.User_Master.Where(c => c.E_UserName == lm.username && c.E_Password == lm.pwd && c.E_Is_Active == 1 && c.E_Is_Delete == 0).Any();
                        if (finduserexistance)
                        {
                            if (lm.rememberme)
                            {
                                var userimage = context.User_Master.Where(c => c.E_UserName == lm.username).Select(c => c.E_Photo).First();
                                HttpCookie cookie = new HttpCookie("authcookie");
                                cookie["username"] = lm.username.ToString();
                                cookie["pwd"] = lm.pwd.ToString();
                                cookie["imgsrc"] = userimage;
                                Response.Cookies.Add(cookie);
                                cookie.Expires = System.DateTime.Now.AddDays(365);

                                FormsAuthentication.SetAuthCookie(lm.username.ToString(), true);
                            }
                            else
                            {
                                var userimage = context.User_Master.Where(c => c.E_UserName == lm.username).Select(c => c.E_Photo).First();
                                HttpCookie cookie = new HttpCookie("authcookie");
                                cookie["username"] = "";
                                cookie["pwd"] = "";
                                cookie["imgsrc"] = userimage;
                                Response.Cookies.Add(cookie);
                                cookie.Expires = System.DateTime.Now.AddDays(1);
                                FormsAuthentication.SetAuthCookie(lm.username.ToString(), false);
                            }
                            CreateActivityLog(lm.username, "Login", System.DateTime.Now);
                            return RedirectToAction("Welcome", "ATS");

                        }
                        else
                        {

                        TempData["loginmsg"] = "Username or password not match.";
                            ModelState.Clear();
                            return RedirectToAction("login");
                        }
                    
                    
                   
                }catch(Exception loginexcetion)
                {
                    ModelState.Clear();
                    return Content(loginexcetion.Message.ToString());
                    //return RedirectToAction("login");
                }
            }
        }

        public ActionResult logout()
        {
            CreateActivityLog(User.Identity.Name, "Logout", System.DateTime.Now);
            FormsAuthentication.SignOut();
            
            return RedirectToAction("login");
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