using ATS2019_2.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Facebook;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Echovoice.JSON;

namespace ATS2019_2.Controllers
{

    [Authorize]
    public class ATSController : Controller
    {
        public Dictionary<string, string> Attendacedetails = new Dictionary<string, string>();
        //welcome page
        [HttpGet]
        public ActionResult Welcome()
        {
            return View();
        }

        //Dashboard Start
        [HttpGet]
        public ActionResult Admindashboard()
        {
            return View();
        }
        [HttpGet]
        [OutputCache(Duration = 30)]
        public ActionResult AdminDashBoardNew()
        {
            using (var context = new ATS2019_dbEntities())
            {

                //Today Count
                List<columnchart_model> chmtoday = new List<columnchart_model>();
                int todaysubcnt = 0, todayintcnt = 0, todayoffcnt = 0, todaystartcnt = 0;
                todaysubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();
                chmtoday.Add(new columnchart_model("Submission", todaysubcnt));

                todayintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmtoday.Add(new columnchart_model("Interview", todayintcnt));

                todayoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();
                chmtoday.Add(new columnchart_model("Offer", todayoffcnt));

                todaystartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();
                chmtoday.Add(new columnchart_model("Start", todaystartcnt));
                ViewBag.TodayChartdata = JsonConvert.SerializeObject(chmtoday);

                //Yesterday Count
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                List<columnchart_model> chmyesterday = new List<columnchart_model>();
                int yesterdaysubcnt = 0, yesterdayintcnt = 0, yesterdayoffcnt = 0, yesterdaystartcnt = 0;
                yesterdaysubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && (c.R_Status == "Accept"))).ToList().Count();
                chmyesterday.Add(new columnchart_model("Submission", yesterdaysubcnt));

                yesterdayintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmyesterday.Add(new columnchart_model("Interview", yesterdayintcnt));

                yesterdayoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday))).ToList().Count();
                chmyesterday.Add(new columnchart_model("Offer", yesterdayoffcnt));

                yesterdaystartcnt = (from c in context.Offer_Master
                                     where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday)
                                     join
                                         d in context.Resume_Master on c.O_Rid equals d.R_Id
                                     join
                                         e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                     join
                                         f in context.Client_Master on e.J_Client_Id equals f.C_id
                                     join
                                         g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                     select new { c, d, e, f, g }).Count();
                chmyesterday.Add(new columnchart_model("Start", yesterdaystartcnt));
                ViewBag.yesterdayChartdata = JsonConvert.SerializeObject(chmyesterday);

                //This week data count
                List<columnchart_model> chmthisweek = new List<columnchart_model>();
                int twsubcnt = 0, twintcnt = 0, twoffcnt = 0, twstartcnt = 0;
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);

                twsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && (c.R_Status == "Accept"))).ToList().Count();
                chmthisweek.Add(new columnchart_model("Submission", twsubcnt));

                twintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmthisweek.Add(new columnchart_model("Interview", twintcnt));

                twoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday))).ToList().Count();
                chmthisweek.Add(new columnchart_model("Offer", twoffcnt));

                twstartcnt = (from c in context.Offer_Master
                              where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday)
                              join
                                  d in context.Resume_Master on c.O_Rid equals d.R_Id
                              join
                                  e in context.Requirement_Master on d.R_Jid equals e.J_Id
                              join
                                  f in context.Client_Master on e.J_Client_Id equals f.C_id
                              join
                                  g in context.User_Master on c.O_Recruiter equals g.E_UserName
                              select new { c, d, e, f, g }).Count();
                chmthisweek.Add(new columnchart_model("Start", twstartcnt));
                ViewBag.thisweekChartdata = JsonConvert.SerializeObject(chmthisweek);

                //Month Data Count
                List<columnchart_model> chmmonth = new List<columnchart_model>();
                int monthsubcnt = 0, monthintcnt = 0, monthoffcnt = 0, monthstartcnt = 0;

                monthsubcnt = context.Resume_Master.Where((c => c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept")).ToList().Count();
                chmmonth.Add(new columnchart_model("Submission", monthsubcnt));

                monthintcnt = context.Interview_Master.Where((c => c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmmonth.Add(new columnchart_model("Interview", monthintcnt));

                monthoffcnt = context.Offer_Master.Where((c => c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year)).ToList().Count();
                chmmonth.Add(new columnchart_model("Offer", monthoffcnt));

                monthstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();
                chmmonth.Add(new columnchart_model("Start", monthstartcnt));
                ViewBag.monthChartdata = JsonConvert.SerializeObject(chmmonth);



                //Till Date Count
                List<columnchart_model> chm = new List<columnchart_model>();
                int tdsubcnt = 0, tdintcnt = 0, tdoffcnt = 0, tdstartcnt = 0;
                tdsubcnt = context.Resume_Master.Where(c => c.R_Status == "Accept").ToList().Count();
                chm.Add(new columnchart_model("Submission", tdsubcnt));

                tdintcnt = context.Interview_Master.Where(c => c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show").ToList().Count();
                chm.Add(new columnchart_model("Interview", tdintcnt));

                tdoffcnt = context.Offer_Master.ToList().Count();
                chm.Add(new columnchart_model("Offer", tdoffcnt));

                tdstartcnt = (from c in context.Offer_Master
                              where c.O_Status == "Join"
                              join
                                  d in context.Resume_Master on c.O_Rid equals d.R_Id
                              join
                                  e in context.Requirement_Master on d.R_Jid equals e.J_Id
                              join
                                  f in context.Client_Master on e.J_Client_Id equals f.C_id
                              join
                                  g in context.User_Master on c.O_Recruiter equals g.E_UserName
                              select new { c, d, e, f, g }).Count();
                chm.Add(new columnchart_model("Start", tdstartcnt));
                ViewBag.Chartdata = JsonConvert.SerializeObject(chm);


                return View();
            }
        }
        [HttpGet]
        public ActionResult Managerdashboard()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Managerdashboardnew()
        {
            using (var context = new ATS2019_dbEntities())
            {

                //Today Count
                List<columnchart_model> chmtoday = new List<columnchart_model>();
                int todaysubcnt = 0, todayintcnt = 0, todayoffcnt = 0, todaystartcnt = 0;
                todaysubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();
                chmtoday.Add(new columnchart_model("Submission", todaysubcnt));

                todayintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmtoday.Add(new columnchart_model("Interview", todayintcnt));

                todayoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();
                chmtoday.Add(new columnchart_model("Offer", todayoffcnt));

                todaystartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();
                chmtoday.Add(new columnchart_model("Start", todaystartcnt));
                ViewBag.TodayChartdata = JsonConvert.SerializeObject(chmtoday);

                //Yesterday Count
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                List<columnchart_model> chmyesterday = new List<columnchart_model>();
                int yesterdaysubcnt = 0, yesterdayintcnt = 0, yesterdayoffcnt = 0, yesterdaystartcnt = 0;
                yesterdaysubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && (c.R_Status == "Accept"))).ToList().Count();
                chmyesterday.Add(new columnchart_model("Submission", yesterdaysubcnt));

                yesterdayintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmyesterday.Add(new columnchart_model("Interview", yesterdayintcnt));

                yesterdayoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday))).ToList().Count();
                chmyesterday.Add(new columnchart_model("Offer", yesterdayoffcnt));

                yesterdaystartcnt = (from c in context.Offer_Master
                                     where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday)
                                     join
                                         d in context.Resume_Master on c.O_Rid equals d.R_Id
                                     join
                                         e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                     join
                                         f in context.Client_Master on e.J_Client_Id equals f.C_id
                                     join
                                         g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                     select new { c, d, e, f, g }).Count();
                chmyesterday.Add(new columnchart_model("Start", yesterdaystartcnt));
                ViewBag.yesterdayChartdata = JsonConvert.SerializeObject(chmyesterday);

                //This week data count
                List<columnchart_model> chmthisweek = new List<columnchart_model>();
                int twsubcnt = 0, twintcnt = 0, twoffcnt = 0, twstartcnt = 0;
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);

                twsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && (c.R_Status == "Accept"))).ToList().Count();
                chmthisweek.Add(new columnchart_model("Submission", twsubcnt));

                twintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmthisweek.Add(new columnchart_model("Interview", twintcnt));

                twoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday))).ToList().Count();
                chmthisweek.Add(new columnchart_model("Offer", twoffcnt));

                twstartcnt = (from c in context.Offer_Master
                              where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday)
                              join
                                  d in context.Resume_Master on c.O_Rid equals d.R_Id
                              join
                                  e in context.Requirement_Master on d.R_Jid equals e.J_Id
                              join
                                  f in context.Client_Master on e.J_Client_Id equals f.C_id
                              join
                                  g in context.User_Master on c.O_Recruiter equals g.E_UserName
                              select new { c, d, e, f, g }).Count();
                chmthisweek.Add(new columnchart_model("Start", twstartcnt));
                ViewBag.thisweekChartdata = JsonConvert.SerializeObject(chmthisweek);

                //Month Data Count
                List<columnchart_model> chmmonth = new List<columnchart_model>();
                int monthsubcnt = 0, monthintcnt = 0, monthoffcnt = 0, monthstartcnt = 0;

                monthsubcnt = context.Resume_Master.Where((c => c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept")).ToList().Count();
                chmmonth.Add(new columnchart_model("Submission", monthsubcnt));

                monthintcnt = context.Interview_Master.Where((c => c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                chmmonth.Add(new columnchart_model("Interview", monthintcnt));

                monthoffcnt = context.Offer_Master.Where((c => c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year)).ToList().Count();
                chmmonth.Add(new columnchart_model("Offer", monthoffcnt));

                monthstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();
                chmmonth.Add(new columnchart_model("Start", monthstartcnt));
                ViewBag.monthChartdata = JsonConvert.SerializeObject(chmmonth);



                //Till Date Count
                List<columnchart_model> chm = new List<columnchart_model>();
                int tdsubcnt = 0, tdintcnt = 0, tdoffcnt = 0, tdstartcnt = 0;
                tdsubcnt = context.Resume_Master.Where(c => c.R_Status == "Accept").ToList().Count();
                chm.Add(new columnchart_model("Submission", tdsubcnt));

                tdintcnt = context.Interview_Master.Where(c => c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show").ToList().Count();
                chm.Add(new columnchart_model("Interview", tdintcnt));

                tdoffcnt = context.Offer_Master.ToList().Count();
                chm.Add(new columnchart_model("Offer", tdoffcnt));

                tdstartcnt = (from c in context.Offer_Master
                              where c.O_Status == "Join"
                              join
                                  d in context.Resume_Master on c.O_Rid equals d.R_Id
                              join
                                  e in context.Requirement_Master on d.R_Jid equals e.J_Id
                              join
                                  f in context.Client_Master on e.J_Client_Id equals f.C_id
                              join
                                  g in context.User_Master on c.O_Recruiter equals g.E_UserName
                              select new { c, d, e, f, g }).Count();
                chm.Add(new columnchart_model("Start", tdstartcnt));
                ViewBag.Chartdata = JsonConvert.SerializeObject(chm);



                return View();
            }
        }
        //[HttpGet]
        //public ActionResult VPdashboard()
        //{
        //    return View();
        //}
        [HttpGet]
        public ActionResult Salesdashboard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Salesdashboardnew()
        {

            using (var context = new ATS2019_dbEntities())
            {
                //Today Count
                List<columnchart_model> chmtoday = new List<columnchart_model>();
                int todaysubcnt = 0, todayintcnt = 0, todayoffcnt = 0, todaystartcnt = 0;
                todaysubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                chmtoday.Add(new columnchart_model("Submission", todaysubcnt));

                todayintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();
                chmtoday.Add(new columnchart_model("Interview", todayintcnt));

                todayoffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();
                chmtoday.Add(new columnchart_model("Offer", todayoffcnt));

                todaystartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();
                chmtoday.Add(new columnchart_model("Start", todaystartcnt));
                ViewBag.TodayChartdata = JsonConvert.SerializeObject(chmtoday);

                //Yesterday Count
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                List<columnchart_model> chmyesterday = new List<columnchart_model>();
                int yesterdaysubcnt = 0, yesterdayintcnt = 0, yesterdayoffcnt = 0, yesterdaystartcnt = 0;
                yesterdaysubcnt = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Resume_Master on e.J_Id equals f.R_Jid
                                   where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && f.R_Status == "Accept"
                                   select new { c, d, e, f }).ToList().Count();
                chmyesterday.Add(new columnchart_model("Submission", yesterdaysubcnt));

                yesterdayintcnt = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Interview_Master on e.J_Id equals f.I_JId
                                   where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   select new { c, d, e, f }).ToList().Count();
                chmyesterday.Add(new columnchart_model("Interview", yesterdayintcnt));

                yesterdayoffcnt = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Offer_Master on e.J_Id equals f.O_Jid
                                   where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                   join
                                       g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                   join
                                       h in context.Resume_Master on f.O_Rid equals h.R_Id
                                   join
                                       i in context.Client_Master on e.J_Client_Id equals i.C_id
                                   select new { c, d, e, f, g, h, i }).ToList().Count();
                chmyesterday.Add(new columnchart_model("Offer", yesterdayoffcnt));

                yesterdaystartcnt = (from c in context.User_Master
                                     where c.E_UserName == User.Identity.Name
                                     join
                                         d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                     join
                                         e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                     join
                                         f in context.Offer_Master on e.J_Id equals f.O_Jid
                                     where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && f.O_Status == "Join"
                                     select new { c, d, e, f }).ToList().Count();
                chmyesterday.Add(new columnchart_model("Start", yesterdaystartcnt));

                ViewBag.yesterdayChartdata = JsonConvert.SerializeObject(chmyesterday);

                //This week data count
                List<columnchart_model> chmthisweek = new List<columnchart_model>();
                int twsubcnt = 0, twintcnt = 0, twoffcnt = 0, twstartcnt = 0;
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);

                twsubcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                            join
                                e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                            join
                                f in context.Resume_Master on e.J_Id equals f.R_Jid
                            where EntityFunctions.TruncateTime(f.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && f.R_Status == "Accept"
                            select new { c, d, e, f }).ToList().Count();
                chmthisweek.Add(new columnchart_model("Submission", twsubcnt));

                twintcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                            join
                                e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                            join
                                f in context.Interview_Master on e.J_Id equals f.I_JId
                            where EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                            select new { c, d, e, f }).ToList().Count();
                chmthisweek.Add(new columnchart_model("Interview", twintcnt));

                twoffcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                            join
                                e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                            join
                                f in context.Offer_Master on e.J_Id equals f.O_Jid
                            where EntityFunctions.TruncateTime(f.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                            join
                                g in context.User_Master on f.O_Recruiter equals g.E_UserName
                            join
                                h in context.Resume_Master on f.O_Rid equals h.R_Id
                            join
                                i in context.Client_Master on e.J_Client_Id equals i.C_id
                            select new { c, d, e, f, g, h, i }).ToList().Count();
                chmthisweek.Add(new columnchart_model("Offer", twoffcnt));

                twstartcnt = (from c in context.User_Master
                              where c.E_UserName == User.Identity.Name
                              join
                                  d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                              join
                                  e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                              join
                                  f in context.Offer_Master on e.J_Id equals f.O_Jid
                              where EntityFunctions.TruncateTime(f.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && f.O_Status == "Join"
                              select new { c, d, e, f }).ToList().Count();
                chmthisweek.Add(new columnchart_model("Start", twstartcnt));
                ViewBag.thisweekChartdata = JsonConvert.SerializeObject(chmthisweek);

                //Month Data Count
                List<columnchart_model> chmmonth = new List<columnchart_model>();
                int monthsubcnt = 0, monthintcnt = 0, monthoffcnt = 0, monthstartcnt = 0;

                monthsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_CreateOn.Value.Month == System.DateTime.Now.Month && f.R_CreateOn.Value.Year == System.DateTime.Now.Year && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                chmmonth.Add(new columnchart_model("Submission", monthsubcnt));

                monthintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();
                chmmonth.Add(new columnchart_model("Interview", monthintcnt));

                monthoffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where f.O_Off_Date.Value.Month == System.DateTime.Now.Month && System.DateTime.Now.Year == System.DateTime.Now.Year
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();
                chmmonth.Add(new columnchart_model("Offer", monthoffcnt));

                monthstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where f.O_Join_Date.Value.Month == System.DateTime.Now.Month && f.O_Join_Date.Value.Year == System.DateTime.Now.Year && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();
                chmmonth.Add(new columnchart_model("Start", monthstartcnt));
                ViewBag.monthChartdata = JsonConvert.SerializeObject(chmmonth);



                //Till Date Count
                List<columnchart_model> chm = new List<columnchart_model>();
                int tdsubcnt = 0, tdintcnt = 0, tdoffcnt = 0, tdstartcnt = 0;

                tdsubcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                            join
                                e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                            join
                                f in context.Resume_Master on e.J_Id equals f.R_Jid
                            where f.R_Status == "Accept"
                            select new { c, d, e, f }).ToList().Count();
                chm.Add(new columnchart_model("Submission", tdsubcnt));

                tdintcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                            join
                                e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                            join
                                f in context.Interview_Master on e.J_Id equals f.I_JId
                            where (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                            select new { c, d, e, f }).ToList().Count();
                chm.Add(new columnchart_model("Interview", tdintcnt));

                tdoffcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                            join
                                e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                            join
                                f in context.Offer_Master on e.J_Id equals f.O_Jid
                            join
                                g in context.User_Master on f.O_Recruiter equals g.E_UserName
                            join
                                h in context.Resume_Master on f.O_Rid equals h.R_Id
                            join
                                i in context.Client_Master on e.J_Client_Id equals i.C_id
                            select new { c, d, e, f, g, h, i }).ToList().Count();
                chm.Add(new columnchart_model("Offer", tdoffcnt));

                tdstartcnt = (from c in context.User_Master
                              where c.E_UserName == User.Identity.Name
                              join
                                  d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                              join
                                  e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                              join
                                  f in context.Offer_Master on e.J_Id equals f.O_Jid
                              where f.O_Status == "Join"
                              select new { c, d, e, f }).ToList().Count();
                chm.Add(new columnchart_model("Start", tdstartcnt));

                ViewBag.Chartdata = JsonConvert.SerializeObject(chm);

            }

            return View();
        }

        [HttpGet]
        public ActionResult Teamleaddashboard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult TeamLeaddashboardnew()
        {
            using (var context = new ATS2019_dbEntities())
            {
                //Today Count
                List<columnchart_model> chmtoday = new List<columnchart_model>();
                int todaysubcnt = 0, todayintcnt = 0, todayoffcnt = 0, todaystartcnt = 0;


                string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                string[] tm = temlist.Split(',');
                foreach (var item in tm)
                {
                    todaysubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                                    select new { c, d }
                              ).Count();

                    todayintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();

                    todayoffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                    select new { c, d }).Count();


                    todaystartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.O_Status == "Join"
                                      select new { c, d }).Count();
                }
                chmtoday.Add(new columnchart_model("Submission", todaysubcnt));
                chmtoday.Add(new columnchart_model("Interview", todayintcnt));
                chmtoday.Add(new columnchart_model("Offer", todayoffcnt));
                chmtoday.Add(new columnchart_model("Start", todaystartcnt));

                ViewBag.TodayChartdata = JsonConvert.SerializeObject(chmtoday);

                //Yesterday Count
                List<columnchart_model> chmyesterday = new List<columnchart_model>();
                int yesterdaysubcnt = 0, yesterdayintcnt = 0, yesterdayoffcnt = 0, yesterdaystartcnt = 0;
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                foreach (var item in tm)
                {
                    yesterdaysubcnt += (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                            d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                        where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                        select new { c, d }
                              ).Count();

                    yesterdayintcnt += (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                            f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                        where
                                             EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                        select new { c, f }).Count();

                    yesterdayoffcnt += (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                            d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                        where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                        select new { c, d }).Count();


                    yesterdaystartcnt += (from c in context.User_Master
                                          where c.E_Fullname == item
                                          join
                                              d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                          where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && d.O_Status == "Join"
                                          select new { c, d }).Count();
                }

                chmyesterday.Add(new columnchart_model("Submission", yesterdaysubcnt));
                chmyesterday.Add(new columnchart_model("Interview", yesterdayintcnt));
                chmyesterday.Add(new columnchart_model("Offer", yesterdayoffcnt));
                chmyesterday.Add(new columnchart_model("Start", yesterdaystartcnt));
                ViewBag.yesterdayChartdata = JsonConvert.SerializeObject(chmyesterday);

                //week Count
                List<columnchart_model> chmweek = new List<columnchart_model>();
                int weeksubcnt = 0, weekintcnt = 0, weekoffcnt = 0, weekstartcnt = 0;
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);



                foreach (var item in tm)
                {
                    weeksubcnt += (from c in context.User_Master
                                   where c.E_Fullname == item
                                   join
                                       d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                   where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                                   select new { c, d }
                              ).Count();

                    weekintcnt += (from c in context.User_Master
                                   where c.E_Fullname == item
                                   join
                                       f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                   where
                                        EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   select new { c, f }).Count();

                    weekoffcnt += (from c in context.User_Master
                                   where c.E_Fullname == item
                                   join
                                       d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                   where EntityFunctions.TruncateTime(d.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                                   select new { c, d }).Count();


                    weekstartcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                     where EntityFunctions.TruncateTime(d.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && d.O_Status == "Join"
                                     select new { c, d }).Count();
                }

                chmweek.Add(new columnchart_model("Submission", weeksubcnt));
                chmweek.Add(new columnchart_model("Interview", weekintcnt));
                chmweek.Add(new columnchart_model("Offer", weekoffcnt));
                chmweek.Add(new columnchart_model("Start", weekstartcnt));
                ViewBag.thisweekChartdata = JsonConvert.SerializeObject(chmweek);

                List<columnchart_model> chmmonth = new List<columnchart_model>();
                int monthsubcnt = 0, monthintcnt = 0, monthoffcnt = 0, monthstartcnt = 0;

                foreach (var item in tm)
                {
                    monthsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                                    select new { c, d }
                              ).Count();

                    monthintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();

                    monthoffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where d.O_Off_Date.Value.Month == System.DateTime.Now.Month && d.O_Off_Date.Value.Year == System.DateTime.Now.Year
                                    select new { c, d }).Count();


                    monthstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where d.O_Join_Date.Value.Month == System.DateTime.Now.Month && d.O_Join_Date.Value.Year == System.DateTime.Now.Year && d.O_Status == "Join"
                                      select new { c, d }).Count();
                }
                chmmonth.Add(new columnchart_model("Submission", monthsubcnt));
                chmmonth.Add(new columnchart_model("Interview", monthintcnt));
                chmmonth.Add(new columnchart_model("Offer", monthoffcnt));
                chmmonth.Add(new columnchart_model("Start", monthstartcnt));
                ViewBag.monthChartdata = JsonConvert.SerializeObject(chmmonth);

                List<columnchart_model> chm = new List<columnchart_model>();
                int tdsubcnt = 0, tdintcnt = 0, tdoffcnt = 0, tdstartcnt = 0;

                foreach (var item in tm)
                {
                    tdsubcnt += (from c in context.User_Master
                                 where c.E_Fullname == item
                                 join
                                     d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                 where d.R_Status == "Accept"
                                 select new { c, d }
                              ).Count();

                    tdintcnt += (from c in context.User_Master
                                 where c.E_Fullname == item
                                 join
                                     f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                 where
                                      (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                 select new { c, f }).Count();

                    tdoffcnt += (from c in context.User_Master
                                 where c.E_Fullname == item
                                 join
                                     d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                 select new { c, d }).Count();


                    tdstartcnt += (from c in context.User_Master
                                   where c.E_Fullname == item
                                   join
                                       d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                   where d.O_Status == "Join"
                                   select new { c, d }).Count();
                }


                chm.Add(new columnchart_model("Submission", tdsubcnt));
                chm.Add(new columnchart_model("Interview", tdintcnt));
                chm.Add(new columnchart_model("Offer", tdoffcnt));
                chm.Add(new columnchart_model("Start", tdstartcnt));
                ViewBag.Chartdata = JsonConvert.SerializeObject(chm);



            }

            return View();
        }
        [HttpGet]
        public ActionResult Recruiterdashboard()
        {

            return View();
        }

        [HttpGet]
        public ActionResult Recruiterdashboardnew()
        {

            using (var context = new ATS2019_dbEntities())
            {
                //Today Count
                List<columnchart_model> chmtoday = new List<columnchart_model>();
                int todaysubcnt = 0, todayintcnt = 0, todayoffcnt = 0, todaystartcnt = 0;



                todaysubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                               where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                               select new { c, d }
                          ).Count();

                todayintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                               where
                                    EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, f }).Count();

                todayoffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                               where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                               select new { c, d }).Count();


                todaystartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                 where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.O_Status == "Join"
                                 select new { c, d }).Count();

                chmtoday.Add(new columnchart_model("Submission", todaysubcnt));
                chmtoday.Add(new columnchart_model("Interview", todayintcnt));
                chmtoday.Add(new columnchart_model("Offer", todayoffcnt));
                chmtoday.Add(new columnchart_model("Start", todaystartcnt));

                ViewBag.TodayChartdata = JsonConvert.SerializeObject(chmtoday);

                //Yesterday Count
                List<columnchart_model> chmyesterday = new List<columnchart_model>();
                int yesterdaysubcnt = 0, yesterdayintcnt = 0, yesterdayoffcnt = 0, yesterdaystartcnt = 0;
                DateTime yesterday = System.DateTime.Now.AddDays(-1);

                yesterdaysubcnt = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                   where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                   select new { c, d }
                          ).Count();

                yesterdayintcnt = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                   where
                                        EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   select new { c, f }).Count();

                yesterdayoffcnt = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                   where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                   select new { c, d }).Count();


                yesterdaystartcnt = (from c in context.User_Master
                                     where c.E_UserName == User.Identity.Name
                                     join
                                         d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                     where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && d.O_Status == "Join"
                                     select new { c, d }).Count();


                chmyesterday.Add(new columnchart_model("Submission", yesterdaysubcnt));
                chmyesterday.Add(new columnchart_model("Interview", yesterdayintcnt));
                chmyesterday.Add(new columnchart_model("Offer", yesterdayoffcnt));
                chmyesterday.Add(new columnchart_model("Start", yesterdaystartcnt));
                ViewBag.yesterdayChartdata = JsonConvert.SerializeObject(chmyesterday);

                //week Count
                List<columnchart_model> chmweek = new List<columnchart_model>();
                int weeksubcnt = 0, weekintcnt = 0, weekoffcnt = 0, weekstartcnt = 0;
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);




                weeksubcnt = (from c in context.User_Master
                              where c.E_UserName == User.Identity.Name
                              join
                                  d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                              where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                              select new { c, d }
                          ).Count();

                weekintcnt = (from c in context.User_Master
                              where c.E_UserName == User.Identity.Name
                              join
                                  f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                              where
                                   EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                              select new { c, f }).Count();

                weekoffcnt = (from c in context.User_Master
                              where c.E_UserName == User.Identity.Name
                              join
                                  d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                              where EntityFunctions.TruncateTime(d.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                              select new { c, d }).Count();


                weekstartcnt += (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                 where EntityFunctions.TruncateTime(d.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && d.O_Status == "Join"
                                 select new { c, d }).Count();


                chmweek.Add(new columnchart_model("Submission", weeksubcnt));
                chmweek.Add(new columnchart_model("Interview", weekintcnt));
                chmweek.Add(new columnchart_model("Offer", weekoffcnt));
                chmweek.Add(new columnchart_model("Start", weekstartcnt));
                ViewBag.thisweekChartdata = JsonConvert.SerializeObject(chmweek);

                List<columnchart_model> chmmonth = new List<columnchart_model>();
                int monthsubcnt = 0, monthintcnt = 0, monthoffcnt = 0, monthstartcnt = 0;


                monthsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                               where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                               select new { c, d }
                          ).Count();

                monthintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                               where
                                    f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, f }).Count();

                monthoffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                               where d.O_Off_Date.Value.Month == System.DateTime.Now.Month && d.O_Off_Date.Value.Year == System.DateTime.Now.Year
                               select new { c, d }).Count();


                monthstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                 where d.O_Join_Date.Value.Month == System.DateTime.Now.Month && d.O_Join_Date.Value.Year == System.DateTime.Now.Year && d.O_Status == "Join"
                                 select new { c, d }).Count();

                chmmonth.Add(new columnchart_model("Submission", monthsubcnt));
                chmmonth.Add(new columnchart_model("Interview", monthintcnt));
                chmmonth.Add(new columnchart_model("Offer", monthoffcnt));
                chmmonth.Add(new columnchart_model("Start", monthstartcnt));
                ViewBag.monthChartdata = JsonConvert.SerializeObject(chmmonth);

                List<columnchart_model> chm = new List<columnchart_model>();
                int tdsubcnt = 0, tdintcnt = 0, tdoffcnt = 0, tdstartcnt = 0;


                tdsubcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                            where d.R_Status == "Accept"
                            select new { c, d }
                          ).Count();

                tdintcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                            where
                                 (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                            select new { c, f }).Count();

                tdoffcnt = (from c in context.User_Master
                            where c.E_UserName == User.Identity.Name
                            join
                                d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                            select new { c, d }).Count();


                tdstartcnt = (from c in context.User_Master
                              where c.E_UserName == User.Identity.Name
                              join
                                  d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                              where d.O_Status == "Join"
                              select new { c, d }).Count();



                chm.Add(new columnchart_model("Submission", tdsubcnt));
                chm.Add(new columnchart_model("Interview", tdintcnt));
                chm.Add(new columnchart_model("Offer", tdoffcnt));
                chm.Add(new columnchart_model("Start", tdstartcnt));
                ViewBag.Chartdata = JsonConvert.SerializeObject(chm);



            }

            return View();
        }
        // Dashboard End

        public JsonResult getclientcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int clientcnt = 0;
                if (User.IsInRole("Admin"))
                {
                    clientcnt = context.Client_Master.Where(c => c.C_Is_Active == 1 && c.C_Is_Delete == 0).ToList().Count();

                }
                if (User.IsInRole("Sales"))
                {
                    clientcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Client_Master on d.SC_CID equals e.C_id
                                 select new { c, d, e }).ToList().Count();

                    //clientcnt = (from c in context.Client_Master
                    //             where c.C_Is_Active == 1 && c.C_Is_Delete == 0
                    //             join
                    //                 d in context.SalesClient_Master on c.C_id equals d.SC_CID
                    //             join
                    //                 e in context.User_Master on d.SC_UID equals e.E_Id
                    //             where e.E_UserName == User.Identity.Name
                    //             select new { c, d, e }).ToList().Count();
                    // clientcnt = context.Client_Master.Where(c => c.C_Is_Active == 1 && c.C_Is_Delete == 0).ToList().Count();

                }
                return Json(new { clientcnt }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getusercount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int usercnt = context.User_Master.Where(c => c.E_Is_Active == 1 && c.E_Is_Delete == 0).ToList().Count();
                return Json(new { usercnt }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getactiverequirementcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Recruiter"))
                {
                    var reqcont = context.Requirement_Master.ToList().Count();
                    return Json(new { reqcont }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    int tt = 1;
                    int ff = 0;
                    var reqcont = (from c in context.Requirement_Master join d in context.Client_Master on c.J_Client_Id equals d.C_id where (c.J_Is_Active == tt) && (c.J_Is_Delete == ff) join e in context.SalesClient_Master on d.C_id equals e.SC_CID join f in context.User_Master on e.SC_UID equals f.E_Code where f.E_UserName == User.Identity.Name select new { c, d, e, f }).ToList().OrderBy(m => m.c.J_Id).Count();
                    return Json(new { reqcont }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var reqcont = context.Requirement_Master.ToList().Count();
                    return Json(new { reqcont }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        public JsonResult getonlyactiverequirementcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var actreqcont = context.Requirement_Master.Where(c => c.J_Status == "Active").ToList().Count();
                return Json(new { actreqcont }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getnotactiverequirementcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var deactreqcont = context.Requirement_Master.Where(c => c.J_Status != "Active").ToList().Count();
                return Json(new { deactreqcont }, JsonRequestBehavior.AllowGet);
            }
        }


        //Month Count Code Start
        public JsonResult getmonthsubmissioncount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int msubcnt = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    msubcnt = context.Resume_Master.Where((c => c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept")).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    msubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_CreateOn.Value.Month == System.DateTime.Now.Month && f.R_CreateOn.Value.Year == System.DateTime.Now.Year && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        msubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();
                    }
                    //  return Json(new { msubcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept" select c).Count();
                    //context.Resume_Master.Where((c => c.R_CreateBy == User.Identity.Name && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept")).ToList().Count();
                }

                if (msubcnt > 0)
                {
                    val = msubcnt.ToString("0");
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult getmonthinterviewcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int mintcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || (User.IsInRole("Manager")))
                {
                    mintcnt = context.Interview_Master.Where((c => c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    mintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        mintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                        f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();
                    }
                }
                else
                {
                    mintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }

                if (mintcnt > 0)
                {
                    val = mintcnt.ToString();
                }
                else
                {
                    val = "-";
                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult getmonthoffercount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int moffcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    moffcnt = context.Offer_Master.Where((c => c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year)).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    moffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where f.O_Off_Date.Value.Month == System.DateTime.Now.Month && f.O_Off_Date.Value.Year == System.DateTime.Now.Year
                               select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        moffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where d.O_Off_Date.Value.Month == System.DateTime.Now.Month && d.O_Off_Date.Value.Year == System.DateTime.Now.Year
                                    select new { c, d }).Count();
                    }
                    //  return Json(new { moffcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    moffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year)).ToList().Count();
                }
                if (moffcnt > 0)
                {
                    val = moffcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getmonthstartcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int mstartcnt = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    mstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    mstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where f.O_Join_Date.Value.Month == System.DateTime.Now.Month && f.O_Join_Date.Value.Year == System.DateTime.Now.Year && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        mstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where d.O_Join_Date.Value.Month == System.DateTime.Now.Month && d.O_Join_Date.Value.Year == System.DateTime.Now.Year && d.O_Status == "Join"
                                      select new { c, d }).Count();
                    }
                    // return Json(new { mstartcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    mstartcnt = (from c in context.Offer_Master where c.O_Recruiter == User.Identity.Name && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "Join" select c).Count();
                }
                if (mstartcnt > 0)
                {
                    val = mstartcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getmonthBDcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int mbdcnt = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    mbdcnt = (from c in context.Offer_Master
                              where c.O_Status == "BD" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                              join
                                  d in context.Resume_Master on c.O_Rid equals d.R_Id
                              join
                                  e in context.Requirement_Master on d.R_Jid equals e.J_Id
                              join
                                  f in context.Client_Master on e.J_Client_Id equals f.C_id
                              join
                                  g in context.User_Master on c.O_Recruiter equals g.E_UserName
                              select new { c, d, e, f, g }).Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    mbdcnt = (from c in context.User_Master
                              where c.E_UserName == User.Identity.Name
                              join
                                  d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                              join
                                  e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                              join
                                  f in context.Offer_Master on e.J_Id equals f.O_Jid
                              where f.O_Join_Date.Value.Month == System.DateTime.Now.Month && f.O_Join_Date.Value.Year == System.DateTime.Now.Year && f.O_Status == "BD"
                              select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        mbdcnt += (from c in context.User_Master
                                   where c.E_Fullname == item
                                   join
                                       d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                   where d.O_Join_Date.Value.Month == System.DateTime.Now.Month && d.O_Join_Date.Value.Year == System.DateTime.Now.Year && d.O_Status == "BD"
                                   select new { c, d }).Count();
                    }
                    // return Json(new { mstartcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    mbdcnt = (from c in context.Offer_Master where c.O_Recruiter == User.Identity.Name && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "BD" select c).Count();
                }
                if (mbdcnt > 0)
                {
                    val = mbdcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        //Month count code end

        //Today count code start

        public JsonResult gettodaysubmissioncount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tsubcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();
                    }
                    //  return Json(new { tsubcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.R_Status == "Accept" select c).Count();
                    //context.Resume_Master.Where((c => c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();
                }

                if (tsubcnt > 0)
                {
                    val = tsubcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult gettodayinterviewcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tintcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    tintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();
                    }
                    //  return Json(new { tintcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }

                if (tintcnt > 0)
                {
                    val = tintcnt.ToString();
                }
                else
                {
                    val = "-";
                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gettodayoffercount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int toffcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    toffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    toffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        toffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                    select new { c, d }).Count();
                    }
                    //return Json(new { toffcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    toffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();
                }

                if (toffcnt > 0)
                {
                    val = toffcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gettodaystartcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tstartcnt = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();

                }
                else if (User.IsInRole("Sales"))
                {
                    tstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.O_Status == "Join"
                                      select new { c, d }).Count();
                    }
                    // return Json(new { tstartcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.O_Status == "Join")).ToList().Count();
                }

                if (tstartcnt > 0)
                {
                    val = tstartcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gettodayBDcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tbdcnt = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tbdcnt = (from c in context.Offer_Master
                              where c.O_Status == "BD" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                              join
                                  d in context.Resume_Master on c.O_Rid equals d.R_Id
                              join
                                  e in context.Requirement_Master on d.R_Jid equals e.J_Id
                              join
                                  f in context.Client_Master on e.J_Client_Id equals f.C_id
                              join
                                  g in context.User_Master on c.O_Recruiter equals g.E_UserName
                              select new { c, d, e, f, g }).Count();
                }
                if (tbdcnt > 0)
                {
                    val = tbdcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }

        //Today count code end

        //Accept Reject Count
        public JsonResult getAcceptRejectCountTeam()
        {
            int rcnt = 0;
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');

                    foreach (var item in tm)
                    {
                        int cnt = (from c in context.Resume_Master
                                   where c.R_Status == "New"
                                   join
                                       d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                   where d.E_Fullname == item
                                   select new { c, d }
                                                                       ).ToList().Count();
                        rcnt = rcnt + cnt;
                    }
                }
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    rcnt = (from c in context.Resume_Master where c.R_Status == "New" join d in context.User_Master on c.R_CreateBy equals d.E_UserName select new { c, d }).ToList().Count();
                }
            }
            string val = "-";
            if (rcnt > 0)
            {
                val = rcnt.ToString();
            }
            else
            {
                val = "-";
            }
            return Json(new { val }, JsonRequestBehavior.AllowGet);
        }

        //View Pending Approval List
        [HttpGet]
        public ActionResult ViewPendingApprovalList()
        {
            return View();
        }
        public ActionResult LoadPendingApprovalData()
        {
            List<ResumeModel> remo = new List<ResumeModel>();
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');

                    foreach (var item in tm)
                    {
                        var cnt = (from c in context.Resume_Master
                                   where c.R_Status == "New"
                                   join
                                       d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                   where d.E_Fullname == item
                                   select new { c, d }
                                                                       ).ToList();
                        foreach (var item1 in cnt)
                        {
                            remo.Add(new ResumeModel
                            {
                                rid = item1.c.R_Id,
                                jid = item1.c.R_Jid,
                                rname = item1.c.R_Name,
                                rdob = Convert.ToDateTime(item1.c.R_DOB),
                                radd = item1.c.R_Address,
                                rcnt = item1.c.R_Cnt,
                                raltcnt = item1.c.R_Alt_Cnt,
                                remail = item1.c.R_Email,
                                rtotexp = item1.c.R_Tot_Exp,
                                rrelexp = item1.c.R_Rel_Exp,
                                rcom = item1.c.R_Cur_Com,
                                rdes = item1.c.R_Cur_Des,
                                rnp = item1.c.R_NP,
                                rcanjoin = item1.c.R_Canjoin,
                                rctc = item1.c.R_Cur_CTC,
                                rectc = item1.c.R_Exp_CTC,
                                rreason = item1.c.R_Reason,
                                rintoff = item1.c.R_Any_Int_Of,
                                redu = item1.c.R_Education,
                                rcloc = item1.c.R_Cur_Location,
                                rpcloc = item1.c.R_Pref_Location,
                                rnative = item1.c.R_Native,
                                rresumename = item1.c.R_Resumename,
                                rstatus = item1.c.R_Status,
                                rcreateby = item1.d.E_Fullname,
                                rrcreaateon = Convert.ToDateTime(item1.c.R_CreateOn)
                            });
                        }
                    }
                }
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    var cnt = (from c in context.Resume_Master where c.R_Status == "New" join d in context.User_Master on c.R_CreateBy equals d.E_UserName select new { c, d }).ToList();
                    foreach (var item1 in cnt)
                    {
                        remo.Add(new ResumeModel
                        {
                            rid = item1.c.R_Id,
                            jid = item1.c.R_Jid,
                            rname = item1.c.R_Name,
                            rdob = Convert.ToDateTime(item1.c.R_DOB),
                            radd = item1.c.R_Address,
                            rcnt = item1.c.R_Cnt,
                            raltcnt = item1.c.R_Alt_Cnt,
                            remail = item1.c.R_Email,
                            rtotexp = item1.c.R_Tot_Exp,
                            rrelexp = item1.c.R_Rel_Exp,
                            rcom = item1.c.R_Cur_Com,
                            rdes = item1.c.R_Cur_Des,
                            rnp = item1.c.R_NP,
                            rcanjoin = item1.c.R_Canjoin,
                            rctc = item1.c.R_Cur_CTC,
                            rectc = item1.c.R_Exp_CTC,
                            rreason = item1.c.R_Reason,
                            rintoff = item1.c.R_Any_Int_Of,
                            redu = item1.c.R_Education,
                            rcloc = item1.c.R_Cur_Location,
                            rpcloc = item1.c.R_Pref_Location,
                            rnative = item1.c.R_Native,
                            rresumename = item1.c.R_Resumename,
                            rstatus = item1.c.R_Status,
                            rcreateby = item1.d.E_Fullname,
                            rrcreaateon = Convert.ToDateTime(item1.c.R_CreateOn)
                        });
                    }
                }

            }

            return Json(new { data = remo }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CreateUser()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateUser(UserModel umm, HttpPostedFileBase userimage)
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
                        CreateActivityLog(User.Identity.Name, "Create User", System.DateTime.Now);
                        ModelState.Clear();
                        TempData["msg"] = "User created successfully.";
                        return View();
                    }
                    else
                    {
                        TempData["msg"] = "User already exists !";
                        return View();
                    }
                }
            }
            else
            {
                return View();
            }
        }



        [HttpGet]
        public ActionResult ViewUser()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var userlist = context.User_Master.Where(m => m.E_Is_Active == 1 && m.E_Is_Delete == 0).ToList().OrderBy(m => m.E_Code);
                List<UserModel> ulm = new List<UserModel>();
                foreach (var item in userlist)
                {
                    ulm.Add(new UserModel
                    {
                        EId = item.E_Id,
                        ECode = item.E_Code,
                        EFullname = item.E_Fullname,
                        EGender = item.E_Gender,
                        EEmail = item.E_Email,
                        EDOB = item.E_DOB,
                        EContNo = item.E_ContNo,
                        EEmergencyContNo = item.E_EEmergencyContNo,
                        EUserName = item.E_UserName,
                        EPassword = item.E_Password,
                        EAddress = item.E_Address,
                        ECAddress = item.E_CAddress,
                        EPostal = Convert.ToDecimal(item.E_Postal),
                        EEtype = item.E_Etype,
                        ECompany_Name = item.E_Company_Name,
                        EDepartment = item.E_Department,
                        EDesignation = item.E_Designation,
                        ELocation = item.E_Location,
                        EOfferDate = item.E_OfferDate,
                        EJoinDate = item.E_JoinDate,
                        ERole = item.E_Role,
                        EBloodGroup = item.E_BloodGroup,
                        EBankName = item.E_BankName,
                        EAccountNo = item.E_AccountNo,
                        EBranch = item.E_Branch,
                        EIFC_Code = item.E_IFC_Code,
                        EPFAccount = item.E_PF_Account,
                        EPAN_No = item.E_PAN_No,
                        EAadhar_no = item.E_Aadhar_no,
                        ESalary = item.E_Salary,
                        EPF = item.E_PF,
                        EPT = item.E_PT,
                        EESI = item.E_ESI,
                        EPFApply = item.E_PF_Apply.Value == 1 ? true : false,
                        EPTApply = item.E_PT_Apply.Value == 1 ? true : false,
                        EESIApply = item.E_ESI_Apply.Value == 1 ? true : false,
                        Ephoto = item.E_Photo,
                        eactive = Convert.ToInt32(item.E_Is_Active),
                        edelete = Convert.ToInt32(item.E_Is_Delete)
                    });
                }

                return View(ulm);
            }
        }

        public ActionResult Deleteuser(string ecode, string exitdt)
        {
            using (var context = new ATS2019_dbEntities())
            {
                decimal uid = Convert.ToDecimal(ecode);
                DateTime edt = Convert.ToDateTime(exitdt);
                var finduser = context.User_Master.Where(c => c.E_Code == uid).FirstOrDefault();
                finduser.E_Is_Delete = Convert.ToInt32("1");
                finduser.E_ExitDate = edt;
                finduser.E_Updated_By = User.Identity.Name;
                finduser.E_Updated_On = System.DateTime.Now;
                context.Entry(finduser).State = EntityState.Modified;
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Delete User", System.DateTime.Now);
                return RedirectToAction("ViewUser");
            }

        }
        public ActionResult Edituser(decimal? uid)
        {
            if (uid != null)
            {
                using (var context = new ATS2019_dbEntities())
                {
                    var finduser = context.User_Master.Where(c => c.E_Code == uid).FirstOrDefault();
                    UserModel umm = new UserModel();
                    umm.EId = finduser.E_Id;
                    umm.ECode = finduser.E_Code;
                    umm.EFullname = finduser.E_Fullname;
                    umm.EGender = finduser.E_Gender;
                    umm.EEmail = finduser.E_Email;
                    umm.EDOB = finduser.E_DOB;
                    umm.EContNo = finduser.E_ContNo;
                    umm.EEmergencyContNo = finduser.E_EEmergencyContNo;
                    umm.EUserName = finduser.E_UserName;
                    umm.EPassword = finduser.E_Password;
                    umm.EAddress = finduser.E_Address;
                    umm.ECAddress = finduser.E_CAddress;
                    umm.EPostal = Convert.ToDecimal(finduser.E_Postal);
                    umm.EEtype = finduser.E_Etype;
                    umm.ECompany_Name = finduser.E_Company_Name;
                    umm.EDepartment = finduser.E_Department;
                    umm.EDesignation = finduser.E_Designation;
                    umm.ELocation = finduser.E_Location;
                    umm.EOfferDate = finduser.E_OfferDate;
                    umm.EJoinDate = finduser.E_JoinDate;
                    umm.ERole = finduser.E_Role;
                    umm.EBloodGroup = finduser.E_BloodGroup;
                    umm.EBankName = finduser.E_BankName;
                    umm.EAccountNo = finduser.E_AccountNo;
                    umm.EBranch = finduser.E_Branch;
                    umm.EIFC_Code = finduser.E_IFC_Code;
                    umm.EPFAccount = finduser.E_PF_Account;
                    umm.EPAN_No = finduser.E_PAN_No;
                    umm.EAadhar_no = finduser.E_Aadhar_no;
                    umm.ESalary = finduser.E_Salary;
                    umm.EPF = finduser.E_PF;
                    umm.EPT = finduser.E_PT;
                    umm.EESI = finduser.E_ESI;
                    umm.EPFApply = finduser.E_PF_Apply.Value == 1 ? true : false;
                    umm.EPTApply = finduser.E_PT_Apply.Value == 1 ? true : false;
                    umm.EESIApply = finduser.E_ESI_Apply.Value == 1 ? true : false;
                    umm.Ephoto = finduser.E_Photo;
                    umm.eactive = Convert.ToInt32(finduser.E_Is_Active);
                    umm.edelete = Convert.ToInt32(finduser.E_Is_Delete);
                    return View(umm);
                }
            }
            else
            {
                return RedirectToAction("Viewuser");
            }
        }
        [HttpPost]
        public ActionResult Updateuser(UserModel umm, HttpPostedFileBase userimage)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var finduser = context.User_Master.Where(c => c.E_Id == umm.EId).FirstOrDefault();
                finduser.E_Code = umm.ECode;
                finduser.E_Fullname = umm.EFullname;
                finduser.E_Gender = umm.EGender;
                finduser.E_Email = umm.EEmail;
                finduser.E_DOB = umm.EDOB;
                finduser.E_ContNo = umm.EContNo;
                finduser.E_EEmergencyContNo = umm.EEmergencyContNo;
                finduser.E_UserName = umm.EUserName;
                finduser.E_Password = umm.EPassword;
                finduser.E_Address = umm.EAddress;
                finduser.E_CAddress = umm.ECAddress;
                finduser.E_Postal = umm.EPostal;
                finduser.E_Etype = umm.EEtype;
                finduser.E_Company_Name = umm.ECompany_Name;
                finduser.E_Department = umm.EDepartment;
                finduser.E_Designation = umm.EDesignation;
                finduser.E_Location = umm.ELocation;
                finduser.E_OfferDate = umm.EOfferDate;
                finduser.E_JoinDate = umm.EJoinDate;
                finduser.E_Role = umm.ERole;
                finduser.E_BloodGroup = umm.EBloodGroup;
                finduser.E_BankName = umm.EBankName;
                finduser.E_AccountNo = umm.EAccountNo;
                finduser.E_Branch = umm.EBranch;
                finduser.E_IFC_Code = umm.EIFC_Code;
                finduser.E_PF_Account = umm.EPFAccount;
                finduser.E_PAN_No = umm.EPAN_No;
                finduser.E_Aadhar_no = umm.EAadhar_no;
                finduser.E_Salary = umm.ESalary;
                finduser.E_PF = umm.EPF;
                finduser.E_PT = umm.EPT;
                finduser.E_ESI = umm.EESI;
                finduser.E_PF_Apply = umm.EPFApply == true ? Convert.ToInt32("1") : Convert.ToInt32("0");
                finduser.E_PT_Apply = umm.EPTApply == true ? Convert.ToInt32("1") : Convert.ToInt32("0");
                finduser.E_ESI_Apply = umm.EESIApply == true ? Convert.ToInt32("1") : Convert.ToInt32("0");
                finduser.E_Updated_By = "Username";
                finduser.E_Updated_On = System.DateTime.Now;
                if (userimage != null)
                {
                    FileInfo fi = new FileInfo(userimage.FileName);
                    string[] filedes = fi.Name.Split('.');
                    string userimagename = umm.ECode + "." + filedes[1].ToString();
                    finduser.E_Photo = userimagename;
                    System.IO.File.Delete(Server.MapPath("~/UserImage/") + userimagename);
                    userimage.SaveAs(Server.MapPath("~/UserImage/") + userimagename);
                }
                else
                {
                    finduser.E_Photo = umm.Ephoto;
                }
                context.Entry(finduser).State = EntityState.Modified;
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Update User", System.DateTime.Now);
                return RedirectToAction("Viewuser");
            }

        }
        [HttpGet]
        public ActionResult LoadData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var userlist = context.User_Master.Where(m => m.E_Is_Active == 1 && m.E_Is_Delete == 0).ToList().OrderBy(m => m.E_Code);
                return Json(new { data = userlist }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CreateUserRole()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var totuser = context.User_Master.Where(m => m.E_Is_Active == 1 && m.E_Is_Delete == 0).ToList().OrderBy(m => m.E_Code);
                List<SelectListItem> userlist = new List<SelectListItem>();
                foreach (var item in totuser)
                {
                    userlist.Add(new SelectListItem
                    {
                        Text = item.E_Code.ToString(),
                        Value = item.E_Fullname.ToString()

                    });
                }
                ViewBag.userlist = userlist.ToList();
                return View();
            }

        }
        [HttpPost]
        public ActionResult CreateUserRole(userrolemodel urm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                Boolean ifexists = context.Role_Master.Where(c => c.R_ecode == urm.uid && c.R_role == urm.role).Any();
                if (!ifexists)
                {
                    Role_Master rm = new Role_Master();
                    rm.R_ecode = urm.uid;
                    rm.R_role = urm.role.ToString();
                    context.Role_Master.Add(rm);
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Assign User Role", System.DateTime.Now);

                    //ModelState.Clear();
                    TempData["msg"] = "Role created.";
                    return RedirectToAction("CreateUserRole");
                }
                else
                {
                    TempData["msg"] = "Role already exists.";
                    return RedirectToAction("CreateUserRole");
                }
            }

        }

        [HttpGet]
        public ActionResult ViewUserRole()
        {
            return View();
        }

        public ActionResult LoadUserRoleData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var userroles = (from c in context.Role_Master join d in context.User_Master on c.R_ecode equals d.E_Code select new { c, d }).ToList();
                return Json(new { data = userroles }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Deleteuserrole(decimal uid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var finduserrole = context.Role_Master.Where(m => m.R_id == uid).FirstOrDefault();
                context.Entry(finduserrole).State = EntityState.Deleted;
                context.SaveChanges();
                return RedirectToAction("ViewUserRole");

            }
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(Changepasswordmodel cpm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var changepassword = context.User_Master.Where(c => c.E_UserName == cpm.username && c.E_Password == cpm.opwd).FirstOrDefault();
                if (changepassword != null)
                {
                    changepassword.E_Password = cpm.npwd;
                    context.Entry(changepassword).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Logout", "login");
        }

        //Client

        [HttpGet]
        public ActionResult CreateClient()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateClient(ClientModel cmm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                Boolean alreadyexists = context.Client_Master.Where(c => c.C_Name == cmm.cname).Any();
                if (!alreadyexists)
                {
                    Client_Master cm = new Client_Master();
                    cm.C_Name = cmm.cname;
                    cm.C_Address = cmm.caddress;
                    cm.C_Cont_Person1 = cmm.cperson1;
                    cm.C_Cont_Person1_Email = cmm.cemail1;
                    cm.C_Cont_Person1_No = cmm.ccnt1;
                    cm.C_Cont_Person2 = cmm.cperson2;
                    cm.C_Cont_Person2_Email = cmm.cemail2;
                    cm.C_Cont_Person2_No = cmm.ccnt2;
                    cm.C_Category = cmm.ccategory;
                    cm.C_Type = cmm.ctype;
                    cm.C_Segment = cmm.csegment;
                    cm.C_Margin_Type = cmm.cmargintype;
                    cm.C_Margin = cmm.cmargin;
                    cm.C_Is_Active = Convert.ToInt16("1");
                    cm.C_Is_Delete = Convert.ToInt16("0");
                    cm.C_Created_By = User.Identity.Name;
                    cm.C_Created_On = System.DateTime.Now;
                    context.Client_Master.Add(cm);
                    context.SaveChanges();
                    TempData["msg"] = "Client added successfully.";
                    CreateActivityLog(User.Identity.Name, "Create Client", System.DateTime.Now);
                    ModelState.Clear();
                    return View();
                }
                else
                {
                    TempData["msg"] = "Client already exists";
                    return View();
                }
            }
        }
        public ActionResult Viewclient()
        {
            return View();
        }
        public ActionResult LoadClientData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ClientModel> cmm = new List<ClientModel>();

                if (User.IsInRole("Admin"))
                {
                    var clientlist = context.Client_Master.Where(c => c.C_Is_Active == 1 && c.C_Is_Delete == 0).ToList().OrderBy(c => c.C_Name);
                    foreach (var item in clientlist)
                    {
                        cmm.Add(new ClientModel
                        {
                            cid = item.C_id,
                            cname = item.C_Name,
                            cperson1 = item.C_Cont_Person1,
                            cemail1 = item.C_Cont_Person1_Email,
                            ccnt1 = item.C_Cont_Person1_No,
                            ctype = item.C_Created_On.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = cmm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var clientlist = (from c in context.SalesClient_Master
                                      join
                                          d in context.User_Master on c.SC_UID equals d.E_Code
                                      where d.E_UserName == User.Identity.Name
                                      join
                                          e in context.Client_Master on c.SC_CID equals e.C_id
                                      select new { c, d, e }).ToList();

                    foreach (var item in clientlist)
                    {
                        cmm.Add(new ClientModel
                        {
                            cid = item.e.C_id,
                            cname = item.e.C_Name,
                            cperson1 = item.e.C_Cont_Person1,
                            cemail1 = item.e.C_Cont_Person1_Email,
                            ccnt1 = item.e.C_Cont_Person1_No,
                            ctype = item.e.C_Created_On.Value.ToShortDateString()
                        });
                    }

                    return Json(new { data = cmm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Editclient(decimal? cid)
        {
            if (cid != null)
            {
                using (var context = new ATS2019_dbEntities())
                {
                    var findclient = context.Client_Master.Where(c => c.C_id == cid).FirstOrDefault();
                    if (findclient != null)
                    {
                        ClientModel cm = new ClientModel();
                        cm.cid = findclient.C_id;
                        cm.cname = findclient.C_Name;
                        cm.caddress = findclient.C_Address;
                        cm.cperson1 = findclient.C_Cont_Person1;
                        cm.ccnt1 = findclient.C_Cont_Person1_No;
                        cm.cemail1 = findclient.C_Cont_Person1_Email;
                        cm.cperson2 = findclient.C_Cont_Person2;
                        cm.ccnt2 = findclient.C_Cont_Person2_No == null ? Convert.ToDecimal("") : Convert.ToDecimal(findclient.C_Cont_Person2_No);
                        cm.cemail2 = findclient.C_Cont_Person2_Email;
                        cm.ccategory = findclient.C_Category;
                        cm.ctype = findclient.C_Type;
                        cm.csegment = findclient.C_Segment;
                        cm.cmargintype = findclient.C_Margin_Type;


                        return View(cm);
                    }
                    else
                    {
                        return RedirectToAction("Viewclient");
                    }

                }
            }
            else
            {
                ViewBag.msg = "No Record found";
                return RedirectToAction("Viewclient");
            }
        }
        [HttpPost]
        public ActionResult UpdateClient(ClientModel cmm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findclient = context.Client_Master.Where(c => c.C_id == cmm.cid).FirstOrDefault();
                if (findclient != null)
                {
                    findclient.C_Name = cmm.cname;
                    findclient.C_Address = cmm.caddress;
                    findclient.C_Cont_Person1 = cmm.cperson1;
                    findclient.C_Cont_Person1_No = cmm.ccnt1;
                    findclient.C_Cont_Person1_Email = cmm.cemail1;
                    findclient.C_Cont_Person2 = cmm.cperson2;
                    findclient.C_Cont_Person2_No = cmm.ccnt2;
                    findclient.C_Cont_Person2_Email = cmm.cemail2;
                    findclient.C_Category = cmm.ccategory;
                    findclient.C_Type = cmm.ctype;
                    findclient.C_Segment = cmm.csegment;
                    findclient.C_Margin_Type = cmm.cmargintype;
                    findclient.C_Updated_By = User.Identity.Name;
                    findclient.C_Updated_On = System.DateTime.Now;
                    context.Entry(findclient).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Update Client", System.DateTime.Now);
                    ViewBag.msg = "Client Updated successfully.";
                    return RedirectToAction("Viewclient");
                }
                else
                {
                    ViewBag.msg = "Failed to update client";
                    return RedirectToAction("Viewclient");
                }
            }

        }
        public ActionResult DeleteClient(decimal? cid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findclient = context.Client_Master.Where(c => c.C_id == cid).FirstOrDefault();
                if (findclient != null)
                {
                    findclient.C_Is_Delete = Convert.ToInt16("1");
                    context.Entry(findclient).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Delete Client", System.DateTime.Now);
                    ViewBag.msg = "Client deleted successfully.";
                    return RedirectToAction("Viewclient");
                }
                else
                {
                    ViewBag.msg = "Failed to delete client.";
                    return RedirectToAction("Viewclient");
                }
            }
        }

        //Team Master
        [HttpGet]
        public ActionResult CreateTeam()
        {
            Alllist();
            Teamleadlist();
            return View();
        }
        //Teamlead list
        public void Teamleadlist()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int yes = 1, no = 0;

                var findteamlead = context.User_Master.Where(c => (c.E_Role == "Teamlead") && (c.E_Is_Active == yes) && (c.E_Is_Delete == no)).ToList();
                List<SelectListItem> tllist = new List<SelectListItem>();
                foreach (var item in findteamlead)
                {
                    tllist.Add(new SelectListItem
                    {
                        Text = item.E_Fullname,
                        Value = item.E_Fullname.ToString()
                    });
                }
                ViewBag.Teamleadlist = tllist.OrderBy(c => c.Text).ToList();
            }
        }
        //Teamlead and Recruiter List
        public void Alllist()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int yes = 1, no = 0;

                var findall = context.User_Master.Where(c => ((c.E_Role == "Teamlead") || (c.E_Role == "Recruiter")) && (c.E_Is_Active == yes) && (c.E_Is_Delete == no)).ToList();
                List<SelectListItem> alllist = new List<SelectListItem>();
                foreach (var item in findall)
                {
                    alllist.Add(new SelectListItem
                    {
                        Text = item.E_Fullname,
                        Value = item.E_Fullname.ToString()
                    });
                }
                ViewBag.alllist = alllist.OrderBy(c => c.Text).ToList();
            }
        }

        [HttpPost]
        public ActionResult CreateTeam(TeamMemberModel tmm, string[] alllist)
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (alllist != null)
                {
                    var userlist = "";
                    foreach (var item in alllist)
                    {
                        if (item.ToString() != "false")
                        {
                            if (userlist != "")
                            {
                                userlist += "," + item.ToString();
                            }
                            else
                            {
                                userlist += item.ToString();
                            }
                        }
                    }
                    Team_Master tm = new Team_Master();
                    tm.T_TeamLead = tmm.teamlead;
                    tm.T_TeamMember = userlist;
                    tm.T_CreatedBy = User.Identity.Name;
                    tm.T_CreatedOn = System.DateTime.Now;
                    context.Team_Master.Add(tm);
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Create Team", System.DateTime.Now);
                    TempData["msg"] = "Team Added Successfully.";
                }
                else
                {
                    TempData["msg"] = "Select TeamMember First !";
                }
                return RedirectToAction("CreateTeam");
            }
        }
        public ActionResult ViewTeam()
        {
            return View();
        }

        public ActionResult LoadTeamData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var teamlist = context.Team_Master.ToList();
                return Json(new { data = teamlist }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteTeam(decimal tid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findteam = context.Team_Master.Where(c => c.T_Id == tid).FirstOrDefault();
                context.Entry(findteam).State = EntityState.Deleted;
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Delete Team", System.DateTime.Now);
                ViewBag.msg = "Team record deleted successfully.";
                return RedirectToAction("ViewTeam");
            }
        }
        public ActionResult EditTeam(decimal tid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                Teamleadlist();
                var findteam = context.Team_Master.Where(c => c.T_Id == tid).FirstOrDefault();
                if (findteam != null)
                {
                    TeamMemberModel tmm = new TeamMemberModel();
                    tmm.tid = findteam.T_Id;
                    tmm.teamlead = findteam.T_TeamLead;

                    string[] tm = findteam.T_TeamMember.ToString().Split(',');
                    List<SelectListItem> ctm = new List<SelectListItem>();
                    foreach (var item in tm)
                    {
                        ctm.Add(new SelectListItem
                        {
                            Text = item.ToString(),
                            Value = item.ToString()
                        });
                    }
                    ViewBag.ctm = ctm;

                    List<SelectListItem> uctm = new List<SelectListItem>();
                    string[] allteamlist = context.User_Master.Where(d => ((d.E_Role == "Teamlead") || (d.E_Role == "Recruiter")) && (d.E_Is_Active == 1) && (d.E_Is_Delete == 0)).Select(d => d.E_Fullname).ToArray();

                    foreach (var item in allteamlist)
                    {
                        if (!tm.Contains(item))
                        {
                            uctm.Add(new SelectListItem
                            {
                                Text = item.ToString(),
                                Value = item.ToString()
                            });
                        }
                    }
                    ViewBag.uctm = uctm;
                    return View();
                }
                else
                {
                    return RedirectToAction("ViewTeam");
                }
            }
        }
        [HttpPost]
        public ActionResult EditTeam(TeamMemberModel tmm, string[] alllist)
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (alllist.Length > 0)
                {
                    var userlist = "";
                    foreach (var item in alllist)
                    {
                        if (item.ToString() != "false")
                        {
                            if (userlist != "")
                            {
                                userlist += "," + item.ToString();
                            }
                            else
                            {
                                userlist += item.ToString();
                            }
                        }
                    }

                    var findteamdetail = context.Team_Master.Where(c => c.T_Id == tmm.tid).FirstOrDefault();
                    if (findteamdetail != null)
                    {
                        findteamdetail.T_TeamLead = tmm.teamlead;
                        findteamdetail.T_TeamMember = userlist;
                        findteamdetail.T_UpdatedBy = User.Identity.Name;
                        findteamdetail.T_UpdatedOn = System.DateTime.Now;
                        context.Entry(findteamdetail).State = EntityState.Modified;
                        context.SaveChanges();
                        CreateActivityLog(User.Identity.Name, "Update Team", System.DateTime.Now);
                    }
                }
                return RedirectToAction("ViewTeam");
            }
        }
        //Requirement
        [HttpGet]
        public ActionResult CreateRequirement()
        {
            totalClientlist();
            Alllist();
            return View();
        }
        //client list
        public void totalClientlist()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<SelectListItem> clients = new List<SelectListItem>();
                var clientlists = context.Client_Master.Where(c => c.C_Is_Active == 1 && c.C_Is_Delete == 0).ToList().OrderBy(c => c.C_Name);
                foreach (var item in clientlists)
                {
                    clients.Add(new SelectListItem
                    {
                        Value = item.C_Name.ToString(),
                        Text = item.C_id.ToString()
                    });
                }
                ViewBag.clientlist = clients;
            }
        }
        public void SalesClientList()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<SelectListItem> clients = new List<SelectListItem>();
                var clientlists = (from c in context.SalesClient_Master
                                   join
                                       d in context.User_Master on c.SC_UID equals d.E_Code
                                   where d.E_UserName == User.Identity.Name
                                   join
                                       e in context.Client_Master on c.SC_CID equals e.C_id
                                   select new { c, d, e }).ToList();
                foreach (var item in clientlists)
                {
                    clients.Add(new SelectListItem
                    {
                        Value = item.e.C_Name.ToString(),
                        Text = item.e.C_id.ToString()
                    });
                }
                ViewBag.clientlist = clients;
            }
        }
        public JsonResult GenerateJobCode(string cid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                string jc = "";

                decimal comid = Convert.ToDecimal(cid);

                var ifclientexists = context.Requirement_Master.Where(c => c.J_Client_Id == comid).ToList();
                if (ifclientexists.Count() > 0)
                {
                    var nextjobcode = ifclientexists.Last();
                    string[] jcs = nextjobcode.J_Code.ToString().Split('-');
                    decimal nextnumber = Convert.ToDecimal(jcs[1].ToString()) + Convert.ToDecimal("1");
                    var jcc = jcs[0] + "-" + nextnumber.ToString();
                    jc = jcc.ToUpper();
                }
                else
                {
                    string companyname = context.Client_Master.Where(c => c.C_id == comid).Select(c => c.C_Name).First();
                    char[] comchar = companyname.ToCharArray();
                    for (int i = 0; i < comchar.Length; i++)
                    {
                        int ind = Convert.ToInt32("1") + i;
                        var jcc = comchar[0].ToString() + comchar[ind].ToString() + "-1";

                        Boolean ch1 = string.IsNullOrWhiteSpace(comchar[0].ToString());
                        Boolean ch2 = string.IsNullOrWhiteSpace(comchar[ind].ToString());
                        if (!ch1 && !ch2)
                        {
                            Boolean check = checkjobcodeexistsornot(jcc.ToUpper());
                            if (!check)
                            {
                                jc = jcc.ToUpper();
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
        }
        public Boolean checkjobcodeexistsornot(string jc)
        {
            using (var context = new ATS2019_dbEntities())
            {
                bool findjobcode = context.Requirement_Master.Where(c => c.J_Code == jc).Any();
                if (findjobcode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateRequirement(RequirementModel rmm, string[] assignlist)
        {
            using (var context = new ATS2019_dbEntities())
            {
                try
                {
                    string assignuser = "";
                    if (assignlist != null)
                    {
                        foreach (var item in assignlist)
                        {
                            if (item.ToString() != "false")
                            {
                                if (assignuser != "")
                                {
                                    assignuser += "," + item.ToString();
                                }
                                else
                                {
                                    assignuser += item.ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        assignuser = "UnAssign";
                    }
                    Requirement_Master rm = new Requirement_Master();
                    rm.J_Code = rmm.jobcode;
                    rm.J_Client_Id = rmm.jclientid;
                    rm.J_Skill = rmm.jskill;
                    rm.J_Position = rmm.jposition;
                    rm.J_Location = rmm.jlocation;
                    rm.J_EndClient = rmm.jendclient;
                    rm.J_AssignUser = assignuser;
                    rm.J_Tot_Max_Exp = rmm.jtotmax;
                    rm.J_Tot_Min_Exp = rmm.jtotmin;
                    rm.J_Rel_Max_Exp = rmm.jrelmax;
                    rm.J_Rel_Min_Exp = rmm.jrelmin;
                    rm.J_Bill_Rate = rmm.jbillrate;
                    rm.J_Pay_Rate = rmm.jpayrate;
                    rm.J_Category = rmm.jcategory;
                    rm.J_Type = rmm.jtype;
                    rm.J_Employment_Type = rmm.jemployementtyp;
                    rm.J_POC = rmm.jpoc;
                    rm.J_POC_No = 0;
                    rm.J_JD = rmm.jjd;
                    rm.J_Status = "Create";
                    rm.J_MandatorySkill = rmm.jmandatoryskill.ToString();
                    rm.J_Is_Active = Convert.ToInt16("1");
                    rm.J_Is_Delete = Convert.ToInt16("0");
                    rm.J_CreatedOn = System.DateTime.Now;
                    rm.J_CreatedBy = User.Identity.Name;
                    context.Requirement_Master.Add(rm);
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "New Requirement Created.", System.DateTime.Now);
                    TempData["msg"] = "Requirement added successfully.";
                }
                catch (DbEntityValidationException dbex)
                {
                    Exception raise = dbex;
                    foreach (var validationErrors in dbex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            // raise a new exception nesting  
                            // the current instance as InnerException  
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
            }
            return RedirectToAction("CreateRequirement");
        }
        [HttpGet]
        public ActionResult ViewDeactiveOrDeleteRequirement()
        {
            return View();
        }
        public ActionResult LoadDeactiveRequirementData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var reqlist = (from c in context.Requirement_Master join d in context.Client_Master on c.J_Client_Id equals d.C_id where (c.J_Is_Active == 1) && (c.J_Is_Delete == 0) && (c.J_Status != "Active") select new { c, d }).ToList().OrderBy(m => m.c.J_Id);
                return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ViewActiveRequirement()
        {
            return View();
        }
        public ActionResult LoadActiveRequirementData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var reqlist = (from c in context.Requirement_Master join d in context.Client_Master on c.J_Client_Id equals d.C_id where (c.J_Is_Active == 1) && (c.J_Is_Delete == 0) && (c.J_Status == "Active") select new { c, d }).ToList().OrderBy(m => m.c.J_Id);
                return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
            }
        }

        [OutputCache(Duration = 30)]
        [HttpGet]
        public ActionResult ViewRequirement()
        {
            return View();
        }

        public ActionResult LoadRequirementData()
        {
            int tt = 1;
            int ff = 0;
            if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Team lead") || User.IsInRole("DataEntry"))
            {
                using (var context = new ATS2019_dbEntities())
                {
                    var reqlist = (from c in context.Requirement_Master join d in context.Client_Master on c.J_Client_Id equals d.C_id where (c.J_Is_Active == tt) && (c.J_Is_Delete == ff) select new { c, d }).ToList().OrderByDescending(m => m.c.J_CreatedOn);
                    return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (User.IsInRole("Sales"))
            {
                using (var context = new ATS2019_dbEntities())
                {
                    var reqlist = (from c in context.Requirement_Master join d in context.Client_Master on c.J_Client_Id equals d.C_id where (c.J_Is_Active == tt) && (c.J_Is_Delete == ff) join e in context.SalesClient_Master on d.C_id equals e.SC_CID join f in context.User_Master on e.SC_UID equals f.E_Code where f.E_UserName == User.Identity.Name select new { c, d, e, f }).ToList().OrderByDescending(m => m.c.J_CreatedOn);
                    return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (User.IsInRole("Recruiter"))
            {
                using (var context = new ATS2019_dbEntities())
                {
                    var reqlist = (from c in context.Requirement_Master join d in context.Client_Master on c.J_Client_Id equals d.C_id where (c.J_Is_Active == tt) && (c.J_Is_Delete == ff) select new { c, d }).ToList().OrderByDescending(m => m.c.J_CreatedOn);
                    return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectToAction("Welcome");
            }
        }

        public ActionResult ViewRequirementDetail(decimal? jid)
        {
            if (jid != null)
            {
                using (var context = new ATS2019_dbEntities())
                {
                    RequirementModel rmm = new RequirementModel();
                    ModelState.Clear();
                    var findreq = (from c in context.Requirement_Master where c.J_Id == jid join d in context.Client_Master on c.J_Client_Id equals d.C_id select new { c, d }).FirstOrDefault();
                    rmm.jid = findreq.c.J_Id;
                    rmm.jobcode = findreq.c.J_Code;
                    rmm.jclientid = findreq.c.J_Client_Id;
                    rmm.jskill = findreq.c.J_Skill;
                    rmm.jposition = findreq.c.J_Position;
                    rmm.jlocation = findreq.c.J_Location;
                    rmm.jendclient = findreq.c.J_EndClient;
                    rmm.jassignuser = findreq.c.J_AssignUser;
                    rmm.jtotmin = float.Parse(findreq.c.J_Tot_Min_Exp.ToString());
                    rmm.jtotmax = float.Parse(findreq.c.J_Tot_Max_Exp.ToString());
                    rmm.jrelmin = float.Parse(findreq.c.J_Rel_Min_Exp.ToString());
                    rmm.jrelmax = float.Parse(findreq.c.J_Rel_Max_Exp.ToString());
                    rmm.jbillrate = Convert.ToDecimal(findreq.c.J_Bill_Rate);
                    rmm.jpayrate = Convert.ToDecimal(findreq.c.J_Pay_Rate);
                    rmm.jcategory = findreq.c.J_Category;
                    rmm.jtype = findreq.c.J_Type;
                    rmm.jemployementtyp = findreq.c.J_Employment_Type;
                    rmm.jpoc = findreq.c.J_POC;
                    rmm.jjd = findreq.c.J_JD;
                    rmm.jstatus = findreq.c.J_Status;
                    rmm.jmandatoryskill = findreq.c.J_MandatorySkill != null ? findreq.c.J_MandatorySkill : "-";
                    return View(rmm);

                }
            }
            else
            {
                return RedirectToAction("ViewRequirement");
            }
        }

        public JsonResult updaterequirementstatus(string rjid, string rstate)
        {
            using (var context = new ATS2019_dbEntities())
            {
                decimal jjid = Convert.ToDecimal(rjid);
                var UpdateReqStats = context.Requirement_Master.Where(c => c.J_Id == jjid).FirstOrDefault();
                if (UpdateReqStats != null)
                {
                    UpdateReqStats.J_Status = rstate;
                    if (rstate == "Delete")
                    {
                        UpdateReqStats.J_Is_Delete = Convert.ToInt16("0");
                    }
                    if (rstate == "Deactive")
                    {
                        UpdateReqStats.J_Is_Active = Convert.ToInt16("1");
                    }
                    context.Entry(UpdateReqStats).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Requirement Status Change", System.DateTime.Now);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult RequireementDetailsJD(decimal? jid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                RequirementModel rmm = new RequirementModel();
                ModelState.Clear();
                var findreq = (from c in context.Requirement_Master where c.J_Id == jid join d in context.Client_Master on c.J_Client_Id equals d.C_id select new { c, d }).FirstOrDefault();
                rmm.jid = findreq.c.J_Id;
                rmm.jobcode = findreq.c.J_Code;
                rmm.jclientid = findreq.c.J_Client_Id;
                rmm.jskill = findreq.c.J_Skill;
                rmm.jposition = findreq.c.J_Position;
                rmm.jlocation = findreq.c.J_Location;
                rmm.jendclient = findreq.c.J_EndClient;
                rmm.jassignuser = findreq.c.J_AssignUser;
                rmm.jtotmin = float.Parse(findreq.c.J_Tot_Min_Exp.ToString());
                rmm.jtotmax = float.Parse(findreq.c.J_Tot_Max_Exp.ToString());
                rmm.jrelmin = float.Parse(findreq.c.J_Rel_Min_Exp.ToString());
                rmm.jrelmax = float.Parse(findreq.c.J_Rel_Max_Exp.ToString());
                rmm.jbillrate = Convert.ToDecimal(findreq.c.J_Bill_Rate);
                rmm.jpayrate = Convert.ToDecimal(findreq.c.J_Pay_Rate);
                rmm.jcategory = findreq.c.J_Category;
                rmm.jtype = findreq.c.J_Type;
                rmm.jemployementtyp = findreq.c.J_Employment_Type;
                rmm.jpoc = findreq.c.J_POC;
                rmm.jjd = findreq.c.J_JD;
                rmm.jstatus = findreq.c.J_Status;
                return PartialView(rmm);
            }
        }


        public ActionResult checkrequirementstatus(decimal rid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var st = (from c in context.Requirement_Master where c.J_Id == rid select c.J_Status).FirstOrDefault();
                if (st == "Active")
                {
                    return Json(new { data = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ViewReusme()//decimal? jid)
        {
            return View();
        }
        public JsonResult LoadResumeData(string jid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                decimal jobid = Convert.ToDecimal(jid);
                var resumelist = (from c in context.Resume_Master
                                  where c.R_Jid == jobid
                                  join
                                      d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                  select new { c, d }).ToList().OrderByDescending(m => m.c.R_CreateOn);

                //context.Resume_Master.Where(c => c.R_Jid == jobid).ToList();
                return Json(new { data = resumelist }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AcceptResume(string resid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                decimal resumeid = Convert.ToDecimal(resid);
                var findresume = context.Resume_Master.Where(c => c.R_Id == resumeid).FirstOrDefault();
                if (findresume != null)
                {
                    findresume.R_Status = "Accept";
                    context.Entry(findresume).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Resume Accepted", System.DateTime.Now);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult RejectResume(string resid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                decimal resumeid = Convert.ToDecimal(resid);
                var findresume = context.Resume_Master.Where(c => c.R_Id == resumeid).FirstOrDefault();
                if (findresume != null)
                {
                    findresume.R_Status = "Reject";
                    context.Entry(findresume).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Resume Rejected", System.DateTime.Now);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult DeleteResume(decimal rid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findresume = context.Resume_Master.Where(c => c.R_Id == rid).FirstOrDefault();
                if (findresume != null)
                {
                    context.Entry(findresume).State = EntityState.Deleted;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Resume Deleted", System.DateTime.Now);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult InterviewwSchedule(decimal rrid, decimal jjid, string ccname, DateTime iidate, string iitime, string bby, string lloc, string nnt, string ssts)
        {
            using (var context = new ATS2019_dbEntities())
            {
                Interview_Master im = new Interview_Master();
                im.I_RId = rrid;
                im.I_JId = jjid;
                im.I_Date = iidate;
                im.I_Time = (TimeSpan.Parse(iitime)).Hours.ToString() + ":" + (TimeSpan.Parse(iitime)).Minutes.ToString();
                im.I_By = bby;
                im.I_Location = lloc;
                im.I_Note = nnt;
                im.I_Status = ssts;
                im.I_CreateBy = User.Identity.Name.ToString();
                im.I_CreateOn = System.DateTime.Now;
                context.Interview_Master.Add(im);
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Interview Scheduled", System.DateTime.Now);

                if (ssts == "Offer")
                {
                    var findintid = context.Interview_Master.Where(c => c.I_RId == rrid).Last();
                    if (findintid != null)
                    {
                        Boolean isofferadded = AddOffer(findintid.I_Id, findintid.I_RId, findintid.I_JId, ccname);
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult UpdateUploadResumeDate(decimal rid, DateTime rdt)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var query = context.Resume_Master.Where(m => m.R_Id == rid).FirstOrDefault();
                if (query != null)
                {
                    query.R_CreateOn = rdt;
                    query.R_updateby = User.Identity.Name;
                    query.R_updateon = System.DateTime.Now;
                    context.Entry(query).State = EntityState.Modified;
                    context.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }


        }
        protected Boolean AddOffer(decimal iid, decimal rid, decimal jid, string name)
        {
            using (var context = new ATS2019_dbEntities())
            {
                Offer_Master of = new Offer_Master();
                of.O_Iid = iid;
                of.O_Rid = rid;
                of.O_Jid = jid;
                of.O_Fullname = name.ToString();
                of.O_Status = "Offer";
                of.O_Recruiter = User.Identity.Name;
                of.O_Recuiter_Date = System.DateTime.Now;
                context.Offer_Master.Add(of);
                context.SaveChanges();
                return true;
            }
        }

        public ActionResult ViewInterview(decimal? jid)
        {
            return View();
        }

        public ActionResult LoadInterviewData(decimal? jid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var interviewlist = (from c in context.Interview_Master
                                     where c.I_JId == jid
                                     join
                                         d in context.Resume_Master on c.I_RId equals d.R_Id
                                     select new { c, d }).ToList();
                return Json(new { data = interviewlist }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateInterviewStatus(decimal iid, string sst)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findinterview = context.Interview_Master.Where(c => c.I_Id == iid).FirstOrDefault();
                if (findinterview != null)
                {
                    findinterview.I_Status = sst;
                    context.Entry(findinterview).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Interview Stats Updated", System.DateTime.Now);
                    if (sst == "Offer")
                    {
                        var findintid = context.Interview_Master.Where(c => c.I_Id == iid).FirstOrDefault();
                        if (findintid != null)
                        {
                            Boolean isofferadded = AddOffer(findintid.I_Id, findintid.I_RId, findintid.I_JId, "-");
                        }
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewOffer(decimal? jid)
        {
            return View();
        }

        public ActionResult LoadOfferData(decimal? jid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var offerlist = (from c in context.Offer_Master
                                 where c.O_Jid == jid
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                  e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                 f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 select new { c, d, e, f }).ToList();
                return Json(new { data = offerlist }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult UploadResume(HttpPostedFileBase closingblock, HttpPostedFileBase resume, decimal? jobid)
        {
            if (resume != null)
            {
                string[] rf = resume.FileName.Split('.');
                FileInfo fi = new FileInfo(resume.FileName.ToString());
                if (fi.Extension.ToLower() == ".docx")
                {
                    var resumename = rf[0].ToString() + "-" + jobid.ToString() + fi.Extension.ToString();
                    ResumeModel rm = new ResumeModel();
                    resume.SaveAs(Server.MapPath("~/ResumeDirectory/") + resumename);
                    string fileName = Server.MapPath("\\ResumeDirectory\\" + resumename);

                    using (WordprocessingDocument doc =
                        WordprocessingDocument.Open(fileName, true))
                    {
                        try
                        {
                            // Find the first table in the document.  
                            Table table =
                                doc.MainDocumentPart.Document.Body.Elements<Table>().First(); ;

                            // Find the second row in the table.  
                            TableRow row = table.Elements<TableRow>().ElementAt(1);
                            rm.rname = row.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the third row in the table
                            TableRow row1 = table.Elements<TableRow>().ElementAt(2);
                            rm.rcnt = row1.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            //Find the fourth row in the table
                            TableRow row2 = table.Elements<TableRow>().ElementAt(3);
                            rm.rfamilycnt = row2.Elements<TableCell>().ElementAt(1).InnerText.ToString().Trim();

                            //Find the fifth row in the table
                            TableRow row3 = table.Elements<TableRow>().ElementAt(4);
                            rm.rfriendcnt = row3.Elements<TableCell>().ElementAt(1).InnerText.ToString().Trim();

                            //Find the sixth row in the table
                            TableRow row4 = table.Elements<TableRow>().ElementAt(5);
                            rm.remail = row4.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the seventh row in the table
                            TableRow row5 = table.Elements<TableRow>().ElementAt(6);
                            rm.rtotexp = row5.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the eighth row in the table
                            TableRow row6 = table.Elements<TableRow>().ElementAt(7);
                            rm.rrelexp = row6.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the nineth row in the table
                            TableRow row7 = table.Elements<TableRow>().ElementAt(8);
                            rm.redu = row7.Elements<TableCell>().ElementAt(1).InnerText.ToString();


                            // Find the tenth row in the table
                            TableRow row8 = table.Elements<TableRow>().ElementAt(9);
                            rm.rreasongap = row8.Elements<TableCell>().ElementAt(1).InnerText.ToString();


                            // Find the eleven row in the table
                            TableRow row9 = table.Elements<TableRow>().ElementAt(10);
                            rm.rcom = row9.Elements<TableCell>().ElementAt(1).InnerText.ToString();


                            // Find the twelve row in the table
                            TableRow row10 = table.Elements<TableRow>().ElementAt(11);
                            rm.rdes = row10.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the thirteen row in the table
                            TableRow row11 = table.Elements<TableRow>().ElementAt(12);
                            rm.rctc = row11.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the fourteen row in the table
                            TableRow row12 = table.Elements<TableRow>().ElementAt(13);
                            rm.rcurtakehome = row12.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the fifteen row in the table
                            TableRow row13 = table.Elements<TableRow>().ElementAt(14);
                            rm.rcurdrawing = row13.Elements<TableCell>().ElementAt(1).InnerText.ToString();


                            // Find the sixteen row in the table
                            TableRow row14 = table.Elements<TableRow>().ElementAt(15);
                            rm.rlastsalaryhike = row14.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the seventeen row in the table
                            TableRow row15 = table.Elements<TableRow>().ElementAt(16);
                            rm.rectc = row15.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the eighteen row in the table
                            TableRow row16 = table.Elements<TableRow>().ElementAt(17);
                            rm.rexptakehome = row16.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the nineteen row in the table
                            TableRow row17 = table.Elements<TableRow>().ElementAt(18);
                            rm.rexpdrawing = row17.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twenty row in the table
                            TableRow row18 = table.Elements<TableRow>().ElementAt(19);
                            rm.rhikereason = row18.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentytwo row in the table
                            TableRow row20 = table.Elements<TableRow>().ElementAt(20);
                            rm.rnp = row20.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentythree row in the table
                            TableRow row21 = table.Elements<TableRow>().ElementAt(21);
                            rm.rcanjoin = row21.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentyfour row in the table
                            TableRow row22 = table.Elements<TableRow>().ElementAt(22);
                            rm.rhowearlyjoinreason = row22.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentyfive row in the table
                            TableRow row23 = table.Elements<TableRow>().ElementAt(23);
                            rm.rwhyjoinarche = row23.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentysix row in the table
                            TableRow row24 = table.Elements<TableRow>().ElementAt(24);
                            rm.rhavedocs = row24.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentyseven row in the table
                            TableRow row25 = table.Elements<TableRow>().ElementAt(25);
                            rm.rcloc = row25.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentyeight row in the table
                            TableRow row26 = table.Elements<TableRow>().ElementAt(26);
                            rm.rpcloc = row26.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the twentynine row in the table
                            TableRow row27 = table.Elements<TableRow>().ElementAt(27);
                            rm.rreasonforlocation = row27.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the thirty row in the table
                            TableRow row28 = table.Elements<TableRow>().ElementAt(28);
                            rm.rnative = row28.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the thirtyone row in the table
                            TableRow row29 = table.Elements<TableRow>().ElementAt(29);
                            string dd = row29.Elements<TableCell>().ElementAt(1).InnerText.ToString().Trim();
                            if ((dd == null) || (dd.Trim() == ""))
                            {
                                rm.rdob = null;
                            }
                            else
                            {
                                try
                                {
                                    if (dd.Contains("-"))
                                    {
                                        dd = dd.Replace("-", "/");
                                        string dd1 = " 00:00:00";
                                        dd = dd + dd1;
                                        CultureInfo provider = CultureInfo.InvariantCulture;
                                        provider = new CultureInfo("en-Us", true);
                                        string format = "dd/MM/yyyy HH:mm:ss";
                                        rm.rdob = DateTime.ParseExact(dd, format, provider);
                                    }
                                    else
                                    {
                                        string dd1 = " 00:00:00";
                                        dd = dd + dd1;

                                        rm.rdob = Convert.ToDateTime(dd.ToString());
                                    }
                                }
                                catch (FormatException fe)
                                {
                                    TempData["msg"] = fe.Message.ToString();
                                }

                            }

                            // Find the thirtythree row in the table
                            TableRow row31 = table.Elements<TableRow>().ElementAt(30);
                            rm.rpannumber = row31.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the thirtyfour row in the table
                            TableRow row32 = table.Elements<TableRow>().ElementAt(31);
                            rm.rtelephonictiming = row32.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the thirtyfive row in the table
                            TableRow row33 = table.Elements<TableRow>().ElementAt(32);
                            rm.rf2favailibilty = row33.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the thirtysix row in the table
                            TableRow row34 = table.Elements<TableRow>().ElementAt(33);
                            rm.rskills = row34.Elements<TableCell>().ElementAt(1).InnerText.ToString();

                            // Find the first paragraph in the table cell.  
                            //Paragraph parag = cell.Elements<Paragraph>().First();

                            // Find the first run in the paragraph.  
                            //Run run = parag.Elements<Run>().First();

                            // Set the text for the run.  
                            //Text text = run.Elements<Text>().First();
                            //text.Text = addedText;
                        }
                        catch (System.ArgumentOutOfRangeException exex)
                        {
                            TempData["msg"] = "Invalid closing block format.";
                        }

                        using (var context = new ATS2019_dbEntities())
                        {

                            try
                            {
                                var query = context.Resume_Master.Where(c => (c.R_Cnt == rm.rcnt.Trim().Substring(0, 10)) && (c.R_Email == rm.remail)).FirstOrDefault();
                                if (query == null)
                                {
                                    Resume_Master rmm = new Resume_Master();
                                    rmm.R_Jid = Convert.ToDecimal(jobid);
                                    rmm.R_Name = rm.rname;
                                    if ((rm.rdob != null))
                                    {
                                        rmm.R_DOB = Convert.ToDateTime(rm.rdob);
                                    }
                                    rmm.R_Cnt = rm.rcnt.Trim().ToString().Substring(0, 10);
                                    rmm.R_Alt_Cnt = rm.raltcnt != null ? rm.raltcnt.Trim().ToString().Substring(0, 10) : null;
                                    rmm.R_Email = rm.remail.ToString();
                                    rmm.R_Tot_Exp = rm.rtotexp;
                                    rmm.R_Rel_Exp = rm.rrelexp;
                                    rmm.R_Cur_Com = rm.rcom;
                                    rmm.R_Cur_Des = rm.rdes;
                                    rmm.R_NP = rm.rnp;
                                    rmm.R_Canjoin = rm.rcanjoin;
                                    rmm.R_Cur_CTC = rm.rctc;
                                    rmm.R_Exp_CTC = rm.rectc;
                                    rmm.R_Reason = rm.rreason;
                                    rmm.R_Any_Int_Of = rm.rintoff;
                                    rmm.R_Education = rm.redu;
                                    rmm.R_Cur_Location = rm.rcloc;
                                    rmm.R_Pref_Location = rm.rpcloc;
                                    rmm.R_Native = rm.rnative;
                                    rmm.R_Skills = rm.rskills;
                                    rmm.R_FamilyCnt = rm.rfamilycnt != "" ? rm.rfamilycnt.Trim().ToString().Substring(0, 10) : "";
                                    rmm.R_friendcnt = rm.rfriendcnt != "" ? rm.rfriendcnt.Trim().ToString().Substring(0, 10) : "";
                                    rmm.R_reasongap = rm.rreasongap;
                                    rmm.R_Currenttakehome = rm.rcurtakehome;
                                    rmm.R_Currentdrawing = rm.rcurdrawing;
                                    rmm.R_Lastsalaryhike = rm.rlastsalaryhike;
                                    rmm.R_Expectedtakehome = rm.rexptakehome;
                                    rmm.R_Expecteddrawing = rm.rexpdrawing;
                                    rmm.R_HikeReason = rm.rhikereason;
                                    rmm.R_Howjoinearlyreason = rm.rhowearlyjoinreason;
                                    rmm.R_WhyJoinArche = rm.rwhyjoinarche;
                                    rmm.R_HaveDocs = rm.rhavedocs;
                                    rmm.R_Reasonofrelocation = rm.rreasonforlocation;
                                    rmm.R_Pannumber = rm.rpannumber;
                                    rmm.R_Telephonicinttiming = rm.rtelephonictiming;
                                    rmm.R_F2Favailibility = rm.rf2favailibilty;
                                    rmm.R_Resumename = resumename;
                                    rmm.R_Status = "New";
                                    rmm.R_CreateBy = User.Identity.Name;
                                    rmm.R_CreateOn = System.DateTime.Now;
                                    context.Resume_Master.Add(rmm);
                                    context.SaveChanges();
                                    // resume.SaveAs(Server.MapPath("~/ResumeDirectory/") + resumename);
                                    doc.Close();
                                    TempData["msg"] = "Profile Uploaded Successfully.";
                                }
                                else
                                {
                                    TempData["msg"] = "Profile already exist.";
                                }
                            }
                            //catch (Exception dbex)
                            //{
                            //    TempData["msg"] = dbex.Message.ToString() + "\n Profile Not Uploaded. Please Try Again.";
                            //}
                            catch (DbEntityValidationException dbex)
                            {
                                Exception raise = dbex;
                                foreach (var validationErrors in dbex.EntityValidationErrors)
                                {
                                    foreach (var validationError in validationErrors.ValidationErrors)
                                    {
                                        string message = string.Format("{0}:{1}",
                                            validationErrors.Entry.Entity.ToString(),
                                            validationError.ErrorMessage);
                                        raise = new InvalidOperationException(message, raise);
                                    }
                                }
                                TempData["msg"] = raise.Message.ToString();
                                //throw raise;

                            }
                        }

                    }

                }
            }
            else
            {
                TempData["msg"] = "Select Resume.";
            }
            /*
            if ((closingblock != null) && (resume != null))
            {
                string[] cb = closingblock.FileName.Split('.');
                string[] rf = resume.FileName.Split('.');
                if ((cb[1].ToString().ToUpper() == "XLS") || (cb[1].ToString().ToUpper() == "XLSX"))
                {
                    if ((rf[1].ToString().ToUpper() == "DOC") || (rf[1].ToString().ToUpper() == "DOCX") || (rf[1].ToString().ToUpper() == "PDF"))
                    {

                        Stream stream = closingblock.InputStream;
                        IExcelDataReader reader = null;
                        if (closingblock.FileName.EndsWith(".xls"))
                        {
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        }
                        else if (closingblock.FileName.EndsWith(".xlsx"))
                        {
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        else
                        {
                            ModelState.AddModelError("File", "This file format is not supported");
                            return View();
                        }

                        //reader.IsFirstRowAsColumnNames = true;
                        DataSet result = reader.AsDataSet();
                        reader.Close();

                        string name = result.Tables[0].Rows[1][1].ToString();
                        string number = result.Tables[0].Rows[2][1].ToString();
                        string alternate = result.Tables[0].Rows[3][1].ToString();
                        string email = result.Tables[0].Rows[4][1].ToString();
                        string totexp = result.Tables[0].Rows[5][1].ToString();
                        string relexp = result.Tables[0].Rows[6][1].ToString();
                        string curcom = result.Tables[0].Rows[7][1].ToString();
                        string curdes = result.Tables[0].Rows[8][1].ToString();
                        string np = result.Tables[0].Rows[9][1].ToString();
                        string canjoin = result.Tables[0].Rows[10][1].ToString();
                        string cctc = result.Tables[0].Rows[11][1].ToString();
                        string ectc = result.Tables[0].Rows[12][1].ToString();
                        string reseon = result.Tables[0].Rows[13][1].ToString();
                        string offerint = result.Tables[0].Rows[14][1].ToString();
                        string prof = result.Tables[0].Rows[15][1].ToString();
                        string location = result.Tables[0].Rows[16][1].ToString();
                        string prelocation = result.Tables[0].Rows[17][1].ToString();
                        string native = result.Tables[0].Rows[18][1].ToString();
                        string aadharpan = result.Tables[0].Rows[19][1].ToString();
                        string dob = result.Tables[0].Rows[20][1].ToString();

                        string str = string.Empty;
                        using (var context = new ATS2019_dbEntities())
                        {
                            var resumename = rf[0].ToString() + "-" + jobid.ToString() + "." + rf[1].ToString();
                            try
                            {
                                Resume_Master rm = new Resume_Master();
                                rm.R_Jid = Convert.ToDecimal(jobid);
                                rm.R_Name = name;
                                if ((dob != null) && (dob != "n/a") && (dob != ""))
                                {
                                    rm.R_DOB = Convert.ToDateTime(dob);
                                }
                                rm.R_Cnt = number.Trim();
                                rm.R_Alt_Cnt = alternate != "" ? alternate.Trim().ToString().Substring(0, 10) : "";
                                rm.R_Email = email;
                                rm.R_Tot_Exp = totexp;
                                rm.R_Rel_Exp = relexp;
                                rm.R_Cur_Com = curcom;
                                rm.R_Cur_Des = curdes;
                                rm.R_NP = np;
                                rm.R_Canjoin = canjoin;
                                rm.R_Cur_CTC = cctc;
                                rm.R_Exp_CTC = ectc;
                                rm.R_Reason = reseon;
                                rm.R_Any_Int_Of = offerint;
                                rm.R_Education = prof;
                                rm.R_Cur_Location = location;
                                rm.R_Pref_Location = prelocation;
                                rm.R_Native = native;
                                rm.R_Resumename = resumename;
                                rm.R_Status = "New";
                                rm.R_CreateBy = User.Identity.Name;
                                rm.R_CreateOn = System.DateTime.Now;
                                context.Resume_Master.Add(rm);
                                context.SaveChanges();
                                resume.SaveAs(Server.MapPath("~/ResumeDirectory/") + resumename);
                                TempData["msg"] = "Profile Uploaded Successfully.";
                            }
                            catch (DbEntityValidationException dbex)
                            {
                                Exception raise = dbex;
                                foreach (var validationErrors in dbex.EntityValidationErrors)
                                {
                                    foreach (var validationError in validationErrors.ValidationErrors)
                                    {
                                        string message = string.Format("{0}:{1}",
                                            validationErrors.Entry.Entity.ToString(),
                                            validationError.ErrorMessage);
                                        raise = new InvalidOperationException(message, raise);
                                    }
                                }
                                throw raise;
                            }
                        }
                    }
                }
            }
            else
            {
                TempData["msg"] = "Select Closingblock and Resume.";
            }
            */
            return RedirectToAction("ViewReusme", new { jid = jobid });
        }
        [HttpPost]
        public ActionResult AddOfferCandidateDetails(decimal oid, string fn, DateTime dobdt, string gen, string marit, string cadd, string padd, string country, string email, string cdes, string odes, string skill, string totexp, string relexp, string mo, string alt, string pan, string client, string loc, float ctc, float ectc, float br, float pr, float gp, float gpm, DateTime sdt, DateTime odt, DateTime jdt, string emergency, string acc, string bank, string branch, string ifci, string aadhar, string stats, string emptype, string note)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findofferdata = context.Offer_Master.Where(c => c.O_Id == oid).FirstOrDefault();
                if (findofferdata != null)
                {
                    findofferdata.O_Fullname = fn;
                    findofferdata.O_DOB = dobdt;
                    findofferdata.O_Gender = gen;
                    findofferdata.O_MaritalStatus = marit;
                    findofferdata.O_Cur_Add = cadd;
                    findofferdata.O_Perm_Add = padd;
                    findofferdata.O_Country = country;
                    findofferdata.O_Email = email;
                    findofferdata.O_Cur_Des = cdes;
                    findofferdata.O_Off_Des = odes;
                    findofferdata.O_Skill = skill;
                    findofferdata.O_Tot_Exp = totexp;
                    findofferdata.O_Rel_Exp = relexp;
                    findofferdata.O_Mobile = mo;
                    findofferdata.O_Alt_Mobile = alt;
                    findofferdata.O_Pan = pan;
                    findofferdata.O_Client = client;
                    findofferdata.O_Location = loc;
                    findofferdata.O_CTC = Convert.ToDecimal(ctc);
                    findofferdata.O_ECTC = Convert.ToDecimal(ectc);
                    findofferdata.O_BR = Convert.ToDecimal(br);
                    findofferdata.O_PR = Convert.ToDecimal(pr);
                    findofferdata.O_GP = Convert.ToDecimal(gp);
                    findofferdata.O_GPM = Convert.ToDecimal(gpm);
                    findofferdata.O_Sel_Date = sdt;
                    findofferdata.O_Off_Date = odt;
                    findofferdata.O_Join_Date = jdt;
                    findofferdata.O_Emergency = emergency;
                    findofferdata.O_Account_No = acc;
                    findofferdata.O_Bank_Name = bank;
                    findofferdata.O_Branch = branch;
                    findofferdata.O_IFCI = ifci;
                    findofferdata.O_Aadhar = aadhar != "" ? Convert.ToDecimal(aadhar) : Convert.ToDecimal("0");
                    findofferdata.O_Status = stats;
                    findofferdata.O_Note = note;
                    findofferdata.O_Type = emptype;
                    findofferdata.O_Teamlead = User.Identity.Name;
                    findofferdata.O_Teamlead_Date = System.DateTime.Now;

                    context.Entry(findofferdata).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Add Offered Candidate Details", System.DateTime.Now);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult AddofferdetailsPerm(decimal oid, string fn, DateTime dobdt, string gen, string marit, string cadd, string padd, string country, string email, string cdes, string odes, string skill, string totexp, string relexp, string mo, string alt, string client, string loc, float ctc, float margin, float gp, DateTime sdt, DateTime odt, DateTime jdt, string emergency, string aadhar, string stats, string emptype, string note)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findofferdetails = context.Offer_Master.Where(o => o.O_Id == oid).FirstOrDefault();
                if (findofferdetails != null)
                {
                    findofferdetails.O_Fullname = fn;
                    findofferdetails.O_DOB = dobdt;
                    findofferdetails.O_Gender = gen;
                    findofferdetails.O_MaritalStatus = marit;
                    findofferdetails.O_Cur_Add = cadd;
                    findofferdetails.O_Perm_Add = padd;
                    findofferdetails.O_Country = country;
                    findofferdetails.O_Email = email;
                    findofferdetails.O_Cur_Des = cdes;
                    findofferdetails.O_Off_Des = odes;
                    findofferdetails.O_Skill = skill;
                    findofferdetails.O_Tot_Exp = totexp;
                    findofferdetails.O_Rel_Exp = relexp;
                    findofferdetails.O_Mobile = mo;
                    findofferdetails.O_Alt_Mobile = alt;
                    findofferdetails.O_Client = client;
                    findofferdetails.O_Location = loc;
                    findofferdetails.O_CTC = Convert.ToDecimal(ctc);
                    findofferdetails.O_GPM = Convert.ToDecimal(margin);
                    findofferdetails.O_GP = Convert.ToDecimal(gp);
                    findofferdetails.O_Sel_Date = sdt;
                    findofferdetails.O_Off_Date = odt;
                    findofferdetails.O_Join_Date = jdt;
                    findofferdetails.O_Emergency = emergency;
                    findofferdetails.O_Aadhar = aadhar != "" ? Convert.ToDecimal(aadhar) : Convert.ToDecimal("0");
                    findofferdetails.O_Type = emptype;
                    findofferdetails.O_Status = stats;
                    findofferdetails.O_Note = note;
                    findofferdetails.O_Teamlead = User.Identity.Name;
                    findofferdetails.O_Teamlead_Date = System.DateTime.Now;
                    context.Entry(findofferdetails).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Add Offered Candidate Details", System.DateTime.Now);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public ActionResult ViewOfferList()
        {
            return View();
        }

        public ActionResult LoadOfferDataList()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Admin"))
                {
                    var offerdatalist = (from c in context.Offer_Master
                                         where c.O_Status == "Offer"
                                         join
                                             d in context.Resume_Master on c.O_Rid equals d.R_Id
                                         join
                                             e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                         join
                                             f in context.Client_Master on e.J_Client_Id equals f.C_id
                                         join
                                             g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                         select new { c, d, e, f, g }).ToList();
                    return Json(new { data = offerdatalist }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var offerdatalist = (from c in context.Offer_Master
                                         where c.O_Status == "Offer"
                                         join
                                             d in context.Resume_Master on c.O_Rid equals d.R_Id
                                         join
                                             e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                         join
                                             f in context.Client_Master on e.J_Client_Id equals f.C_id
                                         join
                                             g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                         join
                                             h in context.SalesClient_Master on f.C_id equals h.SC_CID
                                         join
                                             i in context.User_Master on h.SC_UID equals i.E_Code
                                         where i.E_UserName == User.Identity.Name

                                         select new { c, d, e, f, g, h, i }).ToList();
                    return Json(new { data = offerdatalist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View("ViewOfferList");
                }
            }
        }
        [HttpGet]
        public ActionResult ViewToBeJoinList()
        {
            return View();
        }
        public ActionResult LoadTobejoinDataList()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ViewTobejoinModel> vtbjm = new List<ViewTobejoinModel>();
                if (User.IsInRole("Team Lead"))
                {

                    var leadname = (from c in context.User_Master where c.E_UserName == User.Identity.Name select c.E_Fullname).FirstOrDefault();
                    if (leadname != null)
                    {
                        var teammember = (from d in context.Team_Master where d.T_TeamLead == leadname select d.T_TeamMember).FirstOrDefault();
                        if (teammember != null)
                        {
                            string[] tmlist = teammember.ToString().Split(',');
                            foreach (var item in tmlist)
                            {
                                var offerlist = (from e in context.User_Master
                                                 where e.E_Fullname == item
                                                 join
                                                     f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                                 where f.O_Status == "To be join"
                                                 join
                                                     g in context.Resume_Master on f.O_Rid equals g.R_Id
                                                 join
                                                     h in context.Requirement_Master on f.O_Jid equals h.J_Id
                                                 join
                                                     j in context.Client_Master on h.J_Client_Id equals j.C_id

                                                 select new { e, f, g, h, j }).ToList();
                                foreach (var item1 in offerlist)
                                {
                                    vtbjm.Add(new ViewTobejoinModel
                                    {
                                        oid = item1.f.O_Id,
                                        name = item1.g.R_Name,
                                        client = item1.j.C_Name,
                                        seldate = Convert.ToDateTime(item1.f.O_Sel_Date),
                                        offdate = Convert.ToDateTime(item1.f.O_Off_Date),
                                        joindate = Convert.ToDateTime(item1.f.O_Join_Date),
                                        location = item1.h.J_Location,
                                        type = item1.h.J_Employment_Type,
                                        status = item1.f.O_Status,
                                        recruitername = item1.e.E_Fullname,
                                        skill = item1.h.J_Skill
                                    });
                                }
                            }
                        }
                    }
                    return Json(new { data = vtbjm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offerlist = (from e in context.User_Master
                                     join
                                         f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                     where f.O_Status == "To be join"
                                     join
                                         g in context.Resume_Master on f.O_Rid equals g.R_Id
                                     join
                                         h in context.Requirement_Master on f.O_Jid equals h.J_Id
                                     join
                                         j in context.Client_Master on h.J_Client_Id equals j.C_id

                                     select new { e, f, g, h, j }).ToList();
                    foreach (var item1 in offerlist)
                    {
                        vtbjm.Add(new ViewTobejoinModel
                        {
                            oid = item1.f.O_Id,
                            name = item1.g.R_Name,
                            client = item1.j.C_Name,
                            seldate = Convert.ToDateTime(item1.f.O_Sel_Date),
                            offdate = Convert.ToDateTime(item1.f.O_Off_Date),
                            joindate = Convert.ToDateTime(item1.f.O_Join_Date),
                            location = item1.h.J_Location,
                            type = item1.h.J_Employment_Type,
                            status = item1.f.O_Status,
                            recruitername = item1.e.E_Fullname,
                            skill = item1.h.J_Skill
                        });
                    }
                    return Json(new { data = vtbjm }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpGet]
        public ActionResult ViewJoinDataList()
        {
            return View();
        }
        public ActionResult LoadJoinDataList()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Admin"))
                {
                    var offerdatalist = (from c in context.Offer_Master
                                         where c.O_Status == "Join"
                                         join
                                             d in context.Resume_Master on c.O_Rid equals d.R_Id
                                         join
                                             e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                         join
                                             f in context.Client_Master on e.J_Client_Id equals f.C_id
                                         join
                                             g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                         select new { c, d, e, f, g }).ToList();
                    return Json(new { data = offerdatalist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offerdatalist = (from c in context.Offer_Master
                                         where c.O_Status == "Join"
                                         join
                                             d in context.Resume_Master on c.O_Rid equals d.R_Id
                                         join
                                             e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                         join
                                             f in context.Client_Master on e.J_Client_Id equals f.C_id
                                         join
                                             g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                         join
                                              h in context.SalesClient_Master on f.C_id equals h.SC_CID
                                         join
                                             i in context.User_Master on h.SC_UID equals i.E_Code
                                         where i.E_UserName == User.Identity.Name
                                         select new { c, d, e, f, g, h, i }).ToList();
                    return Json(new { data = offerdatalist }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpGet]
        public ActionResult ViewBDDataList()
        {
            return View();
        }
        public ActionResult LoadBDDataList()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Admin"))
                {
                    var offerdatalist = (from c in context.Offer_Master
                                         where (c.O_Status == "BD") || (c.O_Status == "Reject") || (c.O_Status == "Hold")
                                         join
                                             d in context.Resume_Master on c.O_Rid equals d.R_Id
                                         join
                                             e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                         join
                                             f in context.Client_Master on e.J_Client_Id equals f.C_id
                                         join
                                             g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                         select new { c, d, e, f, g }).ToList();
                    return Json(new { data = offerdatalist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offerdatalist = (from c in context.Offer_Master
                                         where (c.O_Status == "BD") || (c.O_Status == "Reject") || (c.O_Status == "Hold")
                                         join
                                             d in context.Resume_Master on c.O_Rid equals d.R_Id
                                         join
                                             e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                         join
                                             f in context.Client_Master on e.J_Client_Id equals f.C_id
                                         join
                                             g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                         join
                                             h in context.SalesClient_Master on f.C_id equals h.SC_CID
                                         join
                                             i in context.User_Master on h.SC_UID equals i.E_Code
                                         where i.E_UserName == User.Identity.Name
                                         select new { c, d, e, f, g, h, i }).ToList();
                    return Json(new { data = offerdatalist }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult ViewMonthSubmission()
        {
            return View();
        }
        public JsonResult LoadMonthSubmission()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<SubmissionViewModel> svm = new List<SubmissionViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var todaysubdata = (from c in context.Resume_Master
                                        where c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept"
                                        join
                                            d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                        join
                                            e in context.Requirement_Master on c.R_Jid equals e.J_Id
                                        join
                                            f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.c.R_Id,
                            name = item.c.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.c.R_Cnt,
                            email = item.c.R_Email,
                            totexp = item.c.R_Tot_Exp,
                            np = item.c.R_NP,
                            ctc = item.c.R_Cur_CTC,
                            recruiter = item.d.E_Fullname,
                            subdate = item.c.R_CreateOn.Value.ToShortDateString()
                        });
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                            d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                        join
                                            e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                        join
                                            f in context.Resume_Master on e.J_Id equals f.R_Jid
                                        where f.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.E_Created_On.Value.Year == System.DateTime.Now.Year && f.R_Status == "Accept"
                                        join
                                            g in context.User_Master on f.R_CreateBy equals g.E_UserName
                                        join
                                            h in context.Client_Master on e.J_Client_Id equals h.C_id
                                        select new { d, e, f, g, h }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.f.R_Id,
                            name = item.f.R_Name,
                            client = item.h.C_Name,
                            skill = item.e.J_Skill,
                            number = item.f.R_Cnt,
                            email = item.f.R_Email,
                            totexp = item.f.R_Tot_Exp,
                            np = item.f.R_NP,
                            ctc = item.f.R_Cur_CTC,
                            recruiter = item.g.E_Fullname,
                            subdate = item.f.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item2 in tm)
                    {
                        var todaysubdata = (from c in context.User_Master
                                            where c.E_Fullname == item2
                                            join
                                            d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                            where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                                            join
                                            e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                            join
                                            f in context.Client_Master on e.J_Client_Id equals f.C_id
                                            select new { c, d, e, f }
                                            ).ToList();
                        if (todaysubdata.Count > 0)
                        {
                            foreach (var item in todaysubdata)
                            {
                                svm.Add(new SubmissionViewModel
                                {
                                    id = item.d.R_Id,
                                    name = item.d.R_Name,
                                    client = item.f.C_Name,
                                    skill = item.e.J_Skill,
                                    number = item.d.R_Cnt,
                                    email = item.d.R_Email,
                                    totexp = item.d.R_Tot_Exp,
                                    np = item.d.R_NP,
                                    ctc = item.d.R_Cur_CTC,
                                    recruiter = item.c.E_Fullname,
                                    subdate = item.d.R_CreateOn.Value.ToShortDateString()
                                });
                            }
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                        where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                                        join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                        join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }
                                            ).ToList();

                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.d.R_Id,
                            name = item.d.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.d.R_Cnt,
                            email = item.d.R_Email,
                            totexp = item.d.R_Tot_Exp,
                            np = item.d.R_NP,
                            ctc = item.d.R_Cur_CTC,
                            recruiter = item.c.E_Fullname,
                            subdate = item.d.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Till date Submission
        public ActionResult ViewTillDateSubmission()
        {
            return View();
        }
        public JsonResult LoadTillDateSubmission()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<SubmissionViewModel> svm = new List<SubmissionViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var todaysubdata = (from c in context.Resume_Master
                                        where c.R_Status == "Accept"
                                        join
                                            d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                        join
                                            e in context.Requirement_Master on c.R_Jid equals e.J_Id
                                        join
                                            f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.c.R_Id,
                            name = item.c.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.c.R_Cnt,
                            email = item.c.R_Email,
                            totexp = item.c.R_Tot_Exp,
                            np = item.c.R_NP,
                            ctc = item.c.R_Cur_CTC,
                            recruiter = item.d.E_Fullname,
                            subdate = item.c.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                            d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                        join
                                            e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                        join
                                            f in context.Resume_Master on e.J_Id equals f.R_Jid
                                        where f.R_Status == "Accept"
                                        join
                                            g in context.User_Master on f.R_CreateBy equals g.E_UserName
                                        join
                                            h in context.Client_Master on e.J_Client_Id equals h.C_id
                                        select new { d, e, f, g, h }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.f.R_Id,
                            name = item.f.R_Name,
                            client = item.h.C_Name,
                            skill = item.e.J_Skill,
                            number = item.f.R_Cnt,
                            email = item.f.R_Email,
                            totexp = item.f.R_Tot_Exp,
                            np = item.f.R_NP,
                            ctc = item.f.R_Cur_CTC,
                            recruiter = item.g.E_Fullname,
                            subdate = item.f.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item2 in tm)
                    {
                        var todaysubdata = (from c in context.User_Master
                                            where c.E_Fullname == item2
                                            join
                                            d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                            where d.R_Status == "Accept"
                                            join
                                            e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                            join
                                            f in context.Client_Master on e.J_Client_Id equals f.C_id
                                            select new { c, d, e, f }
                                            ).ToList();
                        if (todaysubdata.Count > 0)
                        {
                            foreach (var item in todaysubdata)
                            {
                                svm.Add(new SubmissionViewModel
                                {
                                    id = item.d.R_Id,
                                    name = item.d.R_Name,
                                    client = item.f.C_Name,
                                    skill = item.e.J_Skill,
                                    number = item.d.R_Cnt,
                                    email = item.d.R_Email,
                                    totexp = item.d.R_Tot_Exp,
                                    np = item.d.R_NP,
                                    ctc = item.d.R_Cur_CTC,
                                    recruiter = item.c.E_Fullname,
                                    subdate = item.d.R_CreateOn.Value.ToShortDateString()
                                });
                            }
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                        where d.R_Status == "Accept"
                                        join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                        join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }
                                            ).ToList();

                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.d.R_Id,
                            name = item.d.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.d.R_Cnt,
                            email = item.d.R_Email,
                            totexp = item.d.R_Tot_Exp,
                            np = item.d.R_NP,
                            ctc = item.d.R_Cur_CTC,
                            recruiter = item.c.E_Fullname,
                            subdate = item.d.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult ViewMonthIntervieww()
        {
            return View();
        }

        public JsonResult LoadMonthInterview()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<InterviewViewModel> ivm = new List<InterviewViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var intdata = (from c in context.Interview_Master
                                   where c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName
                                   select new { c, d, e }
                        ).ToList();

                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var intdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Interview_Master on e.J_Id equals f.I_JId
                                   where f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   join
                                       g in context.Resume_Master on f.I_RId equals g.R_Id
                                   join
                                       h in context.User_Master on f.I_CreateBy equals h.E_UserName
                                   select new { c, d, e, f, g, h }).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.f.I_Id,
                            name = item2.g.R_Name,
                            idate = item2.f.I_Date.ToShortDateString(),
                            itime = item2.f.I_Time.ToString(),
                            location = item2.f.I_Location,
                            by = item2.f.I_By,
                            recruiter = item2.h.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todayintdata = (from c in context.User_Master
                                            where c.E_Fullname == item
                                            join
                                                d in context.Interview_Master on c.E_UserName equals d.I_CreateBy
                                            where d.I_Date.Month == System.DateTime.Now.Month && d.I_Date.Year == System.DateTime.Now.Year && (d.I_Status != "Set" && d.I_Status != "Re Schedule" && d.I_Status != "No Show")
                                            join
                                            e in context.Resume_Master on d.I_RId equals e.R_Id
                                            select new { c, d, e }).ToList();
                        foreach (var item2 in todayintdata)
                        {
                            ivm.Add(new InterviewViewModel
                            {
                                iid = item2.d.I_Id,
                                name = item2.e.R_Name,
                                idate = item2.d.I_Date.ToShortDateString(),
                                itime = item2.d.I_Time.ToString(),
                                location = item2.d.I_Location,
                                by = item2.d.I_By,
                                recruiter = item2.c.E_Fullname
                            });
                        }
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var intdata = (from c in context.Interview_Master
                                   where c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && c.I_CreateBy == User.Identity.Name && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName

                                   select new { c, d, e }
                        ).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Till Date interview
        public ActionResult ViewTillDateInterview()
        {
            return View();
        }
        public JsonResult LoadTillDateInterview()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<InterviewViewModel> ivm = new List<InterviewViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var intdata = (from c in context.Interview_Master
                                   where (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName
                                   select new { c, d, e }
                        ).ToList();

                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var intdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Interview_Master on e.J_Id equals f.I_JId
                                   where (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   join
                                       g in context.Resume_Master on f.I_RId equals g.R_Id
                                   join
                                       h in context.User_Master on f.I_CreateBy equals h.E_UserName
                                   select new { c, d, e, f, g, h }).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.f.I_Id,
                            name = item2.g.R_Name,
                            idate = item2.f.I_Date.ToShortDateString(),
                            itime = item2.f.I_Time.ToString(),
                            location = item2.f.I_Location,
                            by = item2.f.I_By,
                            recruiter = item2.h.E_Fullname
                        });
                    }

                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todayintdata = (from c in context.User_Master
                                            where c.E_Fullname == item
                                            join
                                                d in context.Interview_Master on c.E_UserName equals d.I_CreateBy
                                            where (d.I_Status != "Set" && d.I_Status != "Re Schedule" && d.I_Status != "No Show")
                                            join
                                            e in context.Resume_Master on d.I_RId equals e.R_Id
                                            select new { c, d, e }).ToList();
                        foreach (var item2 in todayintdata)
                        {
                            ivm.Add(new InterviewViewModel
                            {
                                iid = item2.d.I_Id,
                                name = item2.e.R_Name,
                                idate = item2.d.I_Date.ToShortDateString(),
                                itime = item2.d.I_Time.ToString(),
                                location = item2.d.I_Location,
                                by = item2.d.I_By,
                                recruiter = item2.c.E_Fullname
                            });
                        }
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var intdata = (from c in context.Interview_Master
                                   where c.I_CreateBy == User.Identity.Name && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName

                                   select new { c, d, e }
                        ).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult ViewMonthOffer()
        {
            return View();
        }
        public JsonResult LoadMonthOffer()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ViewOfferModel> ovm = new List<ViewOfferModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {

                    var offdata = (from c in context.Offer_Master
                                   where c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year
                                   join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                       g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()
                            });
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var offdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Offer_Master on e.J_Id equals f.O_Jid
                                   where f.O_Off_Date.Value.Month == System.DateTime.Now.Month && f.O_Off_Date.Value.Year == System.DateTime.Now.Year
                                   join
                                       g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                   join
                                       h in context.Resume_Master on f.O_Rid equals h.R_Id
                                   join
                                       i in context.Client_Master on e.J_Client_Id equals i.C_id
                                   select new { c, d, e, f, g, h, i }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.f.O_Id,
                                name = item1.h.R_Name,
                                client = item1.i.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status,
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString()
                            });
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var offdata = (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       where d.O_Off_Date.Value.Month == System.DateTime.Now.Month && d.O_Off_Date.Value.Year == System.DateTime.Now.Year
                                       join
                                           e in context.Resume_Master on d.O_Rid equals e.R_Id
                                       join
                                           f in context.Requirement_Master on d.O_Jid equals f.J_Id
                                       join
                                           g in context.Client_Master on f.J_Client_Id equals g.C_id

                                       select new { c, d, e, f, g }).ToList();
                        if (offdata.Count > 0)
                        {
                            foreach (var item1 in offdata)
                            {
                                ovm.Add(new ViewOfferModel
                                {
                                    oid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    location = item1.f.J_Location,
                                    skill = item1.f.J_Skill,
                                    type = item1.f.J_Employment_Type,
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status,
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString()
                                });
                            }
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offdata = (from c in context.Offer_Master
                                   where c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year && c.O_Recruiter == User.Identity.Name
                                   join
                                       d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                    e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()
                            });
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Till Date offer
        public ActionResult ViewTillDateOffer()
        {
            return View();
        }
        public JsonResult LoadTillDateOffer()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ViewOfferModel> ovm = new List<ViewOfferModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var offdata = (from c in context.Offer_Master

                                   join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                       g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            try
                            {
                                ovm.Add(new ViewOfferModel
                                {
                                    oid = item1.c.O_Id,
                                    name = item1.d.R_Name,
                                    client = item1.f.C_Name,
                                    location = item1.e.J_Location,
                                    skill = item1.e.J_Skill,
                                    type = item1.e.J_Employment_Type,
                                    recruiter = item1.g.E_Fullname,
                                    status = item1.c.O_Status,
                                    offdate = item1.c.O_Off_Date.Value.ToShortDateString()
                                });
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var offdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Offer_Master on e.J_Id equals f.O_Jid

                                   join
                                       g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                   join
                                       h in context.Resume_Master on f.O_Rid equals h.R_Id
                                   join
                                       i in context.Client_Master on e.J_Client_Id equals i.C_id
                                   select new { c, d, e, f, g, h, i }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            try
                            {
                                ovm.Add(new ViewOfferModel
                                {
                                    oid = item1.f.O_Id,
                                    name = item1.h.R_Name,
                                    client = item1.i.C_Name,
                                    location = item1.e.J_Location,
                                    skill = item1.e.J_Skill,
                                    type = item1.e.J_Employment_Type,
                                    recruiter = item1.g.E_Fullname,
                                    status = item1.f.O_Status,
                                    offdate = item1.f.O_Off_Date.Value.ToShortDateString()
                                });
                            }
                            catch (Exception exx)
                            {
                                continue;
                            }
                        }
                    }

                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var offdata = (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       join
                                           e in context.Resume_Master on d.O_Rid equals e.R_Id
                                       join
                                           f in context.Requirement_Master on d.O_Jid equals f.J_Id
                                       join
                                           g in context.Client_Master on f.J_Client_Id equals g.C_id

                                       select new { c, d, e, f, g }).ToList();
                        if (offdata.Count > 0)
                        {
                            foreach (var item1 in offdata)
                            {
                                try
                                {
                                    ovm.Add(new ViewOfferModel
                                    {
                                        oid = item1.d.O_Id,
                                        name = item1.e.R_Name,
                                        client = item1.g.C_Name,
                                        location = item1.f.J_Location,
                                        skill = item1.f.J_Skill,
                                        type = item1.f.J_Employment_Type,
                                        recruiter = item1.c.E_Fullname,
                                        status = item1.d.O_Status,
                                        offdate = item1.d.O_Off_Date.Value.ToShortDateString()
                                    });
                                }
                                catch (Exception eex)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offdata = (from c in context.Offer_Master
                                   where c.O_Recruiter == User.Identity.Name
                                   join
                                       d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                    e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            try
                            {
                                ovm.Add(new ViewOfferModel
                                {
                                    oid = item1.c.O_Id,
                                    name = item1.d.R_Name,
                                    client = item1.f.C_Name,
                                    location = item1.e.J_Location,
                                    skill = item1.e.J_Skill,
                                    type = item1.e.J_Employment_Type,
                                    recruiter = item1.g.E_Fullname,
                                    status = item1.c.O_Status,
                                    offdate = item1.c.O_Off_Date.Value.ToShortDateString()
                                });
                            }
                            catch (Exception eexx)
                            {
                                continue;
                            }
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ViewMonthStart()
        {
            return View();
        }

        public JsonResult LoadMonthStart()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<StartViewModel> svm = new List<StartViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var joindata = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                    join
                                        e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                    join
                                        f in context.Offer_Master on e.J_Id equals f.O_Jid
                                    where f.O_Join_Date.Value.Month == System.DateTime.Now.Month && f.O_Join_Date.Value.Year == System.DateTime.Now.Year && f.O_Status == "Join"
                                    join
                                        g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                    join
                                        i in context.Resume_Master on f.O_Rid equals i.R_Id
                                    join
                                        j in context.Client_Master on e.J_Client_Id equals j.C_id
                                    select new { c, d, e, f, g, i, j }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.f.O_Id,
                                name = item1.i.R_Name,
                                client = item1.j.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.f.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.f.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status
                            });
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var joindata = (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                        where d.O_Join_Date.Value.Month == System.DateTime.Now.Month && d.O_Join_Date.Value.Year == System.DateTime.Now.Year && d.O_Status == "Join"
                                        join
                                        e in context.Resume_Master on d.O_Rid equals e.R_Id
                                        join
                                        f in context.Requirement_Master on e.R_Jid equals f.J_Id
                                        join
                                        g in context.Client_Master on f.J_Client_Id equals g.C_id
                                        select new { c, d, e, f, g }).ToList();
                        if (joindata.Count > 0)
                        {
                            foreach (var item1 in joindata)
                            {
                                svm.Add(new StartViewModel
                                {
                                    jid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    skill = item1.f.J_Skill,
                                    location = item1.f.J_Location,
                                    type = item1.f.J_Employment_Type,
                                    seldate = item1.d.O_Sel_Date.Value.ToShortDateString(),
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString(),
                                    joindate = item1.d.O_Join_Date.Value.ToShortDateString(),
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status
                                });
                            }
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    where c.O_Recruiter == User.Identity.Name
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Till Date Hire
        public ActionResult ViewTillDateStart()
        {
            return View();
        }
        public JsonResult LoadTillDateStart()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<StartViewModel> svm = new List<StartViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join"
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var joindata = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                    join
                                        e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                    join
                                        f in context.Offer_Master on e.J_Id equals f.O_Jid
                                    where f.O_Status == "Join"
                                    join
                                        g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                    join
                                        i in context.Resume_Master on f.O_Rid equals i.R_Id
                                    join
                                        j in context.Client_Master on e.J_Client_Id equals j.C_id
                                    select new { c, d, e, f, g, i, j }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.f.O_Id,
                                name = item1.i.R_Name,
                                client = item1.j.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.f.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.f.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status
                            });
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var joindata = (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                        where d.O_Status == "Join"
                                        join
                                        e in context.Resume_Master on d.O_Rid equals e.R_Id
                                        join
                                        f in context.Requirement_Master on e.R_Jid equals f.J_Id
                                        join
                                        g in context.Client_Master on f.J_Client_Id equals g.C_id
                                        select new { c, d, e, f, g }).ToList();
                        if (joindata.Count > 0)
                        {
                            foreach (var item1 in joindata)
                            {
                                svm.Add(new StartViewModel
                                {
                                    jid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    skill = item1.f.J_Skill,
                                    location = item1.f.J_Location,
                                    type = item1.f.J_Employment_Type,
                                    seldate = item1.d.O_Sel_Date.Value.ToShortDateString(),
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString(),
                                    joindate = item1.d.O_Join_Date.Value.ToShortDateString(),
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status
                                });
                            }
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join"
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    where c.O_Recruiter == User.Identity.Name
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ViewTodaySubmission()
        {
            return View();
        }
        public ActionResult ViewSalesTodaySubmission()
        {
            return View();
        }
        public JsonResult LoadTodaySubmission()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<SubmissionViewModel> svm = new List<SubmissionViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var todaysubdata = (from c in context.Resume_Master
                                        where EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.R_Status == "Accept"
                                        join
                                            d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                        join
                                            e in context.Requirement_Master on c.R_Jid equals e.J_Id
                                        join
                                            f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.c.R_Id,
                            name = item.c.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.c.R_Cnt,
                            email = item.c.R_Email,
                            totexp = item.c.R_Tot_Exp,
                            np = item.c.R_NP,
                            ctc = item.c.R_Cur_CTC,
                            recruiter = item.d.E_Fullname,
                            subdate = item.c.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                            d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                        join
                                            e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                        join
                                            f in context.Resume_Master on e.J_Id equals f.R_Jid
                                        where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.R_Status == "Accept"
                                        join
                                            g in context.User_Master on f.R_CreateBy equals g.E_UserName
                                        join
                                            h in context.Client_Master on e.J_Client_Id equals h.C_id
                                        select new { d, e, f, g, h }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.f.R_Id,
                            name = item.f.R_Name,
                            client = item.h.C_Name,
                            skill = item.e.J_Skill,
                            number = item.f.R_Cnt,
                            email = item.f.R_Email,
                            totexp = item.f.R_Tot_Exp,
                            np = item.f.R_NP,
                            ctc = item.f.R_Cur_CTC,
                            recruiter = item.g.E_Fullname,
                            subdate = item.f.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todaysudata = (from c in context.User_Master
                                           where c.E_Fullname == item
                                           join
                                           d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                           where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                                           join
                                           e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                           join
                                           f in context.Client_Master on e.J_Client_Id equals f.C_id
                                           select new { c, d, e, f }
                                            ).ToList();
                        foreach (var item1 in todaysudata)
                        {
                            svm.Add(new SubmissionViewModel
                            {
                                id = item1.d.R_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                number = item1.d.R_Cnt,
                                email = item1.d.R_Email,
                                totexp = item1.d.R_Tot_Exp,
                                np = item1.d.R_NP,
                                ctc = item1.d.R_Cur_CTC,
                                recruiter = item1.c.E_Fullname,
                                subdate = item1.d.R_CreateOn.Value.ToShortDateString()
                            });
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                        where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                        join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                        join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }
                                            ).ToList();

                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.d.R_Id,
                            name = item.d.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.d.R_Cnt,
                            email = item.d.R_Email,
                            totexp = item.d.R_Tot_Exp,
                            np = item.d.R_NP,
                            ctc = item.d.R_Cur_CTC,
                            recruiter = item.c.E_Fullname,
                            subdate = item.d.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //Yesterday Count Report
        public ActionResult ViewYesterdaySubmission()
        {
            return View();
        }
        public JsonResult LoadYesterdaySubmission()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                List<SubmissionViewModel> svm = new List<SubmissionViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var todaysubdata = (from c in context.Resume_Master
                                        where EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && c.R_Status == "Accept"
                                        join
                                            d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                        join
                                            e in context.Requirement_Master on c.R_Jid equals e.J_Id
                                        join
                                            f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.c.R_Id,
                            name = item.c.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.c.R_Cnt,
                            email = item.c.R_Email,
                            totexp = item.c.R_Tot_Exp,
                            np = item.c.R_NP,
                            ctc = item.c.R_Cur_CTC,
                            recruiter = item.d.E_Fullname,
                            subdate = item.c.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                            d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                        join
                                            e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                        join
                                            f in context.Resume_Master on e.J_Id equals f.R_Jid
                                        where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && f.R_Status == "Accept"
                                        join
                                            g in context.User_Master on f.R_CreateBy equals g.E_UserName
                                        join
                                            h in context.Client_Master on e.J_Client_Id equals h.C_id
                                        select new { d, e, f, g, h }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.f.R_Id,
                            name = item.f.R_Name,
                            client = item.h.C_Name,
                            skill = item.e.J_Skill,
                            number = item.f.R_Cnt,
                            email = item.f.R_Email,
                            totexp = item.f.R_Tot_Exp,
                            np = item.f.R_NP,
                            ctc = item.f.R_Cur_CTC,
                            recruiter = item.g.E_Fullname,
                            subdate = item.f.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todaysudata = (from c in context.User_Master
                                           where c.E_Fullname == item
                                           join
                                           d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                           where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                           join
                                           e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                           join
                                           f in context.Client_Master on e.J_Client_Id equals f.C_id
                                           select new { c, d, e, f }
                                            ).ToList();
                        foreach (var item1 in todaysudata)
                        {
                            svm.Add(new SubmissionViewModel
                            {
                                id = item1.d.R_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                number = item1.d.R_Cnt,
                                email = item1.d.R_Email,
                                totexp = item1.d.R_Tot_Exp,
                                np = item1.d.R_NP,
                                ctc = item1.d.R_Cur_CTC,
                                recruiter = item1.c.E_Fullname,
                                subdate = item1.d.R_CreateOn.Value.ToShortDateString()
                            });
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                        where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                        join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                        join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }
                                            ).ToList();

                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.d.R_Id,
                            name = item.d.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.d.R_Cnt,
                            email = item.d.R_Email,
                            totexp = item.d.R_Tot_Exp,
                            np = item.d.R_NP,
                            ctc = item.d.R_Cur_CTC,
                            recruiter = item.c.E_Fullname,
                            subdate = item.d.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //Thisweek submission
        public ActionResult ViewThisweekSubmission()
        {
            return View();
        }
        public JsonResult LoadThisweekSubmission()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                List<SubmissionViewModel> svm = new List<SubmissionViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var todaysubdata = (from c in context.Resume_Master
                                        where EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && c.R_Status == "Accept"
                                        join
                                            d in context.User_Master on c.R_CreateBy equals d.E_UserName
                                        join
                                            e in context.Requirement_Master on c.R_Jid equals e.J_Id
                                        join
                                            f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.c.R_Id,
                            name = item.c.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.c.R_Cnt,
                            email = item.c.R_Email,
                            totexp = item.c.R_Tot_Exp,
                            np = item.c.R_NP,
                            ctc = item.c.R_Cur_CTC,
                            recruiter = item.d.E_Fullname,
                            subdate = item.c.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                            d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                        join
                                            e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                        join
                                            f in context.Resume_Master on e.J_Id equals f.R_Jid
                                        where EntityFunctions.TruncateTime(f.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && f.R_Status == "Accept"
                                        join
                                            g in context.User_Master on f.R_CreateBy equals g.E_UserName
                                        join
                                            h in context.Client_Master on e.J_Client_Id equals h.C_id
                                        select new { d, e, f, g, h }).ToList();
                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.f.R_Id,
                            name = item.f.R_Name,
                            client = item.h.C_Name,
                            skill = item.e.J_Skill,
                            number = item.f.R_Cnt,
                            email = item.f.R_Email,
                            totexp = item.f.R_Tot_Exp,
                            np = item.f.R_NP,
                            ctc = item.f.R_Cur_CTC,
                            recruiter = item.g.E_Fullname,
                            subdate = item.f.R_CreateOn.Value.ToShortDateString()
                        });
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todaysudata = (from c in context.User_Master
                                           where c.E_Fullname == item
                                           join
                                           d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                           where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                                           join
                                           e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                           join
                                           f in context.Client_Master on e.J_Client_Id equals f.C_id
                                           select new { c, d, e, f }
                                            ).ToList();
                        foreach (var item1 in todaysudata)
                        {
                            svm.Add(new SubmissionViewModel
                            {
                                id = item1.d.R_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                number = item1.d.R_Cnt,
                                email = item1.d.R_Email,
                                totexp = item1.d.R_Tot_Exp,
                                np = item1.d.R_NP,
                                ctc = item1.d.R_Cur_CTC,
                                recruiter = item1.c.E_Fullname,
                                subdate = item1.d.R_CreateOn.Value.ToShortDateString()
                            });
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var todaysubdata = (from c in context.User_Master
                                        where c.E_UserName == User.Identity.Name
                                        join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                        where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                                        join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                        join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                        select new { c, d, e, f }
                                            ).ToList();

                    foreach (var item in todaysubdata)
                    {
                        svm.Add(new SubmissionViewModel
                        {
                            id = item.d.R_Id,
                            name = item.d.R_Name,
                            client = item.f.C_Name,
                            skill = item.e.J_Skill,
                            number = item.d.R_Cnt,
                            email = item.d.R_Email,
                            totexp = item.d.R_Tot_Exp,
                            np = item.d.R_NP,
                            ctc = item.d.R_Cur_CTC,
                            recruiter = item.c.E_Fullname,
                            subdate = item.d.R_CreateOn.Value.ToShortDateString()
                        });
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ViewTodayInterview()
        {
            return View();
        }
        public ActionResult ViewSalesTodayInterview()
        {
            return View();
        }
        public JsonResult LoadTodayInterview()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<InterviewViewModel> ivm = new List<InterviewViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var intdata = (from c in context.Interview_Master
                                   where EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName
                                   select new { c, d, e }
                        ).ToList();

                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }

                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var intdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Interview_Master on e.J_Id equals f.I_JId
                                   where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   join
                                       g in context.Resume_Master on f.I_RId equals g.R_Id
                                   join
                                       h in context.User_Master on f.I_CreateBy equals h.E_UserName
                                   select new { c, d, e, f, g, h }).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.f.I_Id,
                            name = item2.g.R_Name,
                            idate = item2.f.I_Date.ToShortDateString(),
                            itime = item2.f.I_Time.ToString(),
                            location = item2.f.I_Location,
                            by = item2.f.I_By,
                            recruiter = item2.h.E_Fullname
                        });
                    }

                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todayintdata = (from c in context.User_Master
                                            where c.E_Fullname == item
                                            join
                                                d in context.Interview_Master on c.E_UserName equals d.I_CreateBy
                                            where EntityFunctions.TruncateTime(d.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (d.I_Status != "Set" && d.I_Status != "Re Schedule" && d.I_Status != "No Show")
                                            join
                                            e in context.Resume_Master on d.I_RId equals e.R_Id
                                            select new { c, d, e }).ToList();
                        foreach (var item2 in todayintdata)
                        {
                            ivm.Add(new InterviewViewModel
                            {
                                iid = item2.d.I_Id,
                                name = item2.e.R_Name,
                                idate = item2.d.I_Date.ToShortDateString(),
                                itime = item2.d.I_Time.ToString(),
                                location = item2.d.I_Location,
                                by = item2.d.I_By,
                                recruiter = item2.c.E_Fullname
                            });
                        }
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var intdata = (from c in context.Interview_Master
                                   where EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.I_CreateBy == User.Identity.Name && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName

                                   select new { c, d, e }
                        ).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //view yesterday interview
        public ActionResult ViewYesterdayInterview()
        {
            return View();
        }
        public JsonResult LoadYesterdayInterview()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                List<InterviewViewModel> ivm = new List<InterviewViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var intdata = (from c in context.Interview_Master
                                   where EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName
                                   select new { c, d, e }
                        ).ToList();

                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }

                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var intdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Interview_Master on e.J_Id equals f.I_JId
                                   where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   join
                                       g in context.Resume_Master on f.I_RId equals g.R_Id
                                   join
                                       h in context.User_Master on f.I_CreateBy equals h.E_UserName
                                   select new { c, d, e, f, g, h }).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.f.I_Id,
                            name = item2.g.R_Name,
                            idate = item2.f.I_Date.ToShortDateString(),
                            itime = item2.f.I_Time.ToString(),
                            location = item2.f.I_Location,
                            by = item2.f.I_By,
                            recruiter = item2.h.E_Fullname
                        });
                    }

                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todayintdata = (from c in context.User_Master
                                            where c.E_Fullname == item
                                            join
                                                d in context.Interview_Master on c.E_UserName equals d.I_CreateBy
                                            where EntityFunctions.TruncateTime(d.I_Date) == EntityFunctions.TruncateTime(yesterday) && (d.I_Status != "Set" && d.I_Status != "Re Schedule" && d.I_Status != "No Show")
                                            join
                                            e in context.Resume_Master on d.I_RId equals e.R_Id
                                            select new { c, d, e }).ToList();
                        foreach (var item2 in todayintdata)
                        {
                            ivm.Add(new InterviewViewModel
                            {
                                iid = item2.d.I_Id,
                                name = item2.e.R_Name,
                                idate = item2.d.I_Date.ToShortDateString(),
                                itime = item2.d.I_Time.ToString(),
                                location = item2.d.I_Location,
                                by = item2.d.I_By,
                                recruiter = item2.c.E_Fullname
                            });
                        }
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var intdata = (from c in context.Interview_Master
                                   where EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && c.I_CreateBy == User.Identity.Name && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName

                                   select new { c, d, e }
                        ).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }

            }
        }
        //View Thisweek Interview
        public ActionResult ViewThisweekInterview()
        {
            return View();
        }
        public JsonResult LoadThisWeekInterview()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                List<InterviewViewModel> ivm = new List<InterviewViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var intdata = (from c in context.Interview_Master
                                   where EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName
                                   select new { c, d, e }
                        ).ToList();

                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }

                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var intdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Interview_Master on e.J_Id equals f.I_JId
                                   where EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                   join
                                       g in context.Resume_Master on f.I_RId equals g.R_Id
                                   join
                                       h in context.User_Master on f.I_CreateBy equals h.E_UserName
                                   select new { c, d, e, f, g, h }).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.f.I_Id,
                            name = item2.g.R_Name,
                            idate = item2.f.I_Date.ToShortDateString(),
                            itime = item2.f.I_Time.ToString(),
                            location = item2.f.I_Location,
                            by = item2.f.I_By,
                            recruiter = item2.h.E_Fullname
                        });
                    }

                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var todayintdata = (from c in context.User_Master
                                            where c.E_Fullname == item
                                            join
                                                d in context.Interview_Master on c.E_UserName equals d.I_CreateBy
                                            where EntityFunctions.TruncateTime(d.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (d.I_Status != "Set" && d.I_Status != "Re Schedule" && d.I_Status != "No Show")
                                            join
                                            e in context.Resume_Master on d.I_RId equals e.R_Id
                                            select new { c, d, e }).ToList();
                        foreach (var item2 in todayintdata)
                        {
                            ivm.Add(new InterviewViewModel
                            {
                                iid = item2.d.I_Id,
                                name = item2.e.R_Name,
                                idate = item2.d.I_Date.ToShortDateString(),
                                itime = item2.d.I_Time.ToString(),
                                location = item2.d.I_Location,
                                by = item2.d.I_By,
                                recruiter = item2.c.E_Fullname
                            });
                        }
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var intdata = (from c in context.Interview_Master
                                   where EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && c.I_CreateBy == User.Identity.Name && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")
                                   join
                                       d in context.Resume_Master on c.I_RId equals d.R_Id
                                   join
                                       e in context.User_Master on c.I_CreateBy equals e.E_UserName

                                   select new { c, d, e }
                        ).ToList();
                    foreach (var item2 in intdata)
                    {
                        ivm.Add(new InterviewViewModel
                        {
                            iid = item2.c.I_Id,
                            name = item2.d.R_Name,
                            idate = item2.c.I_Date.ToShortDateString(),
                            itime = item2.c.I_Time.ToString(),
                            location = item2.c.I_Location,
                            by = item2.c.I_By,
                            recruiter = item2.e.E_Fullname
                        });
                    }
                    return Json(new { data = ivm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ViewTodayOffer()
        {
            return View();
        }
        public ActionResult ViewSalesTodayOffer()
        {
            return View();
        }
        public JsonResult LoadTodayOffer()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ViewOfferModel> ovm = new List<ViewOfferModel>();
                if (User.IsInRole("Admin"))
                {

                    var offdata = (from c in context.Offer_Master
                                   where EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                   join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                       g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var offdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Offer_Master on e.J_Id equals f.O_Jid
                                   where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                   join
                                       g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                   join
                                       h in context.Resume_Master on f.O_Rid equals h.R_Id
                                   join
                                       i in context.Client_Master on e.J_Client_Id equals i.C_id
                                   select new { c, d, e, f, g, h, i }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.f.O_Id,
                                name = item1.h.R_Name,
                                client = item1.i.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status,
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }

                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var offdata = (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                       join
                                           e in context.Resume_Master on d.O_Rid equals e.R_Id
                                       join
                                           f in context.Requirement_Master on d.O_Jid equals f.J_Id
                                       join
                                           g in context.Client_Master on f.J_Client_Id equals g.C_id

                                       select new { c, d, e, f, g }).ToList();
                        if (offdata.Count > 0)
                        {
                            foreach (var item1 in offdata)
                            {
                                ovm.Add(new ViewOfferModel
                                {
                                    oid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    location = item1.f.J_Location,
                                    skill = item1.f.J_Skill,
                                    type = item1.f.J_Employment_Type,
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status,
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString()

                                });
                            }
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offdata = (from c in context.Offer_Master
                                   where EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                   join
                                       d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                    e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }


                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //view yesterday offer
        public ActionResult ViewYesterdayOffer()
        {
            return View();
        }
        public JsonResult LoadYesterdayOffer()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                List<ViewOfferModel> ovm = new List<ViewOfferModel>();
                if (User.IsInRole("Admin"))
                {

                    var offdata = (from c in context.Offer_Master
                                   where EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                   join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                       g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var offdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Offer_Master on e.J_Id equals f.O_Jid
                                   where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                   join
                                       g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                   join
                                       h in context.Resume_Master on f.O_Rid equals h.R_Id
                                   join
                                       i in context.Client_Master on e.J_Client_Id equals i.C_id
                                   select new { c, d, e, f, g, h, i }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.f.O_Id,
                                name = item1.h.R_Name,
                                client = item1.i.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status,
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }

                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var offdata = (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                       join
                                           e in context.Resume_Master on d.O_Rid equals e.R_Id
                                       join
                                           f in context.Requirement_Master on d.O_Jid equals f.J_Id
                                       join
                                           g in context.Client_Master on f.J_Client_Id equals g.C_id

                                       select new { c, d, e, f, g }).ToList();
                        if (offdata.Count > 0)
                        {
                            foreach (var item1 in offdata)
                            {
                                ovm.Add(new ViewOfferModel
                                {
                                    oid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    location = item1.f.J_Location,
                                    skill = item1.f.J_Skill,
                                    type = item1.f.J_Employment_Type,
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status,
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString()

                                });
                            }
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offdata = (from c in context.Offer_Master
                                   where EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                   join
                                       d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                    e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }


                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ViiewThisweekOffer()
        {
            return View();
        }
        public JsonResult LoadThisweekOffer()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                List<ViewOfferModel> ovm = new List<ViewOfferModel>();
                if (User.IsInRole("Admin"))
                {

                    var offdata = (from c in context.Offer_Master
                                   where EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                                   join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                       g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var offdata = (from c in context.User_Master
                                   where c.E_UserName == User.Identity.Name
                                   join
                                       d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                   join
                                       e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                   join
                                       f in context.Offer_Master on e.J_Id equals f.O_Jid
                                   where EntityFunctions.TruncateTime(f.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                                   join
                                       g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                   join
                                       h in context.Resume_Master on f.O_Rid equals h.R_Id
                                   join
                                       i in context.Client_Master on e.J_Client_Id equals i.C_id
                                   select new { c, d, e, f, g, h, i }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.f.O_Id,
                                name = item1.h.R_Name,
                                client = item1.i.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status,
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }

                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var offdata = (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       where EntityFunctions.TruncateTime(d.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                                       join
                                           e in context.Resume_Master on d.O_Rid equals e.R_Id
                                       join
                                           f in context.Requirement_Master on d.O_Jid equals f.J_Id
                                       join
                                           g in context.Client_Master on f.J_Client_Id equals g.C_id

                                       select new { c, d, e, f, g }).ToList();
                        if (offdata.Count > 0)
                        {
                            foreach (var item1 in offdata)
                            {
                                ovm.Add(new ViewOfferModel
                                {
                                    oid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    location = item1.f.J_Location,
                                    skill = item1.f.J_Skill,
                                    type = item1.f.J_Employment_Type,
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status,
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString()

                                });
                            }
                        }
                    }
                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var offdata = (from c in context.Offer_Master
                                   where EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                                   join
                                       d in context.Resume_Master on c.O_Rid equals d.R_Id
                                   join
                                    e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                   join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                                   join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                   select new { c, d, e, f, g }).ToList();
                    if (offdata.Count > 0)
                    {
                        foreach (var item1 in offdata)
                        {
                            ovm.Add(new ViewOfferModel
                            {
                                oid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                location = item1.e.J_Location,
                                skill = item1.e.J_Skill,
                                type = item1.e.J_Employment_Type,
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status,
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString()

                            });
                        }
                    }


                    return Json(new { data = ovm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ViewTodayStart()
        {
            return View();
        }
        public ActionResult ViewSalesTodayStart()
        {
            return View();
        }
        public JsonResult LoadTodayStart()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<StartViewModel> svm = new List<StartViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var joindata = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                    join
                                        e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                    join
                                        f in context.Offer_Master on e.J_Id equals f.O_Jid
                                    where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.O_Status == "Join"
                                    join
                                        g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                    join
                                        i in context.Resume_Master on f.O_Rid equals i.R_Id
                                    join
                                        j in context.Client_Master on e.J_Client_Id equals j.C_id
                                    select new { c, d, e, f, g, i, j }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.f.O_Id,
                                name = item1.i.R_Name,
                                client = item1.j.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.f.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.f.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status
                            });
                        }
                    }



                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var joindata = (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                        where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.O_Status == "Join"
                                        join
                                        e in context.Resume_Master on d.O_Rid equals e.R_Id
                                        join
                                        f in context.Requirement_Master on e.R_Jid equals f.J_Id
                                        join
                                        g in context.Client_Master on f.J_Client_Id equals g.C_id
                                        select new { c, d, e, f, g }).ToList();
                        if (joindata.Count > 0)
                        {
                            foreach (var item1 in joindata)
                            {
                                svm.Add(new StartViewModel
                                {
                                    jid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    skill = item1.f.J_Skill,
                                    location = item1.f.J_Location,
                                    type = item1.f.J_Employment_Type,
                                    seldate = item1.d.O_Sel_Date.Value.ToShortDateString(),
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString(),
                                    joindate = item1.d.O_Join_Date.Value.ToShortDateString(),
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status
                                });
                            }
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    where c.O_Recruiter == User.Identity.Name
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //view yesterday hire
        public ActionResult ViewYesterdayStart()
        {
            return View();
        }
        public JsonResult LoadYesterdayStart()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                List<StartViewModel> svm = new List<StartViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday)
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var joindata = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                    join
                                        e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                    join
                                        f in context.Offer_Master on e.J_Id equals f.O_Jid
                                    where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && f.O_Status == "Join"
                                    join
                                        g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                    join
                                        i in context.Resume_Master on f.O_Rid equals i.R_Id
                                    join
                                        j in context.Client_Master on e.J_Client_Id equals j.C_id
                                    select new { c, d, e, f, g, i, j }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.f.O_Id,
                                name = item1.i.R_Name,
                                client = item1.j.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.f.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.f.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status
                            });
                        }
                    }



                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var joindata = (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                        where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && d.O_Status == "Join"
                                        join
                                        e in context.Resume_Master on d.O_Rid equals e.R_Id
                                        join
                                        f in context.Requirement_Master on e.R_Jid equals f.J_Id
                                        join
                                        g in context.Client_Master on f.J_Client_Id equals g.C_id
                                        select new { c, d, e, f, g }).ToList();
                        if (joindata.Count > 0)
                        {
                            foreach (var item1 in joindata)
                            {
                                svm.Add(new StartViewModel
                                {
                                    jid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    skill = item1.f.J_Skill,
                                    location = item1.f.J_Location,
                                    type = item1.f.J_Employment_Type,
                                    seldate = item1.d.O_Sel_Date.Value.ToShortDateString(),
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString(),
                                    joindate = item1.d.O_Join_Date.Value.ToShortDateString(),
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status
                                });
                            }
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday)
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    where c.O_Recruiter == User.Identity.Name
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //view thisweek hire
        public ActionResult ViewThisweekStart()
        {
            return View();
        }
        public JsonResult LoadThisweekStart()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                List<StartViewModel> svm = new List<StartViewModel>();
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday)
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    var joindata = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                    join
                                        e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                    join
                                        f in context.Offer_Master on e.J_Id equals f.O_Jid
                                    where EntityFunctions.TruncateTime(f.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Join_Date) >= EntityFunctions.TruncateTime(Endaday) && f.O_Status == "Join"
                                    join
                                        g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                    join
                                        i in context.Resume_Master on f.O_Rid equals i.R_Id
                                    join
                                        j in context.Client_Master on e.J_Client_Id equals j.C_id
                                    select new { c, d, e, f, g, i, j }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.f.O_Id,
                                name = item1.i.R_Name,
                                client = item1.j.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.f.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.f.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.f.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.f.O_Status
                            });
                        }
                    }



                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        var joindata = (from c in context.User_Master
                                        where c.E_Fullname == item
                                        join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                        where EntityFunctions.TruncateTime(d.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && d.O_Status == "Join"
                                        join
                                        e in context.Resume_Master on d.O_Rid equals e.R_Id
                                        join
                                        f in context.Requirement_Master on e.R_Jid equals f.J_Id
                                        join
                                        g in context.Client_Master on f.J_Client_Id equals g.C_id
                                        select new { c, d, e, f, g }).ToList();
                        if (joindata.Count > 0)
                        {
                            foreach (var item1 in joindata)
                            {
                                svm.Add(new StartViewModel
                                {
                                    jid = item1.d.O_Id,
                                    name = item1.e.R_Name,
                                    client = item1.g.C_Name,
                                    skill = item1.f.J_Skill,
                                    location = item1.f.J_Location,
                                    type = item1.f.J_Employment_Type,
                                    seldate = item1.d.O_Sel_Date.Value.ToShortDateString(),
                                    offdate = item1.d.O_Off_Date.Value.ToShortDateString(),
                                    joindate = item1.d.O_Join_Date.Value.ToShortDateString(),
                                    recruiter = item1.c.E_Fullname,
                                    status = item1.d.O_Status
                                });
                            }
                        }
                    }
                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var joindata = (from c in context.Offer_Master
                                    where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday)
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                    join
                                        f in context.Client_Master on e.J_Client_Id equals f.C_id
                                    join
                                        g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                    where c.O_Recruiter == User.Identity.Name
                                    select new { c, d, e, f, g }).ToList();

                    if (joindata.Count > 0)
                    {
                        foreach (var item1 in joindata)
                        {
                            svm.Add(new StartViewModel
                            {
                                jid = item1.c.O_Id,
                                name = item1.d.R_Name,
                                client = item1.f.C_Name,
                                skill = item1.e.J_Skill,
                                location = item1.e.J_Location,
                                type = item1.e.J_Employment_Type,
                                seldate = item1.c.O_Sel_Date.Value.ToShortDateString(),
                                offdate = item1.c.O_Off_Date.Value.ToShortDateString(),
                                joindate = item1.c.O_Join_Date.Value.ToShortDateString(),
                                recruiter = item1.g.E_Fullname,
                                status = item1.c.O_Status
                            });
                        }
                    }

                    return Json(new { data = svm }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public ActionResult AddNewClientStats()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddNewClientStats(ClientStatusModel csm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                NewClient_Master ncm = new NewClient_Master();
                ncm.NClient = csm.nclient;
                ncm.NPoc = csm.npoc;
                ncm.NStatus = csm.nstatus;
                ncm.NRemark1 = csm.nremark1;
                ncm.NRemark2 = csm.nremark2;
                ncm.CreateBy = User.Identity.Name;
                ncm.CreatedOn = System.DateTime.Now;
                context.NewClient_Master.Add(ncm);
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Add New Client Status", System.DateTime.Now);
                return RedirectToAction("AddNewClientStats");
            }
        }
        public ActionResult ViewClientStatus()
        {
            return View();
        }
        public JsonResult LoadClientStatusData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var dataclient = context.NewClient_Master.Where(c => c.CreateBy == User.Identity.Name).ToList();
                return Json(new { data = dataclient }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewMonthReport()
        {
            return View();
        }

        public JsonResult LoadMonthReportData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<SIOSModel> sioslist = new List<SIOSModel>();
                int subcnt = 0, intcnt = 0, offcnt = 0, startcnt = 0;
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var userlist = context.User_Master.Where(c => (c.E_Role == "Recruiter" || c.E_Role == "Teamlead") && c.E_Is_Delete == 0 && c.E_Is_Active == 1).ToList();
                    foreach (var item in userlist)
                    {
                        subcnt = context.Resume_Master.Where(c => c.R_CreateBy == item.E_UserName && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year).ToList().Count();

                        intcnt = context.Interview_Master.Where(c => c.I_CreateBy == item.E_UserName && c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                        offcnt = context.Offer_Master.Where(c => c.O_Recruiter == item.E_UserName && c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year).ToList().Count();

                        startcnt = context.Offer_Master.Where(c => c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "Join" && c.O_Recruiter == item.E_UserName).ToList().Count();

                        sioslist.Add(new SIOSModel
                        {
                            name = item.E_Fullname,
                            submission = subcnt,
                            interview = intcnt,
                            offer = offcnt,
                            start = startcnt
                        });
                    }
                }
                if (User.IsInRole("Team Lead"))
                {
                    var teamlist = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.Team_Master on c.E_Fullname equals d.T_TeamLead
                                    select d.T_TeamMember).First();


                    string[] members = teamlist.ToString().Split(',');
                    foreach (var item in members)
                    {
                        string username = context.User_Master.Where(c => c.E_Fullname == item).Select(c => c.E_UserName).First();

                        subcnt = context.Resume_Master.Where(c => c.R_CreateBy == username && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year).ToList().Count();

                        intcnt = context.Interview_Master.Where(c => c.I_CreateBy == username && c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                        offcnt = context.Offer_Master.Where(c => c.O_Recruiter == username && c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year).ToList().Count();

                        startcnt = context.Offer_Master.Where(c => c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "Join" && c.O_Recruiter == username).ToList().Count();

                        sioslist.Add(new SIOSModel
                        {
                            name = item,
                            submission = subcnt,
                            interview = intcnt,
                            offer = offcnt,
                            start = startcnt
                        });
                    }



                }
                if (User.IsInRole("Recruiter"))
                {
                    string fullname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).First();
                    subcnt = context.Resume_Master.Where(c => c.R_CreateBy == User.Identity.Name && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year).ToList().Count();

                    intcnt = context.Interview_Master.Where(c => c.I_CreateBy == User.Identity.Name && c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                    offcnt = context.Offer_Master.Where(c => c.O_Recruiter == User.Identity.Name && c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year).ToList().Count();

                    startcnt = context.Offer_Master.Where(c => c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "Join" && c.O_Recruiter == User.Identity.Name).ToList().Count();

                    sioslist.Add(new SIOSModel
                    {
                        name = fullname,
                        submission = subcnt,
                        interview = intcnt,
                        offer = offcnt,
                        start = startcnt
                    });
                }
                return Json(new { data = sioslist }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewTodayReport()
        {
            return View();
        }
        public JsonResult LoadTodayReportData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<SIOSModel> sioslist = new List<SIOSModel>();
                int subcnt = 0, intcnt = 0, offcnt = 0, startcnt = 0;
                if (User.IsInRole("Admin"))
                {
                    var userlist = context.User_Master.Where(c => (c.E_Role == "Recruiter" || c.E_Role == "Teamlead") && c.E_Is_Delete == 0 && c.E_Is_Active == 1).ToList();
                    foreach (var item in userlist)
                    {
                        subcnt = context.Resume_Master.Where(c => c.R_CreateBy == item.E_UserName && (EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                        intcnt = context.Interview_Master.Where(c => c.I_CreateBy == item.E_UserName && (EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                        offcnt = context.Offer_Master.Where(c => c.O_Recruiter == item.E_UserName && (EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                        startcnt = context.Offer_Master.Where(c => (EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)) && c.O_Status == "Join" && c.O_Recruiter == item.E_UserName).ToList().Count();

                        sioslist.Add(new SIOSModel
                        {
                            name = item.E_Fullname,
                            submission = subcnt,
                            interview = intcnt,
                            offer = offcnt,
                            start = startcnt
                        });
                    }
                }
                if (User.IsInRole("Team Lead"))
                {
                    var teamlist = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.Team_Master on c.E_Fullname equals d.T_TeamLead
                                    select d.T_TeamMember).First();


                    string[] members = teamlist.ToString().Split(',');
                    foreach (var item in members)
                    {
                        string username = context.User_Master.Where(c => c.E_Fullname == item).Select(c => c.E_UserName).First();

                        subcnt = context.Resume_Master.Where(c => c.R_CreateBy == username && (EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                        intcnt = context.Interview_Master.Where(c => c.I_CreateBy == username && (EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                        offcnt = context.Offer_Master.Where(c => c.O_Recruiter == username && (EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                        startcnt = context.Offer_Master.Where(c => (EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)) && c.O_Status == "Join" && c.O_Recruiter == username).ToList().Count();

                        sioslist.Add(new SIOSModel
                        {
                            name = item,
                            submission = subcnt,
                            interview = intcnt,
                            offer = offcnt,
                            start = startcnt
                        });
                    }



                }
                if (User.IsInRole("Recruiter"))
                {
                    string fullname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).First();
                    subcnt = context.Resume_Master.Where(c => c.R_CreateBy == User.Identity.Name && (EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                    intcnt = context.Interview_Master.Where(c => c.I_CreateBy == User.Identity.Name && (EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                    offcnt = context.Offer_Master.Where(c => c.O_Recruiter == User.Identity.Name && (EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                    startcnt = context.Offer_Master.Where(c => (EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)) && c.O_Status == "Join" && c.O_Recruiter == User.Identity.Name).ToList().Count();

                    sioslist.Add(new SIOSModel
                    {
                        name = fullname,
                        submission = subcnt,
                        interview = intcnt,
                        offer = offcnt,
                        start = startcnt
                    });
                }
                return Json(new { data = sioslist }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewAttendance()
        {
            return View();
        }

        public JsonResult AttendanceData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var attendance = context.Attendance_Master.Where(c => c.A_Name == User.Identity.Name && c.A_InTime.Value.Month == System.DateTime.Now.Month && c.A_InTime.Value.Year == System.DateTime.Now.Year).ToList();
                List<AttendanceModel> amlist = new List<AttendanceModel>();
                foreach (var item in attendance)
                {
                    amlist.Add(new AttendanceModel
                    {
                        aid = item.A_Id,
                        aname = item.A_Name,
                        intime = item.A_InTime.ToString(),
                        inip = item.A_InIP,
                        outtime = item.A_OutTime.ToString(),
                        outip = item.A_OutIP,
                        atime = item.A_Time



                    });
                }

                return Json(new { data = amlist }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult checkattencance()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findattenance = (from c in context.Attendance_Master where c.A_Name == User.Identity.Name && EntityFunctions.TruncateTime(c.A_InTime) == EntityFunctions.TruncateTime(System.DateTime.Now) select c).FirstOrDefault();
                //var findattenance = context.Attendance_Master.Where(c => (c.A_Name == User.Identity.Name) && (EntityFunctions.TruncateTime(Convert.ToDateTime(c.A_InTime)) == EntityFunctions.TruncateTime(System.DateTime.Now))).FirstOrDefault();
                if (findattenance == null)
                {
                    return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    findattenance = (from d in context.Attendance_Master where d.A_Name == User.Identity.Name && EntityFunctions.TruncateTime(d.A_InTime) == EntityFunctions.TruncateTime(System.DateTime.Now) && EntityFunctions.TruncateTime(d.A_OutTime) == EntityFunctions.TruncateTime(System.DateTime.Now) select d).FirstOrDefault();
                    //findattenance = context.Attendance_Master.Where(c => (c.A_Name == User.Identity.Name) && (EntityFunctions.TruncateTime(c.A_InTime) == EntityFunctions.TruncateTime(System.DateTime.Now)) && (EntityFunctions.TruncateTime(c.A_OutTime)==EntityFunctions.TruncateTime(System.DateTime.Now))).FirstOrDefault();
                    if (findattenance == null)
                    {
                        return Json(new { data = 2 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { data = 3 }, JsonRequestBehavior.AllowGet);
                    }

                }

            }
        }
        public JsonResult Addstarttime()
        {
            using (var context = new ATS2019_dbEntities())
            {
                Attendance_Master am = new Attendance_Master();
                am.A_Name = User.Identity.Name;
                am.A_InTime = System.DateTime.Now;
                am.A_InIP = Request.UserHostAddress;
                context.Attendance_Master.Add(am);
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Start Time", System.DateTime.Now);
                int cnt = (from c in context.Attendance_Master where EntityFunctions.TruncateTime(c.A_InTime) == EntityFunctions.TruncateTime(System.DateTime.Now) select c).Count();
                if (cnt == 1)
                {
                    BirthdayandWorkAnniversary();
                    TodaysInterviewList();
                    TodaysJoinCandidate();
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult Addendtime()
        {
            using (var context = new ATS2019_dbEntities())
            {

                var finddata = (from c in context.Attendance_Master where EntityFunctions.TruncateTime(c.A_InTime) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.A_Name == User.Identity.Name select c).FirstOrDefault();
                //var finddata = context.Attendance_Master.Where(c => EntityFunctions.TruncateTime(Convert.ToDateTime(c.A_InTime)) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.A_Name==User.Identity.Name).FirstOrDefault();
                if (finddata != null)
                {
                    TimeSpan td = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(finddata.A_InTime);
                    string totaltime = td.Hours.ToString() + ":" + td.Minutes.ToString();
                    finddata.A_OutTime = System.DateTime.Now;
                    finddata.A_OutIP = Request.UserHostAddress;
                    finddata.A_Time = totaltime;
                    context.Entry(finddata).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "End Time", System.DateTime.Now);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //send birthday remider and work anniversary
        private void BirthdayandWorkAnniversary()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var dd = System.DateTime.Now.Day;
                var mm = System.DateTime.Now.Month;
                string mbody = "<h2> Hello All</h2>";
                var birthdaylist = (from c in context.User_Master where (c.E_DOB.Value.Day == dd) && (c.E_DOB.Value.Month == mm) && (c.E_Is_Active == 1) && (c.E_Is_Delete == 0) select c).ToList();
                //var birthdaylist = (from c in context.User_Master where c.E_DOB.Value.Month == System.DateTime.Now.Month && c.E_Is_Delete==0 && c.E_Is_Active==1 select c).ToList();
                if (birthdaylist.Count > 0)
                {
                    mbody += "<br>";
                    mbody += "<b>Today's Birthday</b>";
                    mbody += "<br>";
                    mbody += "<br>";
                    foreach (var item in birthdaylist)
                    {
                        mbody += item.E_Fullname.ToString();
                        mbody += "<br>";
                        mbody += "<br>";
                    }
                }
                var workanniversarulist = (from d in context.User_Master where (d.E_JoinDate.Value.Day == dd) && (d.E_JoinDate.Value.Month == System.DateTime.Now.Month) && (d.E_Is_Active == 1) && (d.E_Is_Delete == 0) select d).ToList();
                //var workanniversarulist = (from d in context.User_Master where d.E_JoinDate.Value.Month == System.DateTime.Now.Month && d.E_Is_Active == 1 && d.E_Is_Delete == 0 select d).ToList();
                if (workanniversarulist.Count > 0)
                {
                    mbody += "<br>";
                    mbody += "<br>";
                    mbody += "<b>Today Work Anniversary</b>";
                    mbody += "<br>";
                    mbody += "<br>";
                    foreach (var item1 in workanniversarulist)
                    {

                        mbody += item1.E_Fullname.ToString();
                        mbody += "<br>";
                        mbody += "<br>";
                    }
                }
                mbody += "<b>Thank You</b>";
                mbody += "<br>";
                mbody += "<b>Arche Softronix Pvt Ltd</b>";
                string tomaillist = "hr@archesoftronix.com,ritisha.patel@archesoftronix.com";
                if ((birthdaylist.Count > 0) && (workanniversarulist.Count > 0))
                {
                    SendEmail(tomaillist, "Today's Birthday and Work Anniversary", mbody);
                }

            }
        }
        //todays interview 
        private void TodaysInterviewList()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var teamlist = (from c in context.Team_Master select c).ToList();
                foreach (var item in teamlist)
                {
                    string mbody = "";
                    string tolistmail = "";
                    int intcnt = 0;
                    string[] teammember = item.T_TeamMember.ToString().Split(',');
                    mbody += "<b>Hello</b>,";
                    mbody += "<br><br>";
                    mbody += "Today's Interview";
                    mbody += "<br><br>";
                    mbody += "<b>Team Lead : </b>" + item.T_TeamLead.ToString();
                    mbody += "<br><br>";
                    mbody += "<b>Date : </b>" + System.DateTime.Now.ToString("dd MMMM yyyy");
                    mbody += "<br><br>";
                    mbody += "<table width=70% border=1px cellspacing=0 cellpadding=0 style='font-size:15px'><tr><td width=25%><b> Name </b></td><td width=5%><b> Time </b></td><td width=20%><b> Location </b></td><td width=20%><b> Interview By </b></td><td width=20%><b> Recruiter </b></td></tr>";
                    foreach (var item1 in teammember)
                    {
                        var interviewlist = (from d in context.User_Master
                                             where d.E_Fullname == item1
                                             join
                                                 e in context.Interview_Master on d.E_UserName equals e.I_CreateBy
                                             where EntityFunctions.TruncateTime(e.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                             join
                                                 f in context.Resume_Master on e.I_RId equals f.R_Id
                                             select new { d, e, f }).ToList();

                        var userid = (from k in context.User_Master where k.E_Fullname == item1 select k).FirstOrDefault();
                        if (tolistmail == "")
                        {
                            tolistmail += userid.E_UserName.ToString();
                        }
                        else
                        {
                            tolistmail += "," + userid.E_UserName.ToString();
                        }
                        if (interviewlist.Count > 0)
                        {
                            intcnt += interviewlist.Count;
                            foreach (var item2 in interviewlist)
                            {
                                mbody += "<tr><td>" + item2.f.R_Name.ToString() + "</td><td>" + item2.e.I_Time.ToString() + "</td><td>" + item2.e.I_Location + "</td><td>" + item2.e.I_By + "</td><td>" + item2.d.E_Fullname + "</td></tr>";

                            }
                        }
                    }
                    mbody += "</table>";
                    mbody += "<br>";
                    mbody += "<b>Thank You</b>";
                    mbody += "<br>";
                    mbody += "<b>Arche Softronix Pvt Ltd</b>";
                    if (intcnt > 0)
                    {
                        SendEmail(tolistmail, "Today's Interview", mbody);
                    }
                }
            }
        }
        //Todays tobe join candidate
        private void TodaysJoinCandidate()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var teamlist = (from c in context.Team_Master select c).ToList();
                foreach (var item in teamlist)
                {
                    string mbody = "";
                    string tolistmail = "";
                    int joincnt = 0;
                    string[] teammember = item.T_TeamMember.ToString().Split(',');
                    mbody += "<b>Hello</b>,";
                    mbody += "<br><br>";
                    mbody += "Today's Join";
                    mbody += "<br><br>";
                    mbody += "<b>Team Lead : </b>" + item.T_TeamLead.ToString();
                    mbody += "<br><br>";
                    mbody += "<b>Date : </b>" + System.DateTime.Now.ToString("dd MMMM yyyy");
                    mbody += "<br><br>";
                    mbody += "<table width=70% border=1px cellspacing=0 cellpadding=0 style='font-size:15px'><tr><td width=25%><b> Name </b></td><td width=5%><b>Company </b></td><td width=20%><b> Location </b></td><td width=20%><b> Skill </b></td><td width=20%><b> Recruiter </b></td></tr>";
                    foreach (var item1 in teammember)
                    {

                        var tobejoinlist = (from b in context.User_Master
                                            where b.E_Fullname == item1
                                            join
                                                c in context.Offer_Master on b.E_UserName equals c.O_Recruiter
                                            where EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.O_Status == "To be join")
                                            join
                                                d in context.Resume_Master on c.O_Rid equals d.R_Id
                                            join
                                                e in context.Requirement_Master on c.O_Jid equals e.J_Id
                                            join
                                                f in context.Client_Master on e.J_Client_Id equals f.C_id
                                            join
                                                g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                            select new
                                            {
                                                b,
                                                c,
                                                d,
                                                e,
                                                f,
                                                g
                                            }
                         ).ToList();
                        if (tobejoinlist.Count > 0)
                        {
                            joincnt += tobejoinlist.Count;

                            foreach (var item2 in tobejoinlist)
                            {
                                if (tolistmail == "")
                                {
                                    tolistmail += item2.g.E_UserName.ToString();
                                }
                                else
                                {
                                    tolistmail += "," + item2.g.E_UserName.ToString();
                                }
                                mbody += "<tr><td>" + item2.d.R_Name.ToString() + "</td><td>" + item2.f.C_Name.ToString() + "</td><td>" + item2.e.J_Location.ToString() + "</td><td>" + item2.e.J_Skill.ToString() + "</td><td>" + item2.g.E_Fullname.ToString() + "</td></tr>";
                            }
                        }
                    }
                    mbody += "</table>";
                    mbody += "<br>";
                    mbody += "<b>Thank You</b>";
                    mbody += "<br>";
                    mbody += "<b>Arche Softronix Pvt Ltd</b>";
                    if (joincnt > 0)
                    {
                        SendEmail(tolistmail, "Today's Join", mbody);
                    }
                }
            }
        }


        [HttpGet]
        public ActionResult UploadRecruiterSalarySheet()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadRecruiterSalarySheet(HttpPostedFileBase upload)
        {
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    Stream stream = upload.InputStream;
                    IExcelDataReader reader = null;
                    if (upload.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (upload.FileName.EndsWith(".xlsx") || upload.FileName.EndsWith(".XLSX"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View();
                    }
                    //reader.IsFirstRowAsColumnNames = true;
                    DataSet result = reader.AsDataSet();
                    reader.Close();


                    string month = "", year = "";
                    foreach (DataRow r in result.Tables[0].Rows)
                    {
                        if (r.ItemArray.Contains("Month") && r.ItemArray.Contains("Salary Sheet"))
                        {
                            string[] monthyear = r.ItemArray[5].ToString().Split('-');
                            month = monthyear[0].ToString();
                            year = monthyear[1].ToString();
                            break;

                        }
                    }

                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        if (row == null || row.ItemArray.Contains("Salary Sheet") == true || row.ItemArray.Contains("EMPLOYEE CODE") == true || row.ItemArray.Contains("Regular Office Staff") == true || row.ItemArray.Contains("Total") == true || row.ItemArray.Contains("Trainee Office Staff") == true || row.ItemArray.Contains("On Site Employees") == true)
                        {
                            continue;
                        }
                        else
                        {
                            if (row.ItemArray[1].ToString() != "")
                            {
                                bool sucess = InsertRowRecruiterData(row, month, year);
                                continue;
                            }
                        }
                    }
                    return View(result.Tables[0]);
                }
                else
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }
            catch (Exception ex)
            {
            }
            return View("UploadRecruiterSalarySheet");
        }

        public bool InsertRowRecruiterData(DataRow r, string m, string y)
        {
            using (var context = new ATS2019_dbEntities())
            {
                try
                {
                    Recruiter_SalarySlip_Master sm = new Recruiter_SalarySlip_Master();
                    sm.Employee_Code = r.ItemArray[1] != null ? Convert.ToDecimal(r.ItemArray[1].ToString()) : Convert.ToDecimal("0");
                    sm.Employee_Name = r.ItemArray[2] != null ? r.ItemArray[2].ToString() : "";
                    sm.Employee_Pan = "";

                    sm.CTC = r.ItemArray[4] != null ? Convert.ToDecimal(r.ItemArray[4]) : Convert.ToDecimal("0.0");
                    sm.Basic_Salary = r.ItemArray[5] != null ? Convert.ToDecimal(r.ItemArray[5]) : Convert.ToDecimal("0.0");
                    sm.Fixed_HRA = r.ItemArray[6] != null ? Convert.ToDecimal(r.ItemArray[6]) : Convert.ToDecimal("0.0");
                    sm.Fixed_Conveyance_Allowance = r.ItemArray[7] != null ? Convert.ToDecimal(r.ItemArray[7]) : Convert.ToDecimal("0.0");
                    sm.Fixed_Medical_Allowance = r.ItemArray[8] != null ? Convert.ToDecimal(r.ItemArray[8]) : Convert.ToDecimal("0.0");
                    sm.Additional_HRA_Allowance = r.ItemArray[9] != null ? Convert.ToDecimal(r.ItemArray[9]) : Convert.ToDecimal("0.0");

                    sm.Total_Days = r.ItemArray[10] != null ? Convert.ToDecimal(r.ItemArray[10]) : Convert.ToDecimal("0.0");
                    sm.Paid_Leave = r.ItemArray[11] != null ? Convert.ToDecimal(r.ItemArray[11]) : Convert.ToDecimal("0.0");
                    sm.LWP_Days = r.ItemArray[12] != null ? Convert.ToDecimal(r.ItemArray[12]) : Convert.ToDecimal("0.0");
                    sm.Total_Payable_Days = r.ItemArray[13] != null ? Convert.ToDecimal(r.ItemArray[13].ToString()) : Convert.ToDecimal("0");

                    sm.Gross_Salary_Payable = r.ItemArray[14] != null ? Convert.ToDecimal(r.ItemArray[14].ToString()) : Convert.ToDecimal("0");
                    sm.Basic = r.ItemArray[15] != null ? Convert.ToDecimal(r.ItemArray[15].ToString()) : Convert.ToDecimal("0.0");
                    sm.House_Rent = r.ItemArray[16] != null ? Convert.ToDecimal(r.ItemArray[16].ToString()) : Convert.ToDecimal("0.0");
                    sm.Employer_Cont_To_PF = r.ItemArray[17] != null ? Convert.ToDecimal(r.ItemArray[17].ToString()) : Convert.ToDecimal("0.0");
                    sm.Conveyance_Allowance = r.ItemArray[18] != null ? Convert.ToDecimal(r.ItemArray[18].ToString()) : Convert.ToDecimal("0.0");
                    sm.Medical_Allowance = r.ItemArray[19] != null ? Convert.ToDecimal(r.ItemArray[19].ToString()) : Convert.ToDecimal("0.0");
                    sm.Add_HRA_Allowance = r.ItemArray[20] != null ? Convert.ToDecimal(r.ItemArray[20].ToString()) : Convert.ToDecimal("0.0");
                    sm.Flexible_Allowance = r.ItemArray[21] != null ? Convert.ToDecimal(r.ItemArray[21].ToString()) : Convert.ToDecimal("0.0");
                    sm.Incentive_Allowance = r.ItemArray[22] != null ? Convert.ToDecimal(r.ItemArray[22].ToString()) : Convert.ToDecimal("0.0");
                    sm.Total_Earning = r.ItemArray[23] != null ? Convert.ToDecimal(r.ItemArray[23].ToString()) : Convert.ToDecimal("0.0");

                    sm.PF_Employee = r.ItemArray[24] != null ? Convert.ToDecimal(r.ItemArray[24].ToString()) : Convert.ToDecimal("0.0");
                    sm.PF_Employer = r.ItemArray[25] != null ? Convert.ToDecimal(r.ItemArray[25].ToString()) : Convert.ToDecimal("0.0");
                    sm.Esic_Employee = r.ItemArray[26] != null ? Convert.ToDecimal(r.ItemArray[26].ToString()) : Convert.ToDecimal("0.0");
                    sm.Esic_Employer = r.ItemArray[27].ToString() != " " ? Convert.ToDecimal(r.ItemArray[27].ToString()) : Convert.ToDecimal("0.0");
                    sm.PT = r.ItemArray[28] != null ? Convert.ToDecimal(r.ItemArray[28].ToString()) : Convert.ToDecimal("0.0");
                    sm.Advances = r.ItemArray[29] != null ? Convert.ToDecimal(r.ItemArray[29].ToString()) : Convert.ToDecimal("0.0");
                    sm.Income_Tax = r.ItemArray[30] != null ? Convert.ToDecimal(r.ItemArray[30].ToString()) : Convert.ToDecimal("0.0");
                    sm.Total_Deduction = r.ItemArray[31] != null ? Convert.ToDecimal(r.ItemArray[31].ToString()) : Convert.ToDecimal("0.0");
                    sm.Net_Payable = r.ItemArray[32] != null ? Convert.ToDecimal(r.ItemArray[32].ToString()) : Convert.ToDecimal("0.0");
                    sm.Salary_Month = m.ToString() != "" ? m.ToString() : "";
                    sm.Salary_Year = y.ToString() != "" ? y.ToString() : "";
                    context.Recruiter_SalarySlip_Master.Add(sm);
                    context.SaveChanges();
                    return true;

                }
                catch (Exception ex)
                {
                    return false;
                }

            }
        }

        [HttpGet]
        public ActionResult ViewRecuiterSalarySlip()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ViewRecuiterSalarySlip(string SalaryMonthYear)
        {
            using (var context = new ATS2019_dbEntities())
            {
                string[] spliminth = SalaryMonthYear.Split('-');
                int month = Convert.ToInt16(spliminth[1]);
                string mname = month.ToString();
                string year = spliminth[0].ToString();
                string monthname = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).ToString();
                //List<SalarySlipViewModel> users = new List<SalarySlipViewModel>();

                if (User.IsInRole("Admin"))
                {
                    var listsalaryslipdata = (from c in context.Recruiter_SalarySlip_Master
                                              where c.Salary_Month == monthname && c.Salary_Year == year
                                              join
                                                  d in context.User_Master on c.Employee_Code equals d.E_Code
                                              select new { c, d }).ToList();
                    ViewBag.ViewSalarySlipList = listsalaryslipdata;
                }
                else if (User.IsInRole("Contract Salary"))
                {
                    var listsalaryslipdata = (from c in context.Recruiter_SalarySlip_Master
                                              where c.Salary_Month == monthname && c.Salary_Year == year
                                              join
                                                  d in context.User_Master on c.Employee_Code equals d.E_Code
                                              where d.E_Role == "Contract"
                                              select new { c, d }).ToList();
                    ViewBag.ViewSalarySlipList = listsalaryslipdata;
                }
                else
                {
                    var listsalaryslipdata = (from c in context.Recruiter_SalarySlip_Master
                                              where c.Salary_Month == monthname && c.Salary_Year == year
                                              join
                                                  d in context.User_Master on c.Employee_Code equals d.E_Code
                                              where d.E_UserName == User.Identity.Name
                                              select new { c, d }).ToList();
                    ViewBag.ViewSalarySlipList = listsalaryslipdata;
                }

                return View();
            }
        }

        public byte[] GetPDF(string pHTML)
        {


            byte[] bPDF = null;



            MemoryStream ms = new MemoryStream();
            //StringReader txtReader = new StringReader(pHTML);
            TextReader txtReader = new StringReader(pHTML);

            // 1: create object of a itextsharp document class
            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);

            // 2: we create a itextsharp pdfwriter that listens to the document and directs a XML-stream to a file
            PdfWriter oPdfWriter = PdfWriter.GetInstance(doc, ms);

            // 3: we create a worker parse the document
            HTMLWorker htmlWorker = new HTMLWorker(doc);

            // 4: we open document and start the worker on the document
            doc.Open();
            doc.NewPage();
            htmlWorker.StartDocument();

            // 5: parse the html into the document
            htmlWorker.Parse(txtReader);

            // 6: close the document and the worker
            htmlWorker.EndDocument();
            htmlWorker.Close();
            doc.Close();

            bPDF = ms.ToArray();




            return bPDF;

        }

        public void DownloadRecuiterPDF(decimal? salaryid)
        {
            using (var context = new ATS2019_dbEntities())
            {


                var query = (from c in context.Recruiter_SalarySlip_Master
                             where c.Salary_Id == salaryid
                             join
                                 d in context.User_Master on c.Employee_Code equals d.E_Code
                             select new { c, d }).FirstOrDefault();


                //var query = (from c in context.User_Master where c.E_Fullname == ename select c).FirstOrDefault();
                if (query != null)
                {
                    string monthyear = query.c.Salary_Month + "-" + query.c.Salary_Year;

                    NumberToWord n = new NumberToWord();
                    double dd = Convert.ToDouble(query.c.Net_Payable);
                    // int ii = int.Parse(dd.ToString());
                    int ii = int.Parse(Math.Round(dd).ToString());
                    string numbertoword = n.NumberToText(ii);

                    string strdob = DateTime.Parse(Convert.ToDateTime(query.d.E_DOB).ToShortDateString()).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                    string strjd = DateTime.Parse(Convert.ToDateTime(query.d.E_JoinDate).ToShortDateString()).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);


                    StringBuilder st = new StringBuilder();

                    //st.Append("<table width='100%'><tr><td width='25%'><img src='http://www.archesoftronix.in/img/logo.png'/></td><td width='25%'>&nbsp;</td><td colspan='2' width='50%'>");
                    
                    st.Append("<table width='100%'><tr><td width='25%'><img src='E:\\Work From Home\\ATS2019_2\\ATS2019_2\\img\\logo.png'/></td><td width='25%'>&nbsp;</td><td colspan='2' width='50%'>");
                    //st.Append("<table width='100%'><tr><td width='25%'><img src='F:\\Users\\Arche\\documents\\visual studio 2013\\Projects\\ATS2019_2\\ATS2019_2\\img\\logo.png'/></td><td width='25%'>&nbsp;</td><td colspan='2' width='50%'>");
                    st.Append("<h3 style='color:#FFA500;'>ARCHE SOFTRONIX PRIVATE LIMITED</h3><span style='color:#003366;font-size:9.5px;padding-right:2px;'>401, Iscon Atria-2, Opp. GET,<br/> Gotri Main Road,Vadodara-390021 Gujarat, India<br/>Website : www.archesoftronix.com<br/>Phone : +91 966 227 7804</span></td></tr>");
                    st.Append("</table>");


                    st.Append("<br/>");
                    st.Append("<table width='100%'><tr><td colspan='4' width='100%' Height='200%' bgcolor='orange'>&nbsp;</td></tr></table>");
                    st.Append("<br/>");

                    st.Append("<table width='100%'>");
                    st.Append("<tr><td colspan='4' width='100%' bgcolor='lightgoldenrodyellow' valign='middle'><span style='text-align:center;font-size:10px;'>Pay Slip For Month Of : " + monthyear + "</span></td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Employee Code</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Code.ToString() + "</td><td width='25%' style='font-size:9.5px;'>PAN</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_PAN_No + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Employee Name</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Fullname.ToString() + "</td><td width='25%' style='font-size:9.5px;'>PF No.</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_PF_Account + " </td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Designation</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Designation.ToString() + "</td><td width='25%' style='font-size:9.5px;'>ESI No.</td><td width='25%' style='font-size:9.5px;'>&nbsp;</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Client</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Company_Name.ToString() + "</td><td width='25%' style='font-size:9.5px;'>Date Of Birth</td><td width='25%' style='font-size:9.5px;'>" + strdob.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Bank Name</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_BankName + "</td><td width='25%' style='font-size:9.5px;'>Date Of Joining</td><td width='25%' style='font-size:9.5px;'>" + strjd.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Bank A/c No.</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_AccountNo + "</td><td width='25%' style='font-size:9.5px;'>Total Days</td><td width='25%' style='font-size:9.5px;'>" + query.c.Total_Days.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Payable Days</td><td width='25%' style='font-size:9.5px;'>" + query.c.Total_Payable_Days.ToString() + "</td><td width='25%' style='font-size:9.5px;'>Paid Leave</td><td width='25%' style='font-size:9.5px;'>" + query.c.Paid_Leave.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>&nbsp;</td><td width='25%' style='font-size:9.5px;'>&nbsp;</td><td width='25%' style='font-size:9.5px;'>LWP Days</td><td width='25%' style='font-size:9.5px;'>" + query.c.LWP_Days.ToString() + "</td></tr>");
                    st.Append("</table>");

                    st.Append("<br/>");
                    st.Append("<br/>");

                    st.Append("<table width='100%' cellpadding='0' cellspacing='0'><tr><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Earning</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Amount(Rs.)</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Deduction</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Amount(Rs.)</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Basic</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Basic.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>PF-Employee Contribution</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.PF_Employee.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>HRA</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.House_Rent.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>ESI</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Esic_Employee.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Conveyence Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Conveyance_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>Professional Tax</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.PT.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Medical Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Medical_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>Income Tax</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Income_Tax.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Additional HRA Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Add_HRA_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>Advances Recovered</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Advances.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Flexible Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Flexible_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>PF-Employer's Contribution</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.PF_Employer.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>PF-Employer's Contribution</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.PF_Employer.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>&nbsp;</span></td><td width='25%' border='1'><span style='text-align:right;'>&nbsp;</span></td></tr>");
                    if (query.c.Incentive_Allowance > 0)
                    {
                        st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Incentive</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Incentive_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>&nbsp;</span></td><td width='25%' border='1'><span style='text-align:right;'>&nbsp;</span></td></tr>");
                    }
                    st.Append("<tr><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Total</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:right;font-size:9.5px;padding-left:5px;'>" + query.c.Total_Earning.ToString() + "</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Total</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:right;font-size:9.5px;padding-left:5px;'>" + query.c.Total_Deduction.ToString() + "</span></td></tr>");
                    st.Append("</table>");

                    st.Append("<table width='100%'><tr><td width='50%' style='font-size:9.5px;'>Gross Pay : " + query.c.CTC.ToString() + "</td><td width='50%' style='font-size:9.5px;'>Net Pay : " + query.c.Net_Payable.ToString() + "</td></tr>");
                    st.Append("<tr><td colspan='4' style='font-size:9.5px;'>In Words : " + numbertoword.ToString() + "</td></tr>");
                    st.Append("</table>");

                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    if (query.c.Incentive_Allowance == 0)
                    {
                        st.Append("<br/>");
                    }

                    st.Append("<table width='100%'><tr><td colspan='4' width='100%' Height='100%' bgcolor='lightgoldenrodyellow' valign='middle' style='font-size:9.5px;'>Note : This is a computer generated copy and no signature is required.</td></tr></table>");

                    st.Append("<br/>");
                    st.Append("<table width='100%'><tr><td colspan='4' width='100%' Height='100%' bgcolor='orange'>&nbsp;</td></tr></table>");
                    st.Append("<br/>");

                    st.Append("<table width='100%'><tr><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>INDIA</span></td><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>CANADA</span></td><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>EUROPE</span></td><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>USA</span></td></tr></table>");


                    string HTMLContent = st.ToString();
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + query.d.E_Fullname + ".pdf");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.BinaryWrite(GetPDF(HTMLContent));
                    Response.End();
                }
            }
        }

        //Reyna technology salary sheet code
        [HttpGet]
        public ActionResult UploadReynaSalarySheet()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadReynaSalarySheet(HttpPostedFileBase upload)
        {
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    Stream stream = upload.InputStream;
                    IExcelDataReader reader = null;
                    if (upload.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (upload.FileName.EndsWith(".xlsx") || upload.FileName.EndsWith(".XLSX"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View();
                    }
                    //reader.IsFirstRowAsColumnNames = true;
                    DataSet result = reader.AsDataSet();
                    reader.Close();


                    string month = "", year = "";
                    foreach (DataRow r in result.Tables[0].Rows)
                    {
                        if (r.ItemArray.Contains("Month") && r.ItemArray.Contains("Salary Sheet"))
                        {
                            string[] monthyear = r.ItemArray[5].ToString().Split('-');
                            month = monthyear[0].ToString();
                            year = monthyear[1].ToString();
                            break;

                        }
                    }

                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        if (row == null || row.ItemArray.Contains("Salary Sheet") == true || row.ItemArray.Contains("EMPLOYEE CODE") == true || row.ItemArray.Contains("Regular Office Staff") == true || row.ItemArray.Contains("Total") == true || row.ItemArray.Contains("Trainee Office Staff") == true || row.ItemArray.Contains("On Site Employees") == true)
                        {
                            continue;
                        }
                        else
                        {
                            if (row.ItemArray[1].ToString() != "")
                            {
                                bool sucess = InsertReynaData(row, month, year);
                                continue;
                            }
                        }
                    }
                    return View(result.Tables[0]);
                }
                else
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }
            catch (Exception ex)
            {
            }
            return View("UploadReynaSalarySheet");
        }

        public bool InsertReynaData(DataRow r, string m, string y)
        {
            using (var context = new ATS2019_dbEntities())
            {
                try
                {
                    Reyna_SalarySlip_Master sm = new Reyna_SalarySlip_Master();
                   // Recruiter_SalarySlip_Master sm = new Recruiter_SalarySlip_Master();
                    sm.Employee_Code = r.ItemArray[1] != null ? Convert.ToDecimal(r.ItemArray[1].ToString()) : Convert.ToDecimal("0");
                    sm.Employee_Name = r.ItemArray[2] != null ? r.ItemArray[2].ToString() : "";
                    sm.Employee_Pan = "";

                    sm.CTC = r.ItemArray[4] != null ? Convert.ToDecimal(r.ItemArray[4]) : Convert.ToDecimal("0.0");
                    sm.Basic_Salary = r.ItemArray[5] != null ? Convert.ToDecimal(r.ItemArray[5]) : Convert.ToDecimal("0.0");
                    sm.Fixed_HRA = r.ItemArray[6] != null ? Convert.ToDecimal(r.ItemArray[6]) : Convert.ToDecimal("0.0");
                    sm.Fixed_Conveyance_Allowance = r.ItemArray[7] != null ? Convert.ToDecimal(r.ItemArray[7]) : Convert.ToDecimal("0.0");
                    sm.Fixed_Medical_Allowance = r.ItemArray[8] != null ? Convert.ToDecimal(r.ItemArray[8]) : Convert.ToDecimal("0.0");
                    sm.Additional_HRA_Allowance = r.ItemArray[9] != null ? Convert.ToDecimal(r.ItemArray[9]) : Convert.ToDecimal("0.0");

                    sm.Total_Days = r.ItemArray[10] != null ? Convert.ToDecimal(r.ItemArray[10]) : Convert.ToDecimal("0.0");
                    sm.Paid_Leave = r.ItemArray[11] != null ? Convert.ToDecimal(r.ItemArray[11]) : Convert.ToDecimal("0.0");
                    sm.LWP_Days = r.ItemArray[12] != null ? Convert.ToDecimal(r.ItemArray[12]) : Convert.ToDecimal("0.0");
                    sm.Total_Payable_Days = r.ItemArray[13] != null ? Convert.ToDecimal(r.ItemArray[13].ToString()) : Convert.ToDecimal("0");

                    sm.Gross_Salary_Payable = r.ItemArray[14] != null ? Convert.ToDecimal(r.ItemArray[14].ToString()) : Convert.ToDecimal("0");
                    sm.Basic = r.ItemArray[15] != null ? Convert.ToDecimal(r.ItemArray[15].ToString()) : Convert.ToDecimal("0.0");
                    sm.House_Rent = r.ItemArray[16] != null ? Convert.ToDecimal(r.ItemArray[16].ToString()) : Convert.ToDecimal("0.0");
                    sm.Employer_Cont_To_PF = r.ItemArray[17] != null ? Convert.ToDecimal(r.ItemArray[17].ToString()) : Convert.ToDecimal("0.0");
                    sm.Conveyance_Allowance = r.ItemArray[18] != null ? Convert.ToDecimal(r.ItemArray[18].ToString()) : Convert.ToDecimal("0.0");
                    sm.Medical_Allowance = r.ItemArray[19] != null ? Convert.ToDecimal(r.ItemArray[19].ToString()) : Convert.ToDecimal("0.0");
                    sm.Add_HRA_Allowance = r.ItemArray[20] != null ? Convert.ToDecimal(r.ItemArray[20].ToString()) : Convert.ToDecimal("0.0");
                    sm.Flexible_Allowance = r.ItemArray[21] != null ? Convert.ToDecimal(r.ItemArray[21].ToString()) : Convert.ToDecimal("0.0");
                    sm.Incentive_Allowance = r.ItemArray[22] != null ? Convert.ToDecimal(r.ItemArray[22].ToString()) : Convert.ToDecimal("0.0");
                    sm.Total_Earning = r.ItemArray[23] != null ? Convert.ToDecimal(r.ItemArray[23].ToString()) : Convert.ToDecimal("0.0");

                    sm.PF_Employee = r.ItemArray[24] != null ? Convert.ToDecimal(r.ItemArray[24].ToString()) : Convert.ToDecimal("0.0");
                    sm.PF_Employer = r.ItemArray[25] != null ? Convert.ToDecimal(r.ItemArray[25].ToString()) : Convert.ToDecimal("0.0");
                    sm.Esic_Employee = r.ItemArray[26] != null ? Convert.ToDecimal(r.ItemArray[26].ToString()) : Convert.ToDecimal("0.0");
                    sm.Esic_Employer = r.ItemArray[27].ToString() != " " ? Convert.ToDecimal(r.ItemArray[27].ToString()) : Convert.ToDecimal("0.0");
                    sm.PT = r.ItemArray[28] != null ? Convert.ToDecimal(r.ItemArray[28].ToString()) : Convert.ToDecimal("0.0");
                    sm.Advances = r.ItemArray[29] != null ? Convert.ToDecimal(r.ItemArray[29].ToString()) : Convert.ToDecimal("0.0");
                    sm.Income_Tax = r.ItemArray[30] != null ? Convert.ToDecimal(r.ItemArray[30].ToString()) : Convert.ToDecimal("0.0");
                    sm.Total_Deduction = r.ItemArray[31] != null ? Convert.ToDecimal(r.ItemArray[31].ToString()) : Convert.ToDecimal("0.0");
                    sm.Net_Payable = r.ItemArray[32] != null ? Convert.ToDecimal(r.ItemArray[32].ToString()) : Convert.ToDecimal("0.0");
                    sm.Salary_Month = m.ToString() != "" ? m.ToString() : "";
                    sm.Salary_Year = y.ToString() != "" ? y.ToString() : "";
                    context.Reyna_SalarySlip_Master.Add(sm);
                    context.SaveChanges();
                    return true;

                }
                catch (Exception ex)
                {
                    return false;
                }

            }
        }

        [HttpGet]
        public ActionResult ViewReynaSalarySlip()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ViewReynaSalarySlip(string SalaryMonthYear)
        {
            using (var context = new ATS2019_dbEntities())
            {
                string[] spliminth = SalaryMonthYear.Split('-');
                int month = Convert.ToInt16(spliminth[1]);
                string mname = month.ToString();
                string year = spliminth[0].ToString();
                string monthname = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).ToString();
                //List<SalarySlipViewModel> users = new List<SalarySlipViewModel>();

                if (User.IsInRole("Admin"))
                {
                    var listsalaryslipdata = (from c in context.Reyna_SalarySlip_Master
                                              where c.Salary_Month == monthname && c.Salary_Year == year
                                              join
                                                  d in context.User_Master on c.Employee_Code equals d.E_Code
                                              select new { c, d }).ToList();
                    ViewBag.ViewSalarySlipList = listsalaryslipdata;
                }
                else if (User.IsInRole("Contract Salary"))
                {
                    var listsalaryslipdata = (from c in context.Reyna_SalarySlip_Master
                                              where c.Salary_Month == monthname && c.Salary_Year == year
                                              join
                                                  d in context.User_Master on c.Employee_Code equals d.E_Code
                                              where d.E_Role == "Contract"
                                              select new { c, d }).ToList();
                    ViewBag.ViewSalarySlipList = listsalaryslipdata;
                }
                else
                {
                    var listsalaryslipdata = (from c in context.Reyna_SalarySlip_Master
                                              where c.Salary_Month == monthname && c.Salary_Year == year
                                              join
                                                  d in context.User_Master on c.Employee_Code equals d.E_Code
                                              where d.E_UserName == User.Identity.Name
                                              select new { c, d }).ToList();
                    ViewBag.ViewSalarySlipList = listsalaryslipdata;
                }
                
                return View();
            }
        }


        public void DownloadReynaPDF(decimal? salaryid)
        {
            using (var context = new ATS2019_dbEntities())
            {


                var query = (from c in context.Reyna_SalarySlip_Master
                             where c.Salary_Id == salaryid
                             join
                                 d in context.User_Master on c.Employee_Code equals d.E_Code
                             select new { c, d }).FirstOrDefault();


                //var query = (from c in context.User_Master where c.E_Fullname == ename select c).FirstOrDefault();
                if (query != null)
                {
                    string monthyear = query.c.Salary_Month + "-" + query.c.Salary_Year;

                    NumberToWord n = new NumberToWord();
                    double dd = Convert.ToDouble(query.c.Net_Payable);
                    // int ii = int.Parse(dd.ToString());
                    int ii = int.Parse(Math.Round(dd).ToString());
                    string numbertoword = n.NumberToText(ii);

                    string strdob = DateTime.Parse(Convert.ToDateTime(query.d.E_DOB).ToShortDateString()).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                    string strjd = DateTime.Parse(Convert.ToDateTime(query.d.E_JoinDate).ToShortDateString()).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);


                    StringBuilder st = new StringBuilder();

                    //--st.Append("<table width='100%'><tr><td width='25%'><img src='http://www.archesoftronix.in/img/logo.png'/></td><td width='25%'>&nbsp;</td><td colspan='2' width='50%'>");
                    st.Append("<table width='100%'>");
                    st.Append("<tr><td width='25%' Height='20%'>&nbsp;</td><td width=25%>&nbsp;</td><td colspan='2' width='50%'><table width='100%'><tr><td width='2%' bgcolor='black'>&nbsp;</td><td width='2%' bgcolor='white'>&nbsp;</td><td width='4%' bgcolor='black'>&nbsp;</td><td width='2%' bgcolor='white'>&nbsp;</td><td width='90%' bgcolor='#FF6700'><span style='color:#FF6700;'>Reyna</span></td></tr></table></td></tr>");
                    st.Append("</table>");
                    //--st.Append("<tr><td width='25%'><img src='E:\\Work From Home\\ATS2019_2\\ATS2019_2\\img\\reynalogo.png'/></td><td width='25%'>&nbsp;</td><td colspan='2' width='50%'>");
                    //--st.Append("<table width='100%'><tr><td width='25%'><img src='F:\\Users\\Arche\\documents\\visual studio 2013\\Projects\\ATS2019_2\\ATS2019_2\\img\\logo.png'/></td><td width='25%'>&nbsp;</td><td colspan='2' width='50%'>");
                    st.Append("<table width='100%'>");
                    st.Append("<tr><td width='15%'><img src='E:\\Work From Home\\ATS2019_2\\ATS2019_2\\img\\reynalogo.png' width='240px' height='80px'/></td><td width='45%'>&nbsp;</td><td colspan='2' width='40%' style='border-bottom-style:solid;border-bottom:thick solid gray;'><h2 style='color:#FF6700;'>REYNA SOLUTIONS LLP</h2><span style='color:#000000;font-size:11px;padding-right:2px;'>GSTIN : 24ABAFR9929C1Z7<br/> LLPIN : AAR-9361</span></td></tr>");
                    st.Append("</table>");
                    //st.Append("<table width='100%' Height='20%' style='background-image: url(~\\img\\header.PNG); background-repeat: no-repeat; background-position: center; background - size: cover;'><tr><td colspan='4' widhth='100'>&nbsp;</td></tr></table>");
                    st.Append("<br/>");
                    st.Append("<table width='100%'><tr><td colspan='4' width='100%' Height='200%' bgcolor='#FF6700'>&nbsp;</td></tr></table>");
                    st.Append("<br/>");

                    st.Append("<table width='100%'>");
                    st.Append("<tr><td colspan='4' width='100%' bgcolor='lightgoldenrodyellow' valign='middle'><span style='text-align:center;font-size:10px;'>Pay Slip For Month Of : " + monthyear + "</span></td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Employee Code</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Code.ToString() + "</td><td width='25%' style='font-size:9.5px;'>PAN</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_PAN_No + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Employee Name</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Fullname.ToString() + "</td><td width='25%' style='font-size:9.5px;'>PF No.</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_PF_Account + " </td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Designation</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Designation.ToString() + "</td><td width='25%' style='font-size:9.5px;'>ESI No.</td><td width='25%' style='font-size:9.5px;'>&nbsp;</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Client</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_Company_Name.ToString() + "</td><td width='25%' style='font-size:9.5px;'>Date Of Birth</td><td width='25%' style='font-size:9.5px;'>" + strdob.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Bank Name</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_BankName + "</td><td width='25%' style='font-size:9.5px;'>Date Of Joining</td><td width='25%' style='font-size:9.5px;'>" + strjd.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Bank A/c No.</td><td width='25%' style='font-size:9.5px;'>" + query.d.E_AccountNo + "</td><td width='25%' style='font-size:9.5px;'>Total Days</td><td width='25%' style='font-size:9.5px;'>" + query.c.Total_Days.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>Payable Days</td><td width='25%' style='font-size:9.5px;'>" + query.c.Total_Payable_Days.ToString() + "</td><td width='25%' style='font-size:9.5px;'>Paid Leave</td><td width='25%' style='font-size:9.5px;'>" + query.c.Paid_Leave.ToString() + "</td></tr>");
                    st.Append("<tr><td width='25%' style='font-size:9.5px;'>&nbsp;</td><td width='25%' style='font-size:9.5px;'>&nbsp;</td><td width='25%' style='font-size:9.5px;'>LWP Days</td><td width='25%' style='font-size:9.5px;'>" + query.c.LWP_Days.ToString() + "</td></tr>");
                    st.Append("</table>");

                    st.Append("<br/>");
                    st.Append("<br/>");

                    st.Append("<table width='100%' cellpadding='0' cellspacing='0'><tr><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Earning</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Amount(Rs.)</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Deduction</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Amount(Rs.)</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Basic</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Basic.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>PF-Employee Contribution</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.PF_Employee.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>HRA</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.House_Rent.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>ESI</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Esic_Employee.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Conveyence Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Conveyance_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>Professional Tax</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.PT.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Medical Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Medical_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>Income Tax</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Income_Tax.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Additional HRA Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Add_HRA_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>Advances Recovered</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.Advances.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Flexible Allowance</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Flexible_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>PF-Employer's Contribution</span></td><td width='25%' border='1'><span style='text-align:right;font-size:9.5px;padding-right:5px;'>" + query.c.PF_Employer.ToString() + "</span></td></tr>");
                    st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>PF-Employer's Contribution</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.PF_Employer.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>&nbsp;</span></td><td width='25%' border='1'><span style='text-align:right;'>&nbsp;</span></td></tr>");
                    if (query.c.Incentive_Allowance > 0)
                    {
                        st.Append("<tr><td width='25%' border='1'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Incentive</span></td><td width='25%' border='1'><span style='text-align:right;padding-right:10px;font-size:9.5px;padding-right:5px;'>" + query.c.Incentive_Allowance.ToString() + "</span></td><td width='25%' border='1'><span style='text-align:left;padding-left:10px;font-size:9.5px;padding-left:5px;'>&nbsp;</span></td><td width='25%' border='1'><span style='text-align:right;'>&nbsp;</span></td></tr>");
                    }
                    st.Append("<tr><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Total</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:right;font-size:9.5px;padding-left:5px;'>" + query.c.Total_Earning.ToString() + "</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:left;font-size:9.5px;padding-left:5px;'>Total</span></td><td width='25%' border='1' bgcolor='lightgoldenrodyellow'><span style='text-align:right;font-size:9.5px;padding-left:5px;'>" + query.c.Total_Deduction.ToString() + "</span></td></tr>");
                    st.Append("</table>");

                    st.Append("<table width='100%'><tr><td width='50%' style='font-size:9.5px;'>Gross Pay : " + query.c.CTC.ToString() + "</td><td width='50%' style='font-size:9.5px;'>Net Pay : " + query.c.Net_Payable.ToString() + "</td></tr>");
                    st.Append("<tr><td colspan='4' style='font-size:9.5px;'>In Words : " + numbertoword.ToString() + "</td></tr>");
                    st.Append("</table>");

                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    st.Append("<br/>");
                    //st.Append("<br/>");
                    //st.Append("<br/>");
                    //st.Append("<br/>");
                    //st.Append("<br/>");
                    if (query.c.Incentive_Allowance == 0)
                    {
                        st.Append("<br/>");
                    }

                    st.Append("<table width='100%'><tr><td colspan='4' width='100%' Height='100%' bgcolor='lightgoldenrodyellow' valign='middle' style='font-size:9.5px;'>Note : This is a computer generated copy and no signature is required.</td></tr></table>");
                    st.Append("<table width='100%' ><tr><td width='17%' Height='20%' bgcolor='#FF6700'>&nbsp;</td>");
                    st.Append("<td width='5%' bgcolor='white'>&nbsp;</td>");
                    st.Append("<td width ='23%' ><img src='E:\\Work From Home\\ATS2019_2\\ATS2019_2\\img\\website.png' /><span style='color:#000000;font-size:9.5px;padding-right:2px;padding-left:5px;margin-left:5px;'> &nbsp; www.reynasolutions.com</span>");
                    st.Append("<br/>");
                    st.Append("<img src ='E:\\Work From Home\\ATS2019_2\\ATS2019_2\\img\\contact.png' /><span style ='color:#000000;font-size:9.5px;padding-right:2px;padding-left:5px;margin-left:5px;'> &nbsp; +91 966 227 7804</span>");
                    st.Append("<br/>");
                    st.Append("<img src ='E:\\Work From Home\\ATS2019_2\\ATS2019_2\\img\\mail.png' /><span style='color:#000000;font-size:9.5px;padding-right:2px;padding-left:5px;margin-left:5px;'> &nbsp; info@reynasolutions.com</span>");
                    st.Append("</td>");
                    st.Append("<td width='15%' >&nbsp;</td>");
                    st.Append("<td width='25%' ><span style='color:#000000;font-size:9.5px;padding-right:2px;padding-left:3px;float:right;'>202 / 401, ISCON ATRIA 2, &nbsp; <img src='E:\\Work From Home\\ATS2019_2\\ATS2019_2\\img\\address.png'/></span>");
                    st.Append("<table width='100%' style='color:#000000;font-size:7.5px;padding-right:7px;padding-left:3px;margin-top:-5'><tr><td Align='right' style='padding-right:10px;border-style:solid;border-right:2px solid #000000;'>Gotri main road,<br/>Vadodara - 390021,<br/> Gujarat, India </td></tr></table>");
                    st.Append("</td>");
                    st.Append("<td width='3%'>&nbsp;</td>");
                    st.Append("<td width='12%' bgcolor='black'>&nbsp;</td>");
                    st.Append("</tr></table>");
                    st.Append("<table width='100%'><tr><td width='25%' style='text-align:center;font-size:15px;font-weight:bold;'>India</td><td width='25%' style='text-align:center;font-size:15px;font-weight:bold;'>USA</td><td width='25%' style='text-align:center;font-size:15px;font-weight:bold;'>EUROPE</td><td width='25%' style='text-align:center;font-size:15px;font-weight:bold;'>LATAM</td></tr></table>");
                    
                   // st.Append("<table width='100%' align='Center'><tr><td width='25%'>INDIA<td><td width='25%'>USA</td><td width='25%'>EUROPE</td><td width='25%'>LATAM</td></tr></table>");
                                                                                                                                                         //st.Append("<br/>");
                                                                                                                                                         //st.Append("<table width='100%'><tr><td colspan='4' width='100%' Height='100%' bgcolor='orange'>&nbsp;</td></tr></table>");
                                                                                                                                                         //st.Append("<br/>");

                                                                                                                                                         //st.Append("<table width='100%'><tr><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>INDIA</span></td><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>CANADA</span></td><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>EUROPE</span></td><td width='25%'><span style='text-align:center;font-size:9.5px;padding-left:5px;'>USA</span></td></tr></table>");


                    string HTMLContent = st.ToString();
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + query.d.E_Fullname + ".pdf");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.BinaryWrite(GetPDF(HTMLContent));
                    Response.End();
                }
            }
        }

        //Reyna technolgoy salary slip code ends 

        [HttpGet]
        public ActionResult AssignClienttoSales()
        {
            Salesuserlist();
            Clientlist();
            return View();
        }
        protected void Salesuserlist()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var totsalesuser = context.User_Master.Where(m => m.E_Is_Active == 1 && m.E_Is_Delete == 0 && m.E_Role == "Sales").ToList().OrderBy(m => m.E_Code);
                List<SelectListItem> userlist = new List<SelectListItem>();
                foreach (var item in totsalesuser)
                {
                    userlist.Add(new SelectListItem
                    {
                        Text = item.E_Code.ToString(),
                        Value = item.E_Fullname.ToString()

                    });
                }
                ViewBag.salesuserlist = userlist.ToList();

            }

        }
        protected void Clientlist()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var clientlist = context.Client_Master.Where(c => c.C_Is_Active == 1 && c.C_Is_Delete == 0).ToList().OrderBy(c => c.C_Name);
                List<SelectListItem> totclientlist = new List<SelectListItem>();
                foreach (var item in clientlist)
                {
                    totclientlist.Add(new SelectListItem
                    {
                        Text = item.C_id.ToString(),
                        Value = item.C_Name.ToString()

                    });
                }
                ViewBag.clientlist = totclientlist.ToList();
            }
        }
        [HttpPost]
        public ActionResult AssignClienttoSales(SalesClientModel scm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                SalesClient_Master sc = new SalesClient_Master();
                sc.SC_UID = scm.uid;
                sc.SC_CID = scm.cid;
                sc.CreatedBy = User.Identity.Name;
                sc.CreatedOn = System.DateTime.Now;
                context.SalesClient_Master.Add(sc);
                context.SaveChanges();
                //Message to show after client assign to sales
                TempData["msg"] = "Record Added Successfully.";
                CreateActivityLog(User.Identity.Name, "Assign Client To Sales", System.DateTime.Now);
                return RedirectToAction("AssignClienttoSales");
            }
        }

        public ActionResult ViewSalesClient()
        {
            return View();
        }
        public JsonResult LoadSalesClientData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var salesclientlist = (from c in context.SalesClient_Master
                                       join
                                           d in context.User_Master on c.SC_UID equals d.E_Code
                                       join
                                           e in context.Client_Master on c.SC_CID equals e.C_id
                                       select new { c, d, e }
                                                                               ).ToList();
                return Json(new { data = salesclientlist }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteSalesClient(decimal uid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var deletesalesclient = context.SalesClient_Master.Where(c => c.SC_Id == uid).FirstOrDefault();
                context.Entry(deletesalesclient).State = EntityState.Deleted;
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Remove Assign Client to Sales", System.DateTime.Now);
                return RedirectToAction("ViewSalesClient");
            }
        }
        [HttpGet]
        public ActionResult ViewAttendanceReport()
        {
            return View();
        }

        public JsonResult LoadAttendanceReport(string dms)
        {
            if (dms != "")
            {
                string[] spliminth = dms.Split('-');
                int mmonth = Convert.ToInt16(spliminth[1]);

                int year = int.Parse(spliminth[0].ToString());


                using (var context = new ATS2019_dbEntities())
                {
                    List<AttendanceModel> amlist = new List<AttendanceModel>();
                    var attendancereportlist = (from c in context.Attendance_Master
                                                where c.A_OutTime.Value.Month == mmonth && c.A_OutTime.Value.Year == year
                                                join
                                                    d in context.User_Master on c.A_Name equals d.E_UserName
                                                select new { c, d }
                                                                                        ).ToList();
                    foreach (var item in attendancereportlist)
                    {
                        amlist.Add(new AttendanceModel
                        {
                            aid = item.c.A_Id,
                            aname = item.d.E_Fullname,
                            intime = item.c.A_InTime.ToString(),
                            inip = item.c.A_InIP.ToString(),
                            outtime = item.c.A_OutTime.ToString(),
                            outip = item.c.A_OutIP.ToString(),
                            atime = item.c.A_Time
                        });
                    }

                    return Json(new { data = amlist }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DateWiseCountReport()
        {
            return View();
        }

        public JsonResult LoadDateWiseCountReportData(string ssd, string eed)
        {
            DateTime dd1 = Convert.ToDateTime(ssd);
            DateTime dd2 = Convert.ToDateTime(eed);
            List<SIOSModel> sioslist = new List<SIOSModel>();
            int subcnt = 0, intcnt = 0, offcnt = 0, startcnt = 0;
            using (var context = new ATS2019_dbEntities())
            {


                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    var userlist = context.User_Master.Where(c => (c.E_Role == "Recruiter" || c.E_Role == "Teamlead") && c.E_Is_Delete == 0 && c.E_Is_Active == 1).ToList();
                    foreach (var item in userlist)
                    {
                        subcnt = context.Resume_Master.Where(c => (c.R_CreateBy == item.E_UserName) && (EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                        intcnt = context.Interview_Master.Where(c => (c.I_CreateBy == item.E_UserName) && (EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(dd2)) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                        offcnt = context.Offer_Master.Where(c => c.O_Recruiter == item.E_UserName && (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                        startcnt = context.Offer_Master.Where(c => (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2)) && c.O_Status == "Join" && c.O_Recruiter == item.E_UserName).ToList().Count();

                        sioslist.Add(new SIOSModel
                        {
                            name = item.E_Fullname,
                            submission = subcnt,
                            interview = intcnt,
                            offer = offcnt,
                            start = startcnt
                        });
                    }

                }
                if (User.IsInRole("Team Lead"))
                {
                    var teamlist = (from c in context.User_Master
                                    where c.E_UserName == User.Identity.Name
                                    join
                                        d in context.Team_Master on c.E_Fullname equals d.T_TeamLead
                                    select d.T_TeamMember).First();


                    string[] members = teamlist.ToString().Split(',');
                    foreach (var item in members)
                    {
                        string username = context.User_Master.Where(c => c.E_Fullname == item).Select(c => c.E_UserName).First();

                        subcnt = context.Resume_Master.Where(c => (c.R_CreateBy == username) && (EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                        intcnt = context.Interview_Master.Where(c => (c.I_CreateBy == username) && (EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(dd2)) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                        offcnt = context.Offer_Master.Where(c => c.O_Recruiter == username && (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                        startcnt = context.Offer_Master.Where(c => (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2)) && c.O_Status == "Join" && c.O_Recruiter == username).ToList().Count();

                        sioslist.Add(new SIOSModel
                        {
                            name = item,
                            submission = subcnt,
                            interview = intcnt,
                            offer = offcnt,
                            start = startcnt
                        });
                    }

                }
                if (User.IsInRole("Recruiter"))
                {
                    string fullname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).First();
                    string username = User.Identity.Name.ToString();

                    subcnt = context.Resume_Master.Where(c => (c.R_CreateBy == username) && (EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                    intcnt = context.Interview_Master.Where(c => (c.I_CreateBy == username) && (EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(dd2)) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                    offcnt = context.Offer_Master.Where(c => c.O_Recruiter == username && (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                    startcnt = context.Offer_Master.Where(c => (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2)) && c.O_Status == "Join" && c.O_Recruiter == username).ToList().Count();

                    sioslist.Add(new SIOSModel
                    {
                        name = fullname,
                        submission = subcnt,
                        interview = intcnt,
                        offer = offcnt,
                        start = startcnt
                    });
                }
                return Json(new { data = sioslist }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ViewColumnChart()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<columnchart_model> subpoint = new List<columnchart_model>();
                List<columnchart_model> intpoint = new List<columnchart_model>();
                List<columnchart_model> offerpoint = new List<columnchart_model>();
                List<columnchart_model> startpoint = new List<columnchart_model>();
                int mm = System.DateTime.Now.Month;
                int yy = System.DateTime.Now.Year;
                var userlist = context.User_Master.Where(c => (c.E_Role == "Recruiter" || c.E_Role == "Teamlead") && c.E_Is_Delete == 0 && c.E_Is_Active == 1).ToList();
                foreach (var item in userlist)
                {
                    //submission count
                    var subcnt = context.Resume_Master.Where(c => c.R_CreateBy == item.E_UserName && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year).ToList().Count();
                    subpoint.Add(new columnchart_model
                    (
                        item.E_Fullname,
                        Convert.ToInt32(subcnt.ToString())
                    ));

                    //interview count
                    var intcnt = context.Interview_Master.Where(c => c.I_CreateBy == item.E_UserName && c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();
                    intpoint.Add(new columnchart_model
                    (
                        item.E_Fullname,
                        Convert.ToInt32(intcnt.ToString())
                    ));

                    //offer Count
                    var offcnt = context.Offer_Master.Where(c => c.O_Recruiter == item.E_UserName && c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year).ToList().Count();
                    if (Convert.ToInt64(offcnt) > 0)
                    {
                        offerpoint.Add(new columnchart_model
                        (
                            item.E_Fullname,
                            Convert.ToInt32(offcnt.ToString())
                        ));
                    }
                    //Hire count
                    var startcnt = context.Offer_Master.Where(c => c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "Join" && c.O_Recruiter == item.E_UserName).ToList().Count();
                    if (Convert.ToInt64(startcnt) > 0)
                    {
                        startpoint.Add(new columnchart_model
                        (
                            item.E_Fullname,
                            Convert.ToInt32(startcnt.ToString())
                        ));
                    }
                }
                ViewBag.subPoints = JsonConvert.SerializeObject(subpoint);
                ViewBag.intPoints = JsonConvert.SerializeObject(intpoint);
                ViewBag.offerPoints = JsonConvert.SerializeObject(offerpoint);
                ViewBag.startpoints = JsonConvert.SerializeObject(startpoint);

            }

            return View();
        }
        [HttpGet]
        public ActionResult ViewDailyActivity()
        {
            return View();
        }

        public ActionResult LoadDailyTargetData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int yes = 1, no = 0;
                bool check = context.DailyTarget_Master.Where(c => EntityFunctions.TruncateTime(c.DT_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now)).Any();
                if (check)
                {
                    var findall = context.DailyTarget_Master.Where(c => EntityFunctions.TruncateTime(c.DT_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now)).ToList();
                    foreach (var item in findall)
                    {
                        int achived = (from c in context.User_Master
                                       where c.E_Fullname == item.DT_AssignTo
                                       join
                                           d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                       where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                                       select new { c, d }).Count();
                        var findtodaydata = context.DailyTarget_Master.Where(c => c.DT_AssignTo == item.DT_AssignTo && EntityFunctions.TruncateTime(c.DT_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now)).FirstOrDefault();
                        findtodaydata.DT_Target = achived;
                        context.Entry(findtodaydata).State = EntityState.Modified;
                        context.SaveChanges();

                    }
                    var targetlist = context.DailyTarget_Master.Where(c => EntityFunctions.TruncateTime(c.DT_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now)).ToList();
                    return Json(new { data = targetlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var findall = context.User_Master.Where(c => ((c.E_Role == "Teamlead") || (c.E_Role == "Recruiter")) && (c.E_Is_Active == yes) && (c.E_Is_Delete == no)).ToList();
                    foreach (var item in findall)
                    {
                        DailyTarget_Master dt = new DailyTarget_Master();
                        dt.DT_Target = 4;
                        dt.DT_Achived = 0;
                        dt.DT_CreateOn = System.DateTime.Now;
                        dt.DT_AssignTo = item.E_Fullname;
                        context.DailyTarget_Master.Add(dt);
                        context.SaveChanges();
                    }
                    var targetlist = context.DailyTarget_Master.Where(c => EntityFunctions.TruncateTime(c.DT_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now)).ToList();
                    return Json(new { data = targetlist }, JsonRequestBehavior.AllowGet);
                }

            }
        }
        public ActionResult LoadDailyClosureData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ClosureModel> cm = new List<ClosureModel>();
                var findall = context.User_Master.Where(c => ((c.E_Role == "Teamlead") || (c.E_Role == "Recruiter")) && (c.E_Is_Active == 1) && (c.E_Is_Delete == 0)).ToList();
                foreach (var item in findall)
                {
                    int closure = context.Resume_Master.Where(c => c.R_CreateBy == item.E_Fullname && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.R_Status == "New").Count();
                    cm.Add(new ClosureModel
                    {
                        name = item.E_Fullname,
                        closure = closure
                    });
                }
                return Json(new { data = cm }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult CreateHolidayList()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateHolidayList(HolidayModel hm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                HolidayList_Master hmm = new HolidayList_Master();
                hmm.Hname = hm.hname;
                hmm.Hdate = hm.date;
                hmm.Hday = hm.day;
                hmm.Htype = hm.type;
                hmm.CreatedOn = DateTime.Now;
                hmm.CreatedBy = User.Identity.Name;
                context.HolidayList_Master.Add(hmm);
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Holiday List Created", System.DateTime.Now);
                TempData["msg"] = "Holiday Added Successfully.";
                return RedirectToAction("CreateHolidayList");
            }
        }
        [HttpGet]
        public ActionResult ViewHolidayList()
        {
            return View();
        }
        public JsonResult ViewHolidayListData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var holidaylist = context.HolidayList_Master.ToList().OrderBy(c => c.Hdate);
                return Json(new { data = holidaylist }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Deleteholiday(decimal hid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findholiday = context.HolidayList_Master.Where(c => c.Hid == hid).FirstOrDefault();
                context.Entry(findholiday).State = EntityState.Deleted;
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Holiday List Deleted", System.DateTime.Now);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SearchResume()
        {

            return View();
        }
        [HttpPost]
        public ActionResult SearchResume(string txtby, string txtvalue)
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (txtby == "By Email")
                {
                    var resumelist = (from c in context.Resume_Master where c.R_Email.Contains(txtvalue) select c).ToList();
                    ViewBag.Resumelist = resumelist;
                }
                if (txtby == "By Name")
                {
                    var resumelist = (from c in context.Resume_Master where c.R_Name.Contains(txtvalue) select c).ToList();
                    ViewBag.Resumelist = resumelist;
                }
                if (txtby == "By Number")
                {
                    var resumelist = (from c in context.Resume_Master where c.R_Cnt.Contains(txtvalue) select c).ToList();
                    ViewBag.Resumelist = resumelist;
                }
                CreateActivityLog(User.Identity.Name, "Resume Search", System.DateTime.Now);

            }
            return View();
        }
        public ActionResult DownloadResume(string resumename)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "ResumeDirectory/";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + resumename);
            string fileName = resumename;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            CreateActivityLog(User.Identity.Name, "Resume Download", System.DateTime.Now);
        }
        [HttpGet]
        public ActionResult ViewSalesClientList()
        {
            return View();
        }
        public ActionResult LoadSalesClientListData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var clientcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Client_Master on d.SC_CID equals e.C_id
                                 select new { c, d, e }).ToList();
                return Json(new { data = clientcnt }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult UpdateOfferStats(decimal oid, string st)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var chnageofferstats = context.Offer_Master.Where(c => c.O_Id == oid).FirstOrDefault();
                chnageofferstats.O_Status = st;
                context.Entry(chnageofferstats).State = EntityState.Modified;
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Offer Status Updated", System.DateTime.Now);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ApplyForLeave()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ApplyForLeave(LeaveModel lm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var findname = (from c in context.User_Master where c.E_UserName == User.Identity.Name select c).FirstOrDefault();
                string mbody = "";
                mbody += "<h3>Hello</h3>";
                mbody += "<b>Subject</b> : Leave Application";
                mbody += "<br>";
                mbody += "<br>";
                mbody += "<b>Name</b> : " + findname.E_Fullname.ToString();
                mbody += "<br>";
                mbody += "<br>";
                mbody += "<b>Leave Type</b> : " + lm.ltype.ToString();
                float totatnoofdays = 0;
                if (lm.linwords == "Half Day")
                {
                    mbody += " for Half day on " + lm.lstartdate.ToString("dd MMMM yyyy");
                    totatnoofdays = float.Parse("0.5");
                }
                else if (lm.linwords == "Full Day")
                {
                    mbody += " for full day on " + lm.lstartdate.ToString("dd MMMM yyyy");
                    totatnoofdays = float.Parse("1");
                }
                else
                {
                    mbody += " from " + lm.lstartdate.ToString("dd MMMM yyyy") + " to " + lm.lenddate.ToString("dd MMMM yyyy");
                    TimeSpan t1 = lm.lenddate - lm.lstartdate;
                    totatnoofdays = t1.Days + int.Parse("1");
                }
                mbody += "<br>";
                mbody += "<br>";
                mbody += "Reason : " + lm.lreason.ToString();
                mbody += "<br>";
                mbody += "<br>";
                mbody += "Thanks & Regards";
                mbody += "<br>";
                mbody += "Arche Softronix Pvt Ltd";

                Leave_Master lmm = new Leave_Master();
                lmm.L_Type = lm.ltype;
                lmm.L_Noofdays = totatnoofdays;
                lmm.L_Inwords = lm.linwords;
                lmm.L_Reason = lm.lreason;
                lmm.L_StartDate = lm.lstartdate;
                lmm.L_EndDate = lm.lenddate;
                lmm.L_CreateBy = User.Identity.Name;
                lmm.L_CreateOn = System.DateTime.Now;
                if (User.IsInRole("Team Lead"))
                {
                    lmm.L_TL_Status = "Approve";
                }
                else
                {
                    lmm.L_TL_Status = "Pending";
                }
                lmm.L_TL_Datetime = System.DateTime.Now;
                if (User.IsInRole("Manager"))
                {
                    lmm.L_M_Status = "Approve";
                }
                else
                {
                    lmm.L_M_Status = "Pending";
                }
                lmm.L_M_Datetime = System.DateTime.Now;
                lmm.L_Admin_Status = "Pending";
                lmm.L_Admin_Datetime = System.DateTime.Now;
                context.Leave_Master.Add(lmm);
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Apply For Leave", System.DateTime.Now);
                string tomaillist = "hr@archesoftronix.com";
                SendEmail(tomaillist, "Leave Application", mbody);
                TempData["msg"] = "Leave application send successfully.";
            }

            return RedirectToAction("ApplyForLeave");
        }
        [HttpGet]
        public ActionResult ViewApplyLeave()
        {
            return View();
        }
        public JsonResult LoadLeaveData()
        {
            List<LeaveModel> leaveList = new List<LeaveModel>();
            using (var context = new ATS2019_dbEntities())
            {

                if ((User.IsInRole("Recruiter")) || (User.IsInRole("Sales")) || (User.IsInRole("IT")))
                {
                    var leavelistdata = (from c in context.Leave_Master
                                         where c.L_CreateBy == User.Identity.Name && c.L_EndDate.Year == System.DateTime.Now.Year
                                         join
                                             d in context.User_Master on c.L_CreateBy equals d.E_UserName
                                         select new { c, d }).ToList();

                    foreach (var item in leavelistdata)
                    {
                        leaveList.Add(new LeaveModel
                        {
                            lid = item.c.L_Id,
                            ltype = item.c.L_Type,
                            lnoofdays = float.Parse(item.c.L_Noofdays.ToString()),
                            linwords = item.c.L_Inwords,
                            lreason = item.c.L_Reason,
                            lstartdate = item.c.L_StartDate,
                            lenddate = item.c.L_EndDate,
                            lcreateby = item.d.E_Fullname,
                            lcreateon = item.c.L_CreateOn,
                            ltlstatus = item.c.L_TL_Status,
                            lmstatus = item.c.L_M_Status,
                            ladminstatus = item.c.L_Admin_Status
                        });
                    }

                    return Json(new { data = leaveList }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item1 in tm)
                    {
                        var leavelistdata = (from c in context.User_Master
                                             where c.E_Fullname == item1
                                             join
                                                 d in context.Leave_Master on c.E_UserName equals d.L_CreateBy
                                             where d.L_EndDate.Year == System.DateTime.Now.Year
                                             select new { c, d }).ToList();


                        foreach (var item in leavelistdata)
                        {
                            leaveList.Add(new LeaveModel
                            {
                                lid = item.d.L_Id,
                                ltype = item.d.L_Type,
                                lnoofdays = float.Parse(item.d.L_Noofdays.ToString()),
                                linwords = item.d.L_Inwords,
                                lreason = item.d.L_Reason,
                                lstartdate = item.d.L_StartDate,
                                lenddate = item.d.L_EndDate,
                                lcreateby = item.c.E_Fullname,
                                lcreateon = item.d.L_CreateOn,
                                ltlstatus = item.d.L_TL_Status,
                                lmstatus = item.d.L_M_Status,
                                ladminstatus = item.d.L_Admin_Status
                            });
                        }



                    }

                    return Json(new { data = leaveList }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var leavelistdata = (from c in context.Leave_Master
                                         where c.L_EndDate.Year == System.DateTime.Now.Year
                                         join
                                             d in context.User_Master on c.L_CreateBy equals d.E_UserName
                                         select new { c, d }).ToList().OrderByDescending(m => m.c.L_CreateOn);

                    foreach (var item in leavelistdata)
                    {
                        leaveList.Add(new LeaveModel
                        {
                            lid = item.c.L_Id,
                            ltype = item.c.L_Type,
                            lnoofdays = float.Parse(item.c.L_Noofdays.ToString()),
                            linwords = item.c.L_Inwords,
                            lreason = item.c.L_Reason,
                            lstartdate = item.c.L_StartDate,
                            lenddate = item.c.L_EndDate,
                            lcreateby = item.d.E_Fullname,
                            lcreateon = item.c.L_CreateOn,
                            ltlstatus = item.c.L_TL_Status,
                            lmstatus = item.c.L_M_Status,
                            ladminstatus = item.c.L_Admin_Status
                        });
                    }

                    return Json(new { data = leaveList }, JsonRequestBehavior.AllowGet);
                }

            }

        }

        public ActionResult changeleavestatus(string lvid, string st)
        {
            using (var context = new ATS2019_dbEntities())
            {
                string tolist = "hr@archesoftronix.com";
                string mbody = "";
                mbody += "<h3>Hello</h3>";

                decimal llid = Convert.ToDecimal(lvid);
                var findrecord = context.Leave_Master.Where(c => c.L_Id == llid).FirstOrDefault();
                if (User.IsInRole("Team Lead"))
                {
                    mbody += "<b>Subject </b>: Leave status change by your TeamLead : " + st.ToString();
                    mbody += "<br><br>";
                    mbody += "";
                    mbody += "Thanks & Regards";
                    mbody += "<br>";
                    mbody += "Arche Softronix Pvt Ltd";
                    tolist += "," + User.Identity.Name + "," + findrecord.L_CreateBy.ToString();
                    findrecord.L_TL_Status = st;
                    findrecord.L_TL_Datetime = System.DateTime.Now;
                    context.Entry(findrecord).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Leave Application Response", System.DateTime.Now);
                    SendEmail(tolist, "Leave Status", mbody);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Manager"))
                {
                    mbody += "<b>Subject </b>: Leave status change by Manager : " + st.ToString();
                    mbody += "<br><br>";
                    mbody += "";
                    mbody += "Thanks & Regards";
                    mbody += "<br>";
                    mbody += "Arche Softronix Pvt Ltd";
                    tolist += "," + User.Identity.Name + "," + findrecord.L_CreateBy.ToString();
                    findrecord.L_M_Status = st;
                    findrecord.L_M_Datetime = System.DateTime.Now;
                    context.Entry(findrecord).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Leave Application Response", System.DateTime.Now);
                    SendEmail(tolist, "Leave Status", mbody);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tolist += "," + User.Identity.Name + "," + findrecord.L_CreateBy.ToString();
                    mbody += "<b>Subject </b>: Leave status change by HR/Admin : " + st.ToString();
                    mbody += "<br><br>";
                    mbody += "";
                    mbody += "Thanks & Regards";
                    mbody += "<br>";
                    mbody += "Arche Softronix Pvt Ltd";
                    findrecord.L_Admin_Status = st;
                    findrecord.L_Admin_Datetime = System.DateTime.Now;
                    context.Entry(findrecord).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Leave Application Response", System.DateTime.Now);
                    SendEmail(tolist, "Leave Status", mbody);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }



            }
        }
        [HttpGet]
        public ActionResult ViewLeaveBalance()
        {
            return View();
        }

        public JsonResult LoadLeaveBalanceData()
        {
            List<LeaveBalanceModel> lbm = new List<LeaveBalanceModel>();
            using (var context = new ATS2019_dbEntities())
            {

                float totearnpy = 0;
                var un = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).FirstOrDefault();
                var countleav = context.Leave_Master.Where(c => c.L_CreateBy == User.Identity.Name && (c.L_Type == "Sick Leave" || c.L_Type == "Casual Leave") && c.L_Admin_Status == "Approve").Select(c => c.L_Noofdays).ToList();
                if (countleav != null)
                {
                    float totsickleavepy = 0;
                    foreach (var item in countleav)
                    {
                        totsickleavepy += float.Parse(item.ToString());
                    }
                    lbm.Add(new LeaveBalanceModel
                    {
                        name = un.E_Fullname.ToString(),
                        title = "Sick Leave /Casual Leave",
                        total = float.Parse("6"),
                        totused = totsickleavepy,
                        totbal = float.Parse("6") - totsickleavepy
                    });
                }

                for (int i = 1; i < System.DateTime.Now.Month; i++)
                {
                    if (i != System.DateTime.Now.Month)
                    {
                        totearnpy += float.Parse("1.25");
                    }
                }
                var countearnleave = context.Leave_Master.Where(c => c.L_CreateBy == User.Identity.Name && (c.L_Type == "Earn Leave") && c.L_Admin_Status == "Approve").Select(c => c.L_Noofdays).ToList();
                if (countearnleave != null)
                {
                    float totearnleavepy = 0;

                    foreach (var item1 in countearnleave)
                    {
                        totearnleavepy += float.Parse(item1.ToString());
                    }
                    lbm.Add(new LeaveBalanceModel
                    {
                        name = un.E_Fullname.ToString(),
                        title = "Earn Leave",
                        total = totearnpy,
                        totused = totearnleavepy,
                        totbal = totearnpy - totearnleavepy
                    });
                }

                return Json(new { data = lbm }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SalesClientWiseMonthlyRevenueAdd()
        {
            SalesClientList();
            return View();
        }
        [HttpPost]
        public ActionResult SalesClientWiseMonthlyRevenueAdd(SalesClientWiseMonthlyRevenueModel scmr)
        {
            using (var context = new ATS2019_dbEntities())
            {
                int mm = Convert.ToDateTime(scmr.srmonth).Month;
                string monthname = findmonth(mm.ToString());
                string yy = Convert.ToDateTime(scmr.srmonth).Year.ToString();
                SalesClientwiseMonthlyRevenue_Master RM = new SalesClientwiseMonthlyRevenue_Master();
                RM.SRR_Month = monthname;
                RM.SRR_Year = Convert.ToDecimal(yy);
                RM.SRR_Client = scmr.srclient;
                RM.SRR_Current_HC = scmr.srcurrenthc;
                RM.SRR_Total_GP = Convert.ToDecimal(scmr.srtotgp);
                RM.SRR_Avg_GP_Added = Convert.ToDecimal(scmr.sravggpadded);
                RM.SRR_Start = scmr.srstart;
                RM.SRR_Attrition = scmr.srattrition;
                RM.SRR_BD = scmr.srbd;
                RM.SRR_Actual_Start = scmr.sractualstart;
                RM.SRR_Net_Total_GP = Convert.ToDecimal(scmr.srnettotgp);
                RM.SRR_Total_GP_Added = Convert.ToDecimal(scmr.srnettotgpadded);
                RM.SRR_Typeof_Employement = scmr.srtypeofemployement;
                RM.SRR_Create_By = User.Identity.Name;
                RM.SRR_Create_On = System.DateTime.Now;
                RM.SRR_Update_By = User.Identity.Name;
                RM.SRR_Update_On = System.DateTime.Now;
                context.SalesClientwiseMonthlyRevenue_Master.Add(RM);
                context.SaveChanges();
                CreateActivityLog(User.Identity.Name, "Manual Add Sale's Client Wise Monthly Data", System.DateTime.Now);
                ViewBag.msg = "Record Added Successfully";
                return RedirectToAction("SalesClientWiseMonthlyRevenueAdd");
            }
        }
        [HttpGet]
        public ActionResult SalesMonthlyReport()
        {
            SalesClientList();
            return View();
        }
        [HttpPost]
        public ActionResult SalesMonthlyReport(SalesMonthlyReportModel smrm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                int mm = Convert.ToDateTime(smrm.smrtime).Month;
                string monthname = findmonth(mm.ToString());
                string yy = Convert.ToDateTime(smrm.smrtime).Year.ToString();
                SalesMonthlyReport_Master smr = new SalesMonthlyReport_Master();
                smr.SMR_Month = monthname.ToString();
                smr.SMR_Year = yy.ToString();
                smr.SMR_Client = smrm.client;
                smr.SMR_OpenPosition = smrm.position != null ? smrm.position : Convert.ToInt32("0");
                smr.SMR_BusinessReceived = smrm.business != null ? smrm.business : Convert.ToInt32("0");
                smr.SMR_Submission = smrm.submission != null ? smrm.submission : Convert.ToInt32("0");
                smr.SMR_Int_received = smrm.intreceived != null ? smrm.intreceived : Convert.ToInt32("0");
                smr.SMR_Feedback_Pending = smrm.feedbackpending != null ? smrm.feedbackpending : Convert.ToInt32("0");
                smr.SMR_Noshow = smrm.noshow != null ? smrm.noshow : Convert.ToInt32("0");
                smr.SMR_Offer = smrm.offer != null ? smrm.offer : Convert.ToInt32("0");
                smr.SMR_BD = smrm.bd != null ? smrm.bd : Convert.ToInt32("0");
                smr.SMR_Join = smrm.join != null ? smrm.join : Convert.ToInt32("0");
                smr.SMR_PassThrough = smrm.passthrough != null ? smrm.passthrough : Convert.ToInt32("0");
                smr.SMR_BulkDeal = smrm.bulkdeal != null ? smrm.bulkdeal : Convert.ToInt32("0");
                smr.SMR_POExtended = smrm.poextend != null ? smrm.poextend : Convert.ToInt32("0");
                smr.SMR_AttritionSaved = smrm.attrition != null ? smrm.attrition : Convert.ToInt32("0");
                smr.SMR_TotalRevenue = smrm.totrevenue != null ? smrm.totrevenue : Convert.ToDecimal("0");
                smr.SMR_Remark = smrm.remark != null ? smrm.remark.ToString() : "";
                smr.CreatedOn = System.DateTime.Now;
                smr.CreatedBy = User.Identity.Name;
                context.SalesMonthlyReport_Master.Add(smr);
                context.SaveChanges();
                ViewBag.msg = "Saved";
                return RedirectToAction("SalesMonthlyReport");
            }
        }
        public ActionResult ViewMonthSalesReport()
        {
            return View();
        }
        public JsonResult LoadMonthSalesReportData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Admin"))
                {
                    var viewmonthlist = (from c in context.SalesMonthlyReport_Master join d in context.User_Master on c.CreatedBy equals d.E_UserName select new { c, d }).ToList();
                    return Json(new { data = viewmonthlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var viewmonthlist = (from c in context.SalesMonthlyReport_Master where c.CreatedBy == User.Identity.Name join d in context.User_Master on c.CreatedBy equals d.E_UserName select new { c, d }).ToList();
                    return Json(new { data = viewmonthlist }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public string findmonth(string m)
        {
            string monthname = "";
            switch (m)
            {
                case "1":
                    monthname = "January";
                    break;
                case "2":
                    monthname = "February";
                    break;
                case "3":
                    monthname = "March";
                    break;
                case "4":
                    monthname = "April";
                    break;
                case "5":
                    monthname = "May";
                    break;
                case "6":
                    monthname = "June";
                    break;
                case "7":
                    monthname = "July";
                    break;
                case "8":
                    monthname = "August";
                    break;
                case "9":
                    monthname = "September";
                    break;
                case "10":
                    monthname = "October";
                    break;
                case "11":
                    monthname = "November";
                    break;
                case "12":
                    monthname = "December";
                    break;
            }
            return monthname;
        }
        [HttpGet]
        public ActionResult ViewSalesMonthClientWiseRevenueReport()
        {
            return View();
        }
        public JsonResult ViewSalesMonthClientwiseRevenuewData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Sales"))
                {
                    var lissalesmonthclientwisedata = (from c in context.SalesClientwiseMonthlyRevenue_Master
                                                       where c.SRR_Create_By == User.Identity.Name
                                                       join
                                                           d in context.User_Master on c.SRR_Create_By equals d.E_UserName
                                                       join
                                                           e in context.Client_Master on c.SRR_Client equals e.C_id
                                                       select new { c, d, e }).ToList();
                    return Json(new { data = lissalesmonthclientwisedata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var lissalesmonthclientwisedata = (from c in context.SalesClientwiseMonthlyRevenue_Master
                                                       join
                                                           d in context.User_Master on c.SRR_Create_By equals d.E_UserName
                                                       join
                                                           e in context.Client_Master on c.SRR_Client equals e.C_id
                                                       select new { c, d, e }).ToList();
                    return Json(new { data = lissalesmonthclientwisedata }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        [HttpGet]
        public ActionResult ViewlDatewiseTeamLeadCountReport()
        {
            Teamleadlist();
            return View();
        }

        public JsonResult LoadDateWiseTeamLeadCountReportData(string ttl, string ssd, string eed)
        {
            List<SIOSModel> sioslist = new List<SIOSModel>();
            using (var context = new ATS2019_dbEntities())
            {
                DateTime dd1 = Convert.ToDateTime(ssd);
                DateTime dd2 = Convert.ToDateTime(eed);

                int subcnt = 0, intcnt = 0, offcnt = 0, startcnt = 0;
                var teamlist = (from d in context.Team_Master where d.T_TeamLead == ttl select d.T_TeamMember).FirstOrDefault();


                string[] members = teamlist.ToString().Split(',');
                foreach (var item in members)
                {
                    string username = context.User_Master.Where(c => c.E_Fullname == item).Select(c => c.E_UserName).First();

                    subcnt = context.Resume_Master.Where(c => (c.R_CreateBy == username) && (EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                    intcnt = context.Interview_Master.Where(c => (c.I_CreateBy == username) && (EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(dd2)) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show")).ToList().Count();

                    offcnt = context.Offer_Master.Where(c => c.O_Recruiter == username && (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2))).ToList().Count();

                    startcnt = context.Offer_Master.Where(c => (EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(dd1)) && (EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(dd2)) && c.O_Status == "Join" && c.O_Recruiter == username).ToList().Count();

                    sioslist.Add(new SIOSModel
                    {
                        name = item,
                        submission = subcnt,
                        interview = intcnt,
                        offer = offcnt,
                        start = startcnt
                    });
                }


            }
            return Json(new { data = sioslist }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult getoffereddataaddcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Team Lead"))
                {
                    int podc = 0;
                    string val = "-";
                    var leadname = (from c in context.User_Master where c.E_UserName == User.Identity.Name select c.E_Fullname).FirstOrDefault();
                    if (leadname != null)
                    {
                        var teammember = (from d in context.Team_Master where d.T_TeamLead == leadname select d.T_TeamMember).FirstOrDefault();
                        if (teammember != null)
                        {
                            string[] tmlist = teammember.ToString().Split(',');
                            foreach (var item in tmlist)
                            {
                                int offerlist = (from e in context.User_Master
                                                 where e.E_Fullname == item
                                                 join
                                                     f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                                 where f.O_Status == "Offer"
                                                 select new { e, f }).ToList().Count;
                                podc += offerlist;
                            }
                        }
                    }
                    if (podc > 0)
                    {
                        val = podc.ToString();
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Manager"))
                {
                    string val = "-";
                    int podc = (from e in context.User_Master
                                join
                                    f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                where f.O_Manager_Status == null && f.O_Teamlead != null
                                select new { e, f }).ToList().Count;
                    if (podc > 0)
                    {
                        val = podc.ToString();
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string val = "-";
                    int podc = (from e in context.User_Master
                                join
                                    f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                where (f.O_Manager_Status != null) && (f.O_Admin_Status == null)
                                select new { e, f }).ToList().Count;
                    if (podc > 0)
                    {
                        val = podc.ToString();
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult gettobejoincount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tbjc = 0;
                if (User.IsInRole("Team Lead"))
                {
                    var leadname = (from c in context.User_Master where c.E_UserName == User.Identity.Name select c.E_Fullname).FirstOrDefault();
                    if (leadname != null)
                    {
                        var teammember = (from d in context.Team_Master where d.T_TeamLead == leadname select d.T_TeamMember).FirstOrDefault();
                        if (teammember != null)
                        {
                            string[] tmlist = teammember.ToString().Split(',');
                            foreach (var item in tmlist)
                            {
                                int offerlist = (from e in context.User_Master
                                                 where e.E_Fullname == item
                                                 join
                                                     f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                                 where f.O_Status == "To be join"
                                                 select new { e, f }).ToList().Count;
                                tbjc += offerlist;
                            }
                        }
                    }
                    return Json(new { tbjc }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    tbjc = (from e in context.User_Master
                            join
                                f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                            where f.O_Status == "To be join"
                            select new { e, f }).ToList().Count;
                    return Json(new { tbjc }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        public ActionResult ViewOfferDataList()
        {
            return View();
        }
        public JsonResult LoadOfferDataPendingList()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ViewOfferModel> vm = new List<ViewOfferModel>();

                if (User.IsInRole("Team Lead"))
                {
                    int podc = 0;
                    var leadname = (from c in context.User_Master where c.E_UserName == User.Identity.Name select c.E_Fullname).FirstOrDefault();
                    if (leadname != null)
                    {
                        var teammember = (from d in context.Team_Master where d.T_TeamLead == leadname select d.T_TeamMember).FirstOrDefault();
                        if (teammember != null)
                        {
                            string[] tmlist = teammember.ToString().Split(',');
                            foreach (var item in tmlist)
                            {
                                var offerlist = (from e in context.User_Master
                                                 where e.E_Fullname == item
                                                 join
                                                     f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                                 where f.O_Status == "Offer"
                                                 join
                                                     g in context.Resume_Master on f.O_Rid equals g.R_Id
                                                 join
                                                     h in context.Requirement_Master on g.R_Jid equals h.J_Id
                                                 join
                                                      i in context.Client_Master on h.J_Client_Id equals i.C_id

                                                 select new { e, f, g, h, i }).ToList();
                                if (offerlist != null)
                                {
                                    foreach (var item1 in offerlist)
                                    {
                                        vm.Add(new ViewOfferModel
                                        {
                                            oid = item1.f.O_Id,
                                            name = item1.g.R_Name,
                                            client = item1.i.C_Name,
                                            location = item1.h.J_Location,
                                            skill = item1.h.J_Skill,
                                            status = item1.f.O_Status,
                                            type = item1.h.J_Employment_Type
                                        });
                                    }
                                }
                            }
                        }
                    }

                }
                else if (User.IsInRole("Manager"))
                {
                    var offerlist = (from e in context.User_Master
                                     join
                                         f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                     where (f.O_Teamlead != null) && (f.O_Manager_Status == null)
                                     join
                                         g in context.Resume_Master on f.O_Rid equals g.R_Id
                                     join
                                         h in context.Requirement_Master on g.R_Jid equals h.J_Id
                                     join
                                          i in context.Client_Master on h.J_Client_Id equals i.C_id

                                     select new { e, f, g, h, i }).ToList();
                    if (offerlist != null)
                    {
                        foreach (var item1 in offerlist)
                        {
                            vm.Add(new ViewOfferModel
                            {
                                oid = item1.f.O_Id,
                                name = item1.g.R_Name,
                                client = item1.i.C_Name,
                                location = item1.h.J_Location,
                                skill = item1.h.J_Skill,
                                status = item1.f.O_Status,
                                type = item1.h.J_Employment_Type
                            });
                        }
                    }
                }
                else
                {
                    var offerlist = (from e in context.User_Master
                                     join
                                         f in context.Offer_Master on e.E_UserName equals f.O_Recruiter
                                     where (f.O_Teamlead != null) && (f.O_Manager_Status != null) && (f.O_Admin_Status == null)
                                     join
                                         g in context.Resume_Master on f.O_Rid equals g.R_Id
                                     join
                                         h in context.Requirement_Master on g.R_Jid equals h.J_Id
                                     join
                                          i in context.Client_Master on h.J_Client_Id equals i.C_id

                                     select new { e, f, g, h, i }).ToList();
                    if (offerlist != null)
                    {
                        foreach (var item1 in offerlist)
                        {
                            vm.Add(new ViewOfferModel
                            {
                                oid = item1.f.O_Id,
                                name = item1.g.R_Name,
                                client = item1.i.C_Name,
                                location = item1.h.J_Location,
                                skill = item1.h.J_Skill,
                                status = item1.f.O_Status,
                                type = item1.h.J_Employment_Type
                            });
                        }
                    }
                }
                return Json(new { data = vm }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ApproveOfferData(string ofid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                decimal id = Convert.ToDecimal(ofid);
                var findofferdata = context.Offer_Master.Where(c => c.O_Id == id).FirstOrDefault();
                if (findofferdata != null)
                {
                    if (User.IsInRole("Manager"))
                    {
                        findofferdata.O_Manager_Status = "Approve";
                        findofferdata.O_Manager_Date = System.DateTime.Now;
                    }
                    if (User.IsInRole("Admin"))
                    {
                        findofferdata.O_Admin_Status = "Approve";
                        findofferdata.O_Admin_Date = System.DateTime.Now;
                    }
                    context.Entry(findofferdata).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Approve Offer Data", System.DateTime.Now);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Changetobejoinstatus(string oid, string stat)
        {
            using (var context = new ATS2019_dbEntities())
            {
                decimal id = Convert.ToDecimal(oid);
                var findtobejoindata = context.Offer_Master.Where(c => c.O_Id == id).FirstOrDefault();
                if (findtobejoindata != null)
                {
                    findtobejoindata.O_Status = stat;
                    context.Entry(findtobejoindata).State = EntityState.Modified;
                    context.SaveChanges();
                    CreateActivityLog(User.Identity.Name, "Change To BE Join Status", System.DateTime.Now);
                }
                return Json(new { data = true }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult ViewRevenuewReportDatewise()
        {
            return View();
        }
        public JsonResult LoadDateWiseRevenueReportData(DateTime ssd, DateTime eed)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var revnuedata = (from c in context.Offer_Master
                                  where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(ssd) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(eed)
                                  join
                                      d in context.Resume_Master on c.O_Rid equals d.R_Id
                                  join
                                      e in context.Requirement_Master on c.O_Jid equals e.J_Id
                                  join
                                      f in context.Client_Master on e.J_Client_Id equals f.C_id
                                  join
                                      g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                  select new
                                  {
                                      c,
                                      d,
                                      e,
                                      f,
                                      g
                                  }).ToList();
                return Json(new { data = revnuedata }, JsonRequestBehavior.AllowGet);

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

        private void SendEmail(string tomail, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("requirement@archesoftronix.in");
            string[] tolist = tomail.Split(',');
            foreach (var item in tolist)
            {
                mail.To.Add(new MailAddress(item.ToString()));
            }
            mail.To.Add(new MailAddress("ajay.zala@archesoftronix.com"));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "mail.archesoftronix.in";
            smtp.Port = 587;
            smtp.EnableSsl = false;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new System.Net.NetworkCredential("requirement@archesoftronix.in", "2010@Arche", "");
            smtp.Send(mail);


        }
        [HttpGet]
        public ActionResult ViewATSActivity()
        {
            return View();
        }

        public JsonResult LoadAtsActivity()
        {
            using (var context = new ATS2019_dbEntities())
            {
                List<ActivityViewModel> acm = new List<ActivityViewModel>();
                var activitylist = (from c in context.User_Activity_Master join d in context.User_Master on c.Username equals d.E_UserName orderby c.Id descending select new { c, d }).ToList().Take(100);
                foreach (var item in activitylist)
                {
                    acm.Add(new ActivityViewModel
                    {
                        name = item.d.E_Fullname,
                        activity = item.c.Activity,
                        activitytime = item.c.ActivityOn.ToString()
                    });
                }
                return Json(new { data = acm }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ViewMonthIOST()
        {
            return View();
        }
        public JsonResult LoadTodayInterviewData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var interviewlist = (from c in context.Interview_Master
                                     where EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                     join
                                         d in context.Resume_Master on c.I_RId equals d.R_Id
                                     select new { c, d }).ToList();
                return Json(new { data = interviewlist }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult LoadMonthOfferData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var offerlist = (from c in context.Offer_Master
                                 where c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.User_Master on c.O_Recruiter equals e.E_UserName
                                 join
                                     f in context.Requirement_Master on d.R_Jid equals f.J_Id
                                 join
                                 g in context.Client_Master on f.J_Client_Id equals g.C_id
                                 select new { c, d, e, f, g }).ToList();
                return Json(new { data = offerlist }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult LoadMonthStartData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var startlist = (from c in context.Offer_Master
                                 where c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "Join"
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.User_Master on c.O_Recruiter equals e.E_UserName
                                 join
                                     f in context.Requirement_Master on d.R_Jid equals f.J_Id
                                 join
                                 g in context.Client_Master on f.J_Client_Id equals g.C_id
                                 select new { c, d, e, f, g }).ToList();
                return Json(new { data = startlist }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult LoadMonthTBJDData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var tobejoinlist = (from c in context.Offer_Master
                                    where c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "To be join"
                                    join
                                        d in context.Resume_Master on c.O_Rid equals d.R_Id
                                    join
                                        e in context.User_Master on c.O_Recruiter equals e.E_UserName
                                    join
                                        f in context.Requirement_Master on d.R_Jid equals f.J_Id
                                    join
                                    g in context.Client_Master on f.J_Client_Id equals g.C_id
                                    select new { c, d, e, f, g }).ToList();
                return Json(new { data = tobejoinlist }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ViewProfile()
        {
            using (var context = new ATS2019_dbEntities())
            {
                UserModel um = new UserModel();
                var finduserdata = (from c in context.User_Master where c.E_UserName == User.Identity.Name select c).FirstOrDefault();
                if (finduserdata != null)
                {
                    um.EId = finduserdata.E_Id;
                    um.ECode = finduserdata.E_Code;
                    um.EContNo = finduserdata.E_ContNo;
                    um.EDepartment = finduserdata.E_Department;
                    um.EDesignation = finduserdata.E_Designation;
                    um.EFullname = finduserdata.E_Fullname;
                    um.EGender = finduserdata.E_Gender;
                    um.ELocation = finduserdata.E_Location;
                    um.Ephoto = finduserdata.E_Photo;
                    um.EAddress = finduserdata.E_Address;
                    um.EBloodGroup = finduserdata.E_BloodGroup;
                    um.EDOB = finduserdata.E_DOB;
                    um.EEmail = finduserdata.E_UserName;
                }
                return View(um);
            }

        }
        //Requirement Progress Report
        public ActionResult ViewRequirementProgressReport()
        {
            return View();
        }
        public JsonResult LoadRequirementProgressReport()
        {
            List<ClientRequirementSIOHReport_Model> sior = new List<ClientRequirementSIOHReport_Model>();
            using (var context = new ATS2019_dbEntities())
            {

                var reqlist = (from c in context.Requirement_Master
                               join
                                   d in context.Client_Master on c.J_Client_Id equals d.C_id
                               select new { c, d }).ToList().OrderByDescending(m => m.c.J_CreatedOn);
                foreach (var item in reqlist)
                {
                    int subcnt = (from e in context.Resume_Master where e.R_Jid == item.c.J_Id && e.R_Status == "Accept" select e).Count();

                    int intcnt = (from f in context.Interview_Master where f.I_JId == item.c.J_Id && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show") select f).Count();

                    int offcnt = (from g in context.Offer_Master where g.O_Jid == item.c.J_Id select g).Count();

                    int hirecnt = (from h in context.Offer_Master where h.O_Jid == item.c.J_Id && h.O_Status == "Join" select h).Count();

                    int bdcnt = (from i in context.Offer_Master where i.O_Jid == item.c.J_Id && i.O_Status == "BD" select i).Count();
                    string createddate = DateTime.Parse(Convert.ToDateTime(item.c.J_CreatedOn).ToShortDateString()).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                    sior.Add(new ClientRequirementSIOHReport_Model
                    {
                        client = item.d.C_Name,
                        skill = item.c.J_Skill,
                        jobcode = item.c.J_Code,
                        position = item.c.J_Position,
                        submission = subcnt,
                        interview = intcnt,
                        offer = offcnt,
                        hire = hirecnt,
                        bd = bdcnt,
                        type = item.c.J_Employment_Type,
                        status = item.c.J_Status,
                        location = item.c.J_Location,
                        createdon = createddate

                    });
                }
                return Json(new { data = sior }, JsonRequestBehavior.AllowGet);
            }

        }

        public void facebookpost()
        {
            string app_id = "1516372765359284";
            string app_secreat = "25669cce2ed495850ac330bd8e56f1c3";
            string scope = "manage_pages, pages_show_list, publish_pages, pages_messaging, public_profile";
            if (Request["code"] == null)
            {
                Response.Redirect(string.Format("https://facebook.graph.com/oauth/access_token?client_id={0}&redirect_uri={1}&scope={2}", app_id, Request.Url.AbsoluteUri, scope));
            }
            else
            {
                Dictionary<string, string> tokens = new Dictionary<string, string>();
                string url = string.Format("https://grpah.facebqook.com/oauth/access_token?client_id={0}&redirect_uri={1}&scope={2}&code={2}&client_secreat{3}", app_id, Request.Url.AbsoluteUri, scope, Request["code"], app_secreat);

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string vals = reader.ReadToEnd();
                    foreach (string token in vals.Split('&'))
                    {
                        tokens.Add(token.Substring(0, token.IndexOf("=")), token.Substring(token.IndexOf("=") + 1, token.Length - token.IndexOf("=") - 1));
                    }
                    var access_token = "EAAJdDhRv89ABAK2GO6qfsknoY90j3ZAxwnmrkAu89bxteQXd6SjVrCLfvoV3ZClZAtW82SQFV2Q4RbwcsNOZB6j44vOhTUx1ZChntJxyoE1ZAF6WMv5E0DULX3B6TYNMIJp4Y7jBtXk6SMPrSFRH8xQZAewRZCiyiurRC9Y8DZCU6RwZDZD";
                    var client = new FacebookClient(access_token);
                    client.Post("/me/feed", new { message = "Hello" });
                }

            }
        }
        //get today percent 
        public JsonResult gettodaysubmissiontointerviewpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tsubcnt = 0, tintcnt = 0;
                float stiper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();

                    tintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();
                    }
                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.R_Status == "Accept" select c).Count();

                    tintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                    tintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();

                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult gettodaysubmissiontoofferpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tsubcnt = 0, toffcnt = 0;
                float stoper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();

                    toffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                    stoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (stoper > 0)
                    {
                        val = stoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        toffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                    select new { c, d }).Count();

                    }
                    stoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (stoper > 0)
                    {
                        val = stoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.R_Status == "Accept" select c).Count();

                    toffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now))).ToList().Count();

                    stoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (stoper > 0)
                    {
                        val = stoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    toffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();

                    stoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (stoper > 0)
                    {
                        val = stoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult gettodaysubmissiontohirepercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tsubcnt = 0, tstartcnt = 0;
                float sthper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();

                    tstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now)
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();

                    sthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (sthper > 0)
                    {
                        val = sthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && d.O_Status == "Join"
                                      select new { c, d }).Count();
                    }
                    sthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (sthper > 0)
                    {
                        val = sthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.R_Status == "Accept" select c).Count();

                    tstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && c.O_Status == "Join")).ToList().Count();

                    sthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (sthper > 0)
                    {
                        val = sthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    tstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(System.DateTime.Now) && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();

                    sthper = (float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (sthper > 0)
                    {
                        val = sthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Yesterday Count Data
        public JsonResult getyesterdaysubmissioncount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                int tysubcnt = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tysubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && (c.R_Status == "Accept"))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tysubcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Resume_Master on e.J_Id equals f.R_Jid
                                where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && f.R_Status == "Accept"
                                select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tysubcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                     where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                     select new { c, d }
                                  ).Count();
                    }
                    // return Json(new { tysubcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tysubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && c.R_Status == "Accept" select c).Count();
                    //context.Resume_Master.Where((c => c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();
                }
                if (tysubcnt > 0)
                {
                    val = tysubcnt.ToString("0");
                }
                else
                {
                    val = "-";
                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult getyesterdayinterviewcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                int tyintcnt = 0;
                string val = "";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    tyintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tyintcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Interview_Master on e.J_Id equals f.I_JId
                                where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tyintcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                     where
                                          EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                     select new { c, f }).Count();
                    }
                    // return Json(new { tyintcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tyintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }

                if (tyintcnt > 0)
                {
                    val = tyintcnt.ToString("0");
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getyesterdayoffercount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = DateTime.Now.AddDays(-1);
                int tyoffcnt = 0;
                string val = "";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    tyoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tyoffcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Offer_Master on e.J_Id equals f.O_Jid
                                where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                join
                                    g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                join
                                    h in context.Resume_Master on f.O_Rid equals h.R_Id
                                join
                                    i in context.Client_Master on e.J_Client_Id equals i.C_id
                                select new { c, d, e, f, g, h, i }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tyoffcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                     where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                     select new { c, d }).Count();
                    }
                    // return Json(new { toffcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tyoffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday))).ToList().Count();
                }
                if (tyoffcnt > 0)
                {
                    val = tyoffcnt.ToString("0");
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getyesterdaystartcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                int tystartcnt = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tystartcnt = (from c in context.Offer_Master
                                  where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday)
                                  join
                                      d in context.Resume_Master on c.O_Rid equals d.R_Id
                                  join
                                      e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                  join
                                      f in context.Client_Master on e.J_Client_Id equals f.C_id
                                  join
                                      g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                  select new { c, d, e, f, g }).Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tystartcnt = (from c in context.User_Master
                                  where c.E_UserName == User.Identity.Name
                                  join
                                      d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                  join
                                      e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                  join
                                      f in context.Offer_Master on e.J_Id equals f.O_Jid
                                  where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && f.O_Status == "Join"
                                  select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tystartcnt += (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && d.O_Status == "Join"
                                       select new { c, d }).Count();
                    }
                    // return Json(new { tstartcnt }, JsonRequestBehavior.AllowGet);
                }
                {
                    tystartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && c.O_Status == "Join")).ToList().Count();
                }
                if (tystartcnt > 0)
                {
                    val = tystartcnt.ToString("0");
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getyesterdayBDcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = System.DateTime.Now.AddDays(-1);
                int tybdcnt = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tybdcnt = (from c in context.Offer_Master
                               where c.O_Status == "BD" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday)
                               join
                                   d in context.Resume_Master on c.O_Rid equals d.R_Id
                               join
                                   e in context.Requirement_Master on d.R_Jid equals e.J_Id
                               join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                               join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                               select new { c, d, e, f, g }).Count();
                    if (tybdcnt > 0)
                    {
                        val = tybdcnt.ToString("0");
                    }
                    else
                    {
                        val = "-";
                    }
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getyesterdaysubmissiontointerviewpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = DateTime.Now.AddDays(-1);
                int tsubcnt = 0, tintcnt = 0;
                float stiper = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && (c.R_Status == "Accept"))).ToList().Count();

                    tintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();
                    }
                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && c.R_Status == "Accept" select c).Count();

                    tintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.I_Date) == EntityFunctions.TruncateTime(yesterday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                    tintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where EntityFunctions.TruncateTime(f.I_Date) == EntityFunctions.TruncateTime(yesterday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();

                    stiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (stiper > 0)
                    {
                        val = stiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult getyesterdaysubmissiontoofferpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = DateTime.Now.AddDays(-1);
                int tsubcnt = 0, toffcnt = 0;
                float ystoper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && (c.R_Status == "Accept"))).ToList().Count();

                    toffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday))).ToList().Count();

                    ystoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (ystoper > 0)
                    {
                        val = ystoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        toffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where EntityFunctions.TruncateTime(d.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                                    select new { c, d }).Count();

                    }
                    ystoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (ystoper > 0)
                    {
                        val = ystoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && c.R_Status == "Accept" select c).Count();

                    toffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Off_Date) == EntityFunctions.TruncateTime(yesterday))).ToList().Count();

                    ystoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (ystoper > 0)
                    {
                        val = ystoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    toffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(yesterday)
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();

                    ystoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (ystoper > 0)
                    {
                        val = ystoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult getyesterdaysubmissiontohirepercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime yesterday = DateTime.Now.AddDays(-1);
                int tsubcnt = 0, tstartcnt = 0;
                float ysthper = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && (c.R_Status == "Accept"))).ToList().Count();

                    tstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday)
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();

                    ysthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));
                    if (ysthper > 0)
                    {
                        val = ysthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where EntityFunctions.TruncateTime(d.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && d.O_Status == "Join"
                                      select new { c, d }).Count();
                    }
                    ysthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (ysthper > 0)
                    {
                        val = ysthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && c.R_Status == "Accept" select c).Count();

                    tstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && c.O_Status == "Join")).ToList().Count();

                    ysthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (ysthper > 0)
                    {
                        val = ysthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) == EntityFunctions.TruncateTime(yesterday) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    tstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where EntityFunctions.TruncateTime(f.O_Join_Date) == EntityFunctions.TruncateTime(yesterday) && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();

                    ysthper = (float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (ysthper > 0)
                    {
                        val = ysthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // week count data
        public JsonResult getweeksubmissioncount()
        {
            using (var context = new ATS2019_dbEntities())
            {

                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                string val = "-";

                int twsubcnt = 0;
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    twsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && (c.R_Status == "Accept"))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    twsubcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Resume_Master on e.J_Id equals f.R_Jid
                                where EntityFunctions.TruncateTime(f.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && f.R_Status == "Accept"
                                select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        twsubcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                     where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                                     select new { c, d }
                                  ).Count();
                    }
                    // return Json(new { tysubcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    twsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Endaday) && c.R_Status == "Accept" select c).Count();
                    //context.Resume_Master.Where((c => c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();
                }

                if (twsubcnt > 0)
                {
                    val = twsubcnt.ToString();
                }
                else
                {
                    val.ToString();
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult getweekinterviewcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                int twintcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    twintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    twintcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Interview_Master on e.J_Id equals f.I_JId
                                where EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        twintcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                     where
                                          EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                     select new { c, f }).Count();
                    }
                    // return Json(new { tyintcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    twintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }

                if (twintcnt > 0)
                {
                    val = twintcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getweekoffercount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                int twoffcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    twoffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday))).ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    twoffcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Offer_Master on e.J_Id equals f.O_Jid
                                where EntityFunctions.TruncateTime(f.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Off_Date) == EntityFunctions.TruncateTime(Endaday)
                                join
                                    g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                join
                                    h in context.Resume_Master on f.O_Rid equals h.R_Id
                                join
                                    i in context.Client_Master on e.J_Client_Id equals i.C_id
                                select new { c, d, e, f, g, h, i }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        twoffcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                     where EntityFunctions.TruncateTime(d.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Off_Date) <= EntityFunctions.TruncateTime(Firstday)
                                     select new { c, d }).Count();
                    }
                    // return Json(new { toffcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    twoffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday))).ToList().Count();
                }

                if (twoffcnt > 0)
                {
                    val = twoffcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getweekstartcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                int twstartcnt = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    twstartcnt = (from c in context.Offer_Master
                                  where c.O_Status == "Join" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                                  join
                                      d in context.Resume_Master on c.O_Rid equals d.R_Id
                                  join
                                      e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                  join
                                      f in context.Client_Master on e.J_Client_Id equals f.C_id
                                  join
                                      g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                  select new { c, d, e, f, g }).Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    twstartcnt = (from c in context.User_Master
                                  where c.E_UserName == User.Identity.Name
                                  join
                                      d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                  join
                                      e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                  join
                                      f in context.Offer_Master on e.J_Id equals f.O_Jid
                                  where EntityFunctions.TruncateTime(f.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Join_Date) >= EntityFunctions.TruncateTime(Endaday) && f.O_Status == "Join"
                                  select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        twstartcnt += (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       where EntityFunctions.TruncateTime(d.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && d.O_Status == "Join"
                                       select new { c, d }).Count();
                    }
                    // return Json(new { tstartcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    twstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && c.O_Status == "Join")).ToList().Count();
                }
                if (twstartcnt > 0)
                {
                    val = twstartcnt.ToString();
                }
                else
                {
                    val = "-";
                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getweekBDcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                string val = "-";
                int twbdcnt = 0;
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    twbdcnt = (from c in context.Offer_Master
                               where c.O_Status == "BD" && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday)
                               join
                                   d in context.Resume_Master on c.O_Rid equals d.R_Id
                               join
                                   e in context.Requirement_Master on d.R_Jid equals e.J_Id
                               join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                               join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                               select new { c, d, e, f, g }).Count();
                }

                if (twbdcnt > 0)
                {
                    val = twbdcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getweeksubmissiontointerviewpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                string val = "-";
                int tsubcnt = 0, tintcnt = 0;
                float wstiper = 0;
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && (c.R_Status == "Accept"))).ToList().Count();

                    tintcnt = context.Interview_Master.Where((c => EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    wstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (wstiper > 0)
                    {
                        val = wstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();
                    }
                    wstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (wstiper > 0)
                    {
                        val = wstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Endaday) && c.R_Status == "Accept" select c).Count();

                    tintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    wstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (wstiper > 0)
                    {
                        val = wstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                    tintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where EntityFunctions.TruncateTime(f.I_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.I_Date) <= EntityFunctions.TruncateTime(Endaday) && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();

                    wstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (wstiper > 0)
                    {
                        val = wstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult getweeksubmissiontoofferpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                int tsubcnt = 0, toffcnt = 0;
                float wstoper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && (c.R_Status == "Accept"))).ToList().Count();

                    toffcnt = context.Offer_Master.Where((c => EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday))).ToList().Count();

                    wstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (wstoper > 0)
                    {
                        val = wstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        toffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where EntityFunctions.TruncateTime(d.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                                    select new { c, d }).Count();

                    }
                    wstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (wstoper > 0)
                    {
                        val = wstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && c.R_Status == "Accept" select c).Count();

                    toffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday))).ToList().Count();

                    wstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (wstoper > 0)
                    {
                        val = wstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    toffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where EntityFunctions.TruncateTime(f.O_Off_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Off_Date) <= EntityFunctions.TruncateTime(Endaday)
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();

                    wstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (wstoper > 0)
                    {
                        val = wstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult getweeksubmissiontohirepercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime Firstday = System.DateTime.Now.AddDays(-(int)System.DateTime.Now.DayOfWeek);
                DateTime Endaday = Firstday.AddDays(6);
                int tsubcnt = 0, tstartcnt = 0;
                float wsthper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && (c.R_Status == "Accept"))).ToList().Count();

                    tstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday)
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();

                    wsthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));
                    if (wsthper > 0)
                    {
                        val = wsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where EntityFunctions.TruncateTime(d.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where EntityFunctions.TruncateTime(d.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(d.O_Join_Date) >= EntityFunctions.TruncateTime(Endaday) && d.O_Status == "Join"
                                      select new { c, d }).Count();
                    }
                    wsthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (wsthper > 0)
                    {
                        val = wsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && c.R_Status == "Accept" select c).Count();

                    tstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && EntityFunctions.TruncateTime(c.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(c.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && c.O_Status == "Join")).ToList().Count();

                    wsthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (wsthper > 0)
                    {
                        val = wsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where EntityFunctions.TruncateTime(f.R_CreateOn) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.R_CreateOn) <= EntityFunctions.TruncateTime(Endaday) && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    tstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where EntityFunctions.TruncateTime(f.O_Join_Date) >= EntityFunctions.TruncateTime(Firstday) && EntityFunctions.TruncateTime(f.O_Join_Date) <= EntityFunctions.TruncateTime(Endaday) && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();

                    wsthper = (float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (wsthper > 0)
                    {
                        val = wsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // month count data
        public JsonResult getmonthsubmissiontointerviewpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {

                int tsubcnt = 0, tintcnt = 0;
                float mstiper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where(c => c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && (c.R_Status == "Accept")).ToList().Count();

                    tintcnt = context.Interview_Master.Where((c => c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    mstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (mstiper > 0)
                    {
                        val = mstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();
                    }
                    mstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (mstiper > 0)
                    {
                        val = mstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept" select c).Count();

                    tintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && c.I_Date.Month == System.DateTime.Now.Month && c.I_Date.Year == System.DateTime.Now.Year && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    mstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (mstiper > 0)
                    {
                        val = mstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_CreateOn.Value.Month == System.DateTime.Now.Month && f.R_CreateOn.Value.Year == System.DateTime.Now.Year && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                    tintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where f.I_Date.Month == System.DateTime.Now.Month && f.I_Date.Year == System.DateTime.Now.Year && (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();

                    mstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (mstiper > 0)
                    {
                        val = mstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult getmonthsubmissiontoofferpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tsubcnt = 0, toffcnt = 0;
                float mstoper = 0;
                string val = "";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && (c.R_Status == "Accept"))).ToList().Count();

                    toffcnt = context.Offer_Master.Where((c => c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year)).ToList().Count();

                    mstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (mstoper > 0)
                    {
                        val = mstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        toffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    where d.O_Off_Date.Value.Month == System.DateTime.Now.Month && d.O_Off_Date.Value.Year == System.DateTime.Now.Year
                                    select new { c, d }).Count();

                    }
                    mstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (mstoper > 0)
                    {
                        val = mstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept" select c).Count();

                    toffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && c.O_Off_Date.Value.Month == System.DateTime.Now.Month && c.O_Off_Date.Value.Year == System.DateTime.Now.Year)).ToList().Count();

                    mstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (mstoper > 0)
                    {
                        val = mstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_CreateOn.Value.Month == System.DateTime.Now.Month && f.R_CreateOn.Value.Year == System.DateTime.Now.Year && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    toffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               where f.O_Off_Date.Value.Month == System.DateTime.Now.Month && f.O_Off_Date.Value.Year == System.DateTime.Now.Year
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();

                    mstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (mstoper > 0)
                    {
                        val = mstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult getmonthsubmissiontohirepercet()
        {
            using (var context = new ATS2019_dbEntities())
            {
                int tsubcnt = 0, tstartcnt = 0;
                float msthper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where((c => c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && (c.R_Status == "Accept"))).ToList().Count();

                    tstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join" && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();

                    msthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));
                    if (msthper > 0)
                    {
                        val = msthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_CreateOn.Value.Month == System.DateTime.Now.Month && d.R_CreateOn.Value.Year == System.DateTime.Now.Year && d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where d.O_Join_Date.Value.Month == System.DateTime.Now.Month && d.O_Join_Date.Value.Year == System.DateTime.Now.Year && d.O_Status == "Join"
                                      select new { c, d }).Count();
                    }
                    msthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (msthper > 0)
                    {
                        val = msthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_CreateOn.Value.Month == System.DateTime.Now.Month && c.R_CreateOn.Value.Year == System.DateTime.Now.Year && c.R_Status == "Accept" select c).Count();

                    tstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && c.O_Join_Date.Value.Month == System.DateTime.Now.Month && c.O_Join_Date.Value.Year == System.DateTime.Now.Year && c.O_Status == "Join")).ToList().Count();

                    msthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (msthper > 0)
                    {
                        val = msthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_CreateOn.Value.Month == System.DateTime.Now.Month && f.R_CreateOn.Value.Year == System.DateTime.Now.Year && f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    tstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where f.O_Join_Date.Value.Month == System.DateTime.Now.Month && f.O_Join_Date.Value.Year == System.DateTime.Now.Year && f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();

                    msthper = (float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (msthper > 0)
                    {
                        val = msthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //till date data count
        public JsonResult gettdsubmissioncount()
        {
            using (var context = new ATS2019_dbEntities())
            {

                int tdsubcnt = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {

                    tdsubcnt = context.Resume_Master.Where(c => c.R_Status == "Accept").ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tdsubcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Resume_Master on e.J_Id equals f.R_Jid
                                where f.R_Status == "Accept"
                                select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tdsubcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                     where d.R_Status == "Accept"
                                     select new { c, d }
                                  ).Count();
                    }
                    // return Json(new { tysubcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tdsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_Status == "Accept" select c).Count();
                    //context.Resume_Master.Where((c => c.R_CreateBy == User.Identity.Name && EntityFunctions.TruncateTime(c.R_CreateOn) == EntityFunctions.TruncateTime(System.DateTime.Now) && (c.R_Status == "Accept"))).ToList().Count();
                }
                if (tdsubcnt > 0)
                {

                    val = tdsubcnt.ToString();

                }
                else
                {
                    val = "-";

                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult gettdinterviewcount()
        {
            using (var context = new ATS2019_dbEntities())
            {

                int tdintcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    tdintcnt = context.Interview_Master.Where(c => c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show").ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tdintcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Interview_Master on e.J_Id equals f.I_JId
                                where (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tdintcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                     where
(f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                     select new { c, f }).Count();
                    }
                    // return Json(new { tyintcnt }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    tdintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();
                }

                if (tdintcnt > 0)
                {
                    val = tdintcnt.ToString();

                }
                else
                {
                    val = "-";

                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gettdoffercount()
        {
            using (var context = new ATS2019_dbEntities())
            {

                int tdoffcnt = 0;
                string val = "-";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    tdoffcnt = context.Offer_Master.ToList().Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tdoffcnt = (from c in context.User_Master
                                where c.E_UserName == User.Identity.Name
                                join
                                    d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                join
                                    e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                join
                                    f in context.Offer_Master on e.J_Id equals f.O_Jid
                                join
                                    g in context.User_Master on f.O_Recruiter equals g.E_UserName
                                join
                                    h in context.Resume_Master on f.O_Rid equals h.R_Id
                                join
                                    i in context.Client_Master on e.J_Client_Id equals i.C_id
                                select new { c, d, e, f, g, h, i }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tdoffcnt += (from c in context.User_Master
                                     where c.E_Fullname == item
                                     join
                                         d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                     select new { c, d }).Count();
                    }
                    // return Json(new { toffcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tdoffcnt = context.Offer_Master.Where(c => c.O_Recruiter == User.Identity.Name).ToList().Count();
                }
                if (tdoffcnt > 0)
                {
                    val = tdoffcnt.ToString();

                }
                else
                {
                    val = "-";

                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult gettdstartcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                string val = "-";
                int tdstartcnt = 0;

                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tdstartcnt = (from c in context.Offer_Master
                                  where c.O_Status == "Join"
                                  join
                                      d in context.Resume_Master on c.O_Rid equals d.R_Id
                                  join
                                      e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                  join
                                      f in context.Client_Master on e.J_Client_Id equals f.C_id
                                  join
                                      g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                  select new { c, d, e, f, g }).Count();
                }
                else if (User.IsInRole("Sales"))
                {
                    tdstartcnt = (from c in context.User_Master
                                  where c.E_UserName == User.Identity.Name
                                  join
                                      d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                  join
                                      e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                  join
                                      f in context.Offer_Master on e.J_Id equals f.O_Jid
                                  where f.O_Status == "Join"
                                  select new { c, d, e, f }).ToList().Count();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tdstartcnt += (from c in context.User_Master
                                       where c.E_Fullname == item
                                       join
                                           d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                       where d.O_Status == "Join"
                                       select new { c, d }).Count();
                    }
                    // return Json(new { tstartcnt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tdstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && c.O_Status == "Join")).ToList().Count();
                }
                if (tdstartcnt > 0)
                {

                    val = tdstartcnt.ToString();

                }
                else
                {
                    val = "-";

                }

                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gettdBDcount()
        {
            using (var context = new ATS2019_dbEntities())
            {
                string val = "-";

                int tdbdcnt = 0;
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tdbdcnt = (from c in context.Offer_Master
                               where c.O_Status == "BD"
                               join
                                   d in context.Resume_Master on c.O_Rid equals d.R_Id
                               join
                                   e in context.Requirement_Master on d.R_Jid equals e.J_Id
                               join
                                   f in context.Client_Master on e.J_Client_Id equals f.C_id
                               join
                                   g in context.User_Master on c.O_Recruiter equals g.E_UserName
                               select new { c, d, e, f, g }).Count();
                }
                if (tdbdcnt > 0)
                {
                    val = tdbdcnt.ToString();
                }
                else
                {
                    val = "-";
                }
                return Json(new { val }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult gettdsubmissiontointerviewpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {

                int tsubcnt = 0, tintcnt = 0;
                float tdstiper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where(c => c.R_Status == "Accept").ToList().Count();

                    tintcnt = context.Interview_Master.Where(c => c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show").ToList().Count();

                    tdstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (tdstiper > 0)
                    {
                        val = tdstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tintcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        f in context.Interview_Master on c.E_UserName equals f.I_CreateBy
                                    where
                                         (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                                    select new { c, f }).Count();
                    }
                    tdstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (tdstiper > 0)
                    {
                        val = tdstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_Status == "Accept" select c).Count();

                    tintcnt = context.Interview_Master.Where((c => c.I_CreateBy == User.Identity.Name && (c.I_Status != "Set" && c.I_Status != "Re Schedule" && c.I_Status != "No Show"))).ToList().Count();

                    tdstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (tdstiper > 0)
                    {
                        val = tdstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();
                    tintcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Interview_Master on e.J_Id equals f.I_JId
                               where (f.I_Status != "Set" && f.I_Status != "Re Schedule" && f.I_Status != "No Show")
                               select new { c, d, e, f }).ToList().Count();

                    tdstiper = (float.Parse(tintcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());
                    if (tdstiper > 0)
                    {
                        val = tdstiper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult gettdsubmissiontoofferpercet()
        {
            using (var context = new ATS2019_dbEntities())
            {

                int tsubcnt = 0, toffcnt = 0;
                float tdstoper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where(c => c.R_Status == "Accept").ToList().Count();

                    toffcnt = context.Offer_Master.ToList().Count();

                    tdstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (tdstoper > 0)
                    {
                        val = tdstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        toffcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                    select new { c, d }).Count();

                    }
                    tdstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (tdstoper > 0)
                    {
                        val = tdstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_Status == "Accept" select c).Count();

                    toffcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name)).ToList().Count();

                    tdstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (tdstoper > 0)
                    {
                        val = tdstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    toffcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Offer_Master on e.J_Id equals f.O_Jid
                               join
                                   g in context.User_Master on f.O_Recruiter equals g.E_UserName
                               join
                                   h in context.Resume_Master on f.O_Rid equals h.R_Id
                               join
                                   i in context.Client_Master on e.J_Client_Id equals i.C_id
                               select new { c, d, e, f, g, h, i }).ToList().Count();

                    tdstoper = (float.Parse(toffcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (tdstoper > 0)
                    {
                        val = tdstoper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult gettdsubmissiontohirepercet()
        {
            using (var context = new ATS2019_dbEntities())
            {

                int tsubcnt = 0, tstartcnt = 0;
                float tdsthper = 0;
                string val = "-";
                if ((User.IsInRole("Admin")) || (User.IsInRole("Manager")))
                {
                    tsubcnt = context.Resume_Master.Where(c => c.R_Status == "Accept").ToList().Count();

                    tstartcnt = (from c in context.Offer_Master
                                 where c.O_Status == "Join"
                                 join
                                     d in context.Resume_Master on c.O_Rid equals d.R_Id
                                 join
                                     e in context.Requirement_Master on d.R_Jid equals e.J_Id
                                 join
                                     f in context.Client_Master on e.J_Client_Id equals f.C_id
                                 join
                                     g in context.User_Master on c.O_Recruiter equals g.E_UserName
                                 select new { c, d, e, f, g }).Count();

                    tdsthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (tdsthper > 0)
                    {
                        val = tdsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);

                }
                else if (User.IsInRole("Team Lead"))
                {
                    string tlname = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Fullname).FirstOrDefault();

                    string temlist = context.Team_Master.Where(c => c.T_TeamLead == tlname).Select(c => c.T_TeamMember).FirstOrDefault();

                    string[] tm = temlist.Split(',');
                    foreach (var item in tm)
                    {
                        tsubcnt += (from c in context.User_Master
                                    where c.E_Fullname == item
                                    join
                                        d in context.Resume_Master on c.E_UserName equals d.R_CreateBy
                                    where d.R_Status == "Accept"
                                    select new { c, d }
                                  ).Count();

                        tstartcnt += (from c in context.User_Master
                                      where c.E_Fullname == item
                                      join
                                          d in context.Offer_Master on c.E_UserName equals d.O_Recruiter
                                      where d.O_Status == "Join"
                                      select new { c, d }).Count();
                    }
                    tdsthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (tdsthper > 0)
                    {
                        val = tdsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }

                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Recruiter"))
                {
                    tsubcnt = (from c in context.Resume_Master where c.R_CreateBy == User.Identity.Name && c.R_Status == "Accept" select c).Count();

                    tstartcnt = context.Offer_Master.Where((c => c.O_Recruiter == User.Identity.Name && c.O_Status == "Join")).ToList().Count();

                    tdsthper = ((float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString()));

                    if (tdsthper > 0)
                    {
                        val = tdsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else if (User.IsInRole("Sales"))
                {
                    tsubcnt = (from c in context.User_Master
                               where c.E_UserName == User.Identity.Name
                               join
                                   d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                               join
                                   e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                               join
                                   f in context.Resume_Master on e.J_Id equals f.R_Jid
                               where f.R_Status == "Accept"
                               select new { c, d, e, f }).ToList().Count();

                    tstartcnt = (from c in context.User_Master
                                 where c.E_UserName == User.Identity.Name
                                 join
                                     d in context.SalesClient_Master on c.E_Code equals d.SC_UID
                                 join
                                     e in context.Requirement_Master on d.SC_CID equals e.J_Client_Id
                                 join
                                     f in context.Offer_Master on e.J_Id equals f.O_Jid
                                 where f.O_Status == "Join"
                                 select new { c, d, e, f }).ToList().Count();

                    tdsthper = (float.Parse(tstartcnt.ToString()) * float.Parse("100")) / float.Parse(tsubcnt.ToString());

                    if (tdsthper > 0)
                    {
                        val = tdsthper.ToString("0.00");
                    }
                    else
                    {
                        val = "-";
                    }
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { val }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpGet]
        public ActionResult CreateClientCallRecord()
        {
            if (User.IsInRole("Sales"))
            {
                SalesClientList();
            }
            else
            {
                Clientlist();
            }
            return View();
        }
        [HttpPost]
        public ActionResult SalesClientCallRecordAdd(SalesClientCallModel sccm)
        {
            using (var ats = new ATS2019_dbEntities())
            {
                SalesClientCall_Master sc = new SalesClientCall_Master();
                sc.scr_date = sccm.dt;
                sc.scr_client = sccm.client;
                sc.scr_poc = sccm.poc;
                sc.scr_agenda = sccm.agenda;
                sc.scr_createdby = User.Identity.Name;
                sc.scr_creaton = System.DateTime.Now;
                ats.SalesClientCall_Master.Add(sc);
                ats.SaveChanges();

                TempData["msg"] = "Record Added Successfully";
                return RedirectToAction("CreateClientCallRecord");
            }

        }

        [HttpGet]
        public ActionResult ViewClientCallRecord()
        {
            return View();
        }

        public JsonResult Viewclientcalldata()
        {
            using (var context = new ATS2019_dbEntities())
            {
                if (User.IsInRole("Admin"))
                {
                    var calllist = (from c in context.SalesClientCall_Master
                                    join
                                        d in context.User_Master on c.scr_createdby equals d.E_UserName

                                    select new { c, d }).ToList();
                    return Json(new { data = calllist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var calllist = (from c in context.SalesClientCall_Master
                                    join
                                        d in context.User_Master on c.scr_createdby equals d.E_UserName
                                    where d.E_UserName == User.Identity.Name

                                    select new { c, d }).ToList();
                    return Json(new { data = calllist }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Registrationlink()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Sendlink(string empid)
        {
            if (empid != null)
            {
                string mbody = "<b>Hello,</b>";
                mbody += "<br><br>";
                mbody += "User registration link.";
                mbody += "<br><br>";
                mbody += "Registration link : <a href='http://www.archesoftronix.in/Register/Registration'>Register</a>";
                mbody += "<br><br>";
                mbody += "Login Link : <a href='http://www.archesoftronix.in'>Login</a>";
                mbody += "<br><br>";
                mbody += "Thank You";
                mbody += "<br>";
                mbody += "Arche Softronix Pvt Ltd";


                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("requirement@archesoftronix.in");
                mail.To.Add(new MailAddress(empid.ToString()));
                mail.Subject = "New user registration link";
                mail.Body = mbody;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "mail.archesoftronix.in";
                smtp.Port = 587;
                smtp.EnableSsl = false;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = new System.Net.NetworkCredential("requirement@archesoftronix.in", "2010@Arche", "");
                smtp.Send(mail);
                ViewBag.msg = "Link send successfully.";
                return RedirectToAction("Registrationlink");
            }
            else
            {
                ViewBag.msg = "Error! Fail to send link.";
                return RedirectToAction("Registrationlink");
            }
        }

        //code to get day name from date value
        public JsonResult getDayname(string dd)
        {
            string dayName = DateTime.Parse(dd).ToString("ddddd");
            return Json(new { dayName }, JsonRequestBehavior.AllowGet);
        }


        //code to get requirement Summary

        public JsonResult UpdateRequirementSummary(int rjid)
        {

            using (var context = new ATS2019_dbEntities())
            {

                int subcount = (from c in context.Resume_Master where c.R_Jid == rjid && c.R_Status == "Accept" select c).ToList().Count();

                int intcount = (from d in context.Interview_Master where d.I_JId == rjid && (d.I_Status != "Set" || d.I_Status != "No Show") select d).ToList().Count();

                int offcount = (from e in context.Offer_Master where e.O_Jid == rjid select e).ToList().Count();

                int stacount = (from f in context.Offer_Master where f.O_Jid == rjid && f.O_Status == "Join" select f).ToList().Count();

                int bdcount = (from g in context.Offer_Master where g.O_Jid == rjid && g.O_Status == "BD" select g).ToList().Count();

                var result = new { sub = subcount, iter = intcount, off = offcount, sta = stacount, bd = bdcount, Jid = rjid };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        //Code for critical requirement list
        //requirement which submission not uploaded in last three days

        public ActionResult ViewCriticalRequirement()
        {
            return View();
        }

        public ActionResult LoadCriticalRequirementData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                DateTime comDate = System.DateTime.Now.AddDays(-3);
                var listRequirement = (from c in context.Requirement_Master where c.J_Status == "Active" select c).ToList();
                var jidlist = "";
                foreach (var req in listRequirement)
                {
                    var lastResumeUploaded = context.Resume_Master.Where(c => c.R_Jid == req.J_Id).OrderByDescending(c => c.R_CreateOn).FirstOrDefault();
                    if (lastResumeUploaded != null)
                    {
                        if (lastResumeUploaded.R_CreateOn != null)
                        {
                            if (lastResumeUploaded.R_CreateOn <= comDate)
                            {
                                if (jidlist == "")
                                {
                                    jidlist += lastResumeUploaded.R_Jid.ToString();
                                }
                                else
                                {
                                    jidlist += "," + lastResumeUploaded.R_Jid.ToString();
                                }
                            }
                        }
                    }
                }

                //var reqList1 = context.Requirement_Master.Where(c => jidlist.Contains(c.J_Id.ToString())).ToList().OrderByDescending(m => c.J_CreatedOn);
                var reqList = (from c in context.Requirement_Master
                               join d in context.Client_Master on c.J_Client_Id equals d.C_id
                               where jidlist.Contains(c.J_Id.ToString())
                               select new { c, d }).ToList().OrderByDescending(m => m.c.J_CreatedOn);
                return Json(new { data = reqList }, JsonRequestBehavior.AllowGet);
                //add code to view requirement and create page for view........
            }
        }

        public ActionResult EditRequirement(decimal? jid)
        {
            using (var context = new ATS2019_dbEntities())
            {
                totalClientlist();
                Alllist();
                RequirementModel rm = new RequirementModel();
                var req = context.Requirement_Master.Where(c => c.J_Id == jid).FirstOrDefault();
                rm.jid = req.J_Id;
                rm.jobcode = req.J_Code;
                rm.jclientid = req.J_Client_Id;
                rm.jskill = req.J_Skill;
                rm.jposition = req.J_Position;
                rm.jlocation = req.J_Location;
                rm.jendclient = req.J_EndClient;
                rm.jassignuser = req.J_AssignUser;
                rm.jtotmin = (float)req.J_Tot_Min_Exp;
                rm.jtotmax = (float)req.J_Tot_Max_Exp;
                rm.jrelmin = (float)req.J_Rel_Min_Exp;
                rm.jrelmax = (float)req.J_Rel_Max_Exp;
                rm.jbillrate = (decimal)req.J_Bill_Rate;
                rm.jpayrate = (decimal)req.J_Pay_Rate;
                rm.jcategory = req.J_Category;
                rm.jtype = req.J_Type;
                rm.jemployementtyp = req.J_Employment_Type;
                rm.jpoc = req.J_POC;
                rm.jpocno = (decimal)req.J_POC_No;
                rm.jjd = req.J_JD;
                rm.jmandatoryskill = req.J_MandatorySkill != null ? req.J_MandatorySkill : "";
                return View(rm);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditRequirement(RequirementModel rm)
        {
            using (var context = new ATS2019_dbEntities())
            {
                var req = context.Requirement_Master.Where(c => c.J_Id == rm.jid).FirstOrDefault();
                if (req != null)
                {
                    req.J_Code = rm.jobcode;
                    req.J_Client_Id = rm.jclientid;
                    req.J_Skill = rm.jskill;
                    req.J_Position = rm.jposition;
                    req.J_Location = rm.jlocation;
                    req.J_EndClient = rm.jendclient;
                    req.J_Tot_Min_Exp = rm.jtotmin;
                    req.J_Tot_Max_Exp = rm.jtotmax;
                    req.J_Rel_Min_Exp = rm.jrelmin;
                    req.J_Rel_Max_Exp = rm.jrelmax;
                    req.J_Bill_Rate = rm.jbillrate;
                    req.J_Pay_Rate = rm.jpayrate;
                    req.J_Category = rm.jcategory;
                    req.J_Type = rm.jtype;
                    req.J_Employment_Type = rm.jemployementtyp;
                    req.J_POC = rm.jpoc;
                    req.J_POC_No = rm.jpocno;
                    req.J_JD = rm.jjd;
                    req.J_MandatorySkill = rm.jmandatoryskill;
                    req.J_UpdatedBy = User.Identity.Name;
                    req.J_UpdatedOn = System.DateTime.Now;
                    context.Entry(req).State = EntityState.Modified;
                    context.SaveChanges();
                }

                return RedirectToAction("ViewRequirement");
            }

        }

        //General Resume Upload
        [HttpGet]
        public ActionResult UploadArcheResume()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadArcheResume(ArcheResumeModel arm, HttpPostedFileBase resume)
        {
            using (ATS2019_dbEntities context = new ATS2019_dbEntities())
            {
                if (ModelState.IsValid)
                {
                    if (resume != null)
                    {
                        string[] rf = resume.FileName.Split('.');
                        if ((rf[1].ToString().ToUpper() == "DOC") || (rf[1].ToString().ToUpper() == "DOCX") || (rf[1].ToString().ToUpper() == "PDF"))
                        {
                            var resumename = rf[0].ToString() + "-" + System.DateTime.Now.Year.ToString() + "." + rf[1].ToString();

                            Arche_Resume_Master arche_Resume_Master = new Arche_Resume_Master();
                            arche_Resume_Master.Name = arm.name;
                            arche_Resume_Master.Contact = arm.contact;
                            arche_Resume_Master.Email = arm.email;
                            arche_Resume_Master.DOB = arm.dob;
                            arche_Resume_Master.Skill = arm.skill;
                            arche_Resume_Master.Location = arm.location;
                            arche_Resume_Master.Experience = arm.experience;
                            arche_Resume_Master.Notice = arm.noticeperiod;
                            arche_Resume_Master.CTC = arm.ctc;
                            arche_Resume_Master.Resume = resumename;
                            arche_Resume_Master.CreatedBy = User.Identity.Name;
                            arche_Resume_Master.CreatedOn = System.DateTime.Now;
                            context.Arche_Resume_Master.Add(arche_Resume_Master);
                            context.SaveChanges();
                            resume.SaveAs(Server.MapPath("~/ResumeDirectory/") + resumename);
                            TempData["msg"] = "Profile uploaded successfully.";
                            ModelState.Clear();
                        }
                        return View();
                    }
                    else
                    {
                        TempData["msg"] = "Select Resume";
                        return View();
                    }
                }
                else
                {
                    return View();
                }


            }

        }

        public ActionResult ViewArcheResume()
        {
            return View();
        }
        public ActionResult LoadArcheResumeData()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var resumelist = context.Arche_Resume_Master.OrderByDescending(m => m.CreatedOn).ToList();
                return Json(new { data = resumelist }, JsonRequestBehavior.AllowGet);
            }

        }

        //Call External Attendacnce API
        public ActionResult ViewMonthAttendaceReport()
        {
            return View();
        }

        public ActionResult LoadMonthAttendanceReport()
        {
            var context = new ATS2019_dbEntities();
            var Empcode = context.User_Master.Where(x => x.E_UserName == User.Identity.Name).Select(x => x.E_Code).FirstOrDefault();

            AttendanceData students = null;
            var client = new System.Net.Http.HttpClient();
            //client.BaseAddress = new Uri("http://192.168.1.33:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getAttendance/");
            client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getAttendance/");
            // client.BaseAddress = new Uri("http://192.168.1.24:8080/attendance/getAttendance/");
            var result = client.GetAsync("?id=" + Empcode.ToString()).Result;
            if (result.IsSuccessStatusCode)
            {


                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var jsondata = result.Content.ReadAsStringAsync().Result;


                //Dictionary<string, object> dict = (Dictionary<string, object>)jsSerializer.DeserializeObject(data);
                //foreach (var item in dict)
                //{

                //}

                students = JsonConvert.DeserializeObject<AttendanceData>(jsondata);
                //List<AttendanceDataViewModel> attendanceDataViewModel = new List<AttendanceDataViewModel>();
                //int n = students.attendanceDate.Count;
                //for (int i = 0; i < n; i++)
                //{
                //    attendanceDataViewModel.Add(new AttendanceDataViewModel
                //    {
                //        attendanceDate = students.attendanceDate[i],
                //        intime = students.intime[i],
                //        outtime = students.outtime[i],
                //        totaltime = students.totaltime[i],
                //        firstHalf = students.firstHalf[i],
                //        secondHalf = students.secondHalf[i],
                //        shiftName = students.shiftName[i]
                //    });
                //}


                //Attendacedetails.Add("Total Present Days", students.totalpresent.ToString());
                //Attendacedetails.Add("Total Absent Days", students.totalabsent.ToString());
                //Attendacedetails.Add("Total Week Off", students.totalweekoff.ToString());
                //Attendacedetails.Add("Total Late Count", students.totalLatecount.ToString());
                //Attendacedetails.Add("Total Missing Punch", students.totalMissingPunch.ToString());

                return Json(new { data = students }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("ViewMonthAttendaceReport");
            }
        }
        //public ActionResult LoadAD()
        //{
        //    return Json(new { result = Attendacedetails }, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult Getinoutpunch(string cdate)
        {
            var context = new ATS2019_dbEntities();
            var Empcode = context.User_Master.Where(x => x.E_UserName == User.Identity.Name).Select(x => x.E_Code).FirstOrDefault();


            var client = new System.Net.Http.HttpClient();
            //client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getAllAttendancepunchs/?id=30&date=03/02/2020");
            client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getAllAttendancepunchs/");
            //client.BaseAddress = new Uri("http://192.168.1.24:8080/attendance/getAllAttendancepunchs/");
            // client.BaseAddress = new Uri("http://192.168.1.24:8080/attendance/getAllAttendancepunch/");
            var result = client.GetAsync("?id=" + Empcode.ToString() + "&date=" + cdate).Result;
            if (result.IsSuccessStatusCode)
            {


                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var jsondatedata = result.Content.ReadAsStringAsync().Result;

                return Json(jsondatedata, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("ViewMonthAttendaceReport");
            }
        }
        [HttpGet]
        public ActionResult CheckEmployeeAttendance()
        {
            using (var context = new ATS2019_dbEntities())
            {
                var totuser = context.User_Master.Where(m => m.E_Is_Active == 1 && m.E_Is_Delete == 0 && m.E_Location == "Vadodara").ToList().OrderBy(m => m.E_Code);
                List<SelectListItem> userlist = new List<SelectListItem>();
                foreach (var item in totuser)
                {
                    userlist.Add(new SelectListItem
                    {
                        Text = item.E_Code.ToString(),
                        Value = item.E_Fullname.ToString()

                    });
                }
                ViewBag.userlist = userlist.ToList().OrderBy(c => c.Text);
                return View();
            }
        }
        public ActionResult EmployeeMonthAttendanceReport(string empcode, string ssdate, string eedate)
        {
            string code = "";
            if ((empcode == "") || (empcode == null))
            {
                var context = new ATS2019_dbEntities();
                var Empcode = context.User_Master.Where(x => x.E_UserName == User.Identity.Name).Select(x => x.E_Code).FirstOrDefault();
                code = Empcode.ToString();
            }
            else
            {
                code = empcode.ToString();
            }
            AttendanceData students = null;
            var client = new System.Net.Http.HttpClient();
            //var result="";
            if ((ssdate == null) && (eedate == null))
            {
                client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getAttendance/");
                var result = client.GetAsync("?id=" + code.ToString()).Result;
                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;
                    students = JsonConvert.DeserializeObject<AttendanceData>(jsondata);
                    return Json(new { data = students }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return RedirectToAction("CheckEmployeeAttendance");
                }
            }
            else
            {
                client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getDateAttendance");
                //client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getDateAttendance");
                CultureInfo provider = CultureInfo.InvariantCulture;
                provider = new CultureInfo("en-Us", true);
                string format = "yyyy-MM-dd";
                DateTime sdate = DateTime.ParseExact(ssdate, format, provider);
                DateTime edate = DateTime.ParseExact(eedate, format, provider);

                //DateTime birthday = DateTime.ParseExact(ssdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //DateTime birthday2 = DateTime.ParseExact(eedate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string[] dd1 = sdate.ToString().Split('-');
                string[] dd2 = edate.ToString().Split('-');
                string sddate = dd1[0] + "/" + dd1[1] + "/" + sdate.Year.ToString();  //sdate.ToShortDateString().Replace('-', '/');
                string eddate = dd2[0] + "/" + dd2[1] + "/" + edate.Year.ToString(); //edate.ToShortDateString().Replace('-', '/');

                //string apistring = "?startdate=" + sddate + "&enddate=" + eddate + "&id=" + code;
                string apistring = "?startdate=" + sddate + "&enddate=" + eddate;

                var result = client.GetAsync(apistring + "&id=" + code).Result;
                // var result = client.GetAsync("?startdate=" + ssdate + "&enddate=" + eedate + "&id=" + code.ToString()).Result;

                // ViewBag.errormsg = errorresult.ToString();

                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;
                    students = JsonConvert.DeserializeObject<AttendanceData>(jsondata);
                    return Json(new { data = students }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { apistringvalue = apistring }, JsonRequestBehavior.AllowGet);
                }

            }

            //client.BaseAddress = new Uri("http://192.168.1.24:8080/attendance/getAttendance/");
            //http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getDateAttendance?startdate=10/02/2020&enddate=13/02/2020&id=30




            //if (result.IsSuccessStatusCode)
            //{
            //    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //    var jsondata = result.Content.ReadAsStringAsync().Result;
            //    students = JsonConvert.DeserializeObject<AttendanceData>(jsondata);
            //    return Json(new { data = students }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    return RedirectToAction("CheckEmployeeAttendance");
            //}
        }
        public ActionResult GetEmployeeinoutpunch(string empcode, string cdate)
        {
            string code = "";
            if ((empcode == "") || (empcode == null))
            {
                var context = new ATS2019_dbEntities();
                var Empcode = context.User_Master.Where(x => x.E_UserName == User.Identity.Name).Select(x => x.E_Code).FirstOrDefault();
                code = Empcode.ToString();
            }
            else
            {
                code = empcode.ToString();
            }

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getAllAttendancepunch/");
            //client.BaseAddress = new Uri("http://144.48.250.235:8080/ArcheSoftHR-0.0.1-SNAPSHOT/attendance/getAttendance/");
            //client.BaseAddress = new Uri("http://192.168.1.24:8080/attendance/getAllAttendancepunchs/");
            //client.BaseAddress = new Uri("http://192.168.1.24:8080/attendance/getAllAttendancepunch/");
            var result = client.GetAsync("?id=" + code + "&date=" + cdate).Result;
            if (result.IsSuccessStatusCode)
            {


                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var jsondatedata = result.Content.ReadAsStringAsync().Result;

                return Json(jsondatedata, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("ViewMonthAttendaceReport");
            }

        }
        public JsonResult getcurrentUserId()
        {
            using (var context = new ATS2019_dbEntities())
            {
                string val = (from c in context.User_Master where c.E_UserName == User.Identity.Name select c.E_Code).FirstOrDefault().ToString();
                return Json(val, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        public ActionResult AttendanceRegularazation()
        {
            using (var context = new ATS2019_dbEntities())
            {
                return View(context.User_Master.Where(c => c.E_UserName == User.Identity.Name).FirstOrDefault());
            }
        }

        public ActionResult ViewAllARDetails()
        {

            var context = new ATS2019_dbEntities();
            List<ARViewModel> aralldata = new List<ARViewModel>();
            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri("http://192.168.1.24:8080/ar/");
            
            
            var result = client.GetAsync("getallardetail").Result;
            if (result.IsSuccessStatusCode)
            {
                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var jsondata = result.Content.ReadAsStringAsync().Result;


                Object arvm = JsonConvert.DeserializeObject(jsondata.ToString());
                string input = jsondata;
                string[] resulttt = JSONDecoders.DecodeJSONArray(input);

                for (int i = 0; i < resulttt.Length; i++)
                {
                    string[] res1 = JSONDecoders.DecodeJSONArray(resulttt[i]);
                    string arstatus = "";
                    int empid = Convert.ToInt32(res1[1].ToString());
                    string empname = context.User_Master.Where(c => c.E_Code == empid).Select(c => c.E_Fullname).First().ToString();
                    int arst = Convert.ToInt32(res1[4]);
                    if (arst == 1)
                    {
                        arstatus = "Create";
                    }
                    else if (arst == 2)
                    {
                        arstatus = "Withdraw";
                    }
                    else if (arst == 3)
                    {
                        arstatus = "Approved";
                    }
                    else if (arst == 4)
                    {
                        arstatus = "Reject";
                    }
                    else if (arst == 5)
                    {
                        arstatus = "Revoke";
                    }
                    else if (arst == 6)
                    {
                        arstatus = "Partial Approval";
                    }
                    if (res1[2].ToString() != "")
                    {
                        string artype = "";
                        if (Convert.ToInt32(res1[3]) == 1)
                        {
                            artype = "AR";
                        }
                        else
                        {
                            artype = "MissPunch";
                        }
                        aralldata.Add(new ARViewModel
                        {
                            ARID = Convert.ToInt32(res1[0]),
                            EmpCode = empid,
                            EmpName = empname,
                            EmpDate = Convert.ToDateTime(res1[2]),
                            Type = artype,

                            Status = arstatus

                        }); ;
                    }

                }
                return View(aralldata.OrderByDescending(c => c.ARID));
            }
            else
            {
                return View();
            }


        }
        public ActionResult LoadAllArData()
        {
            var context = new ATS2019_dbEntities();
            List<ARViewModel> aralldata = new List<ARViewModel>();
            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri("http://192.168.1.24:8080/ar/");
            
            if (User.IsInRole("HR") || (User.IsInRole("Admin")))
            {
                
                var result = client.GetAsync("getallardetail").Result;
                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;


                    Object arvm = JsonConvert.DeserializeObject(jsondata.ToString());
                    string input = jsondata;
                    string[] resulttt = JSONDecoders.DecodeJSONArray(input);

                    for (int i = 0; i < resulttt.Length; i++)
                    {
                        string[] res1 = JSONDecoders.DecodeJSONArray(resulttt[i]);
                        string arstatus = "";
                        int empid = Convert.ToInt32(res1[1].ToString());
                        string empname = context.User_Master.Where(c => c.E_Code == empid).Select(c => c.E_Fullname).First().ToString();
                        int arst = Convert.ToInt32(res1[4]);
                        if (arst == 1)
                        {
                            arstatus = "Create";
                        }
                        else if (arst == 2)
                        {
                            arstatus = "Withdraw";
                        }
                        else if (arst == 3)
                        {
                            arstatus = "Approved";
                        }
                        else if (arst == 4)
                        {
                            arstatus = "Reject";
                        }
                        else if (arst == 5)
                        {
                            arstatus = "Revoke";
                        }
                        else if (arst == 6)
                        {
                            arstatus = "Partial Approval";
                        }


                        string artype = "";
                        if (Convert.ToInt32(res1[3]) == 1)
                        {
                            artype = "AR";
                        }
                        else
                        {
                            artype = "MissPunch";
                        }
                        aralldata.Add(new ARViewModel
                        {
                            ARID = Convert.ToInt32(res1[0]),
                            EmpCode = empid,
                            EmpName = empname,
                            EmpDate = Convert.ToDateTime(res1[2]),
                            Type = artype,

                            Status = arstatus

                        });

                    }
                    return Json(new { data = aralldata }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var empidd = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Code).First();
                var result = client.GetAsync("getardetail?id=" + empidd).Result;
                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;


                    Object arvm = JsonConvert.DeserializeObject(jsondata.ToString());
                    string input = jsondata;
                    string[] resulttt = JSONDecoders.DecodeJSONArray(input);

                    for (int i = 0; i < resulttt.Length; i++)
                    {
                        string[] res1 = JSONDecoders.DecodeJSONArray(resulttt[i]);
                        string arstatus = "";
                        int empid = Convert.ToInt32(res1[1].ToString());
                        string empname = context.User_Master.Where(c => c.E_Code == empid).Select(c => c.E_Fullname).First().ToString();
                        int arst = Convert.ToInt32(res1[4]);
                        if (arst == 1)
                        {
                            arstatus = "Create";
                        }
                        else if (arst == 2)
                        {
                            arstatus = "Withdraw";
                        }
                        else if (arst == 3)
                        {
                            arstatus = "Approved";
                        }
                        else if (arst == 4)
                        {
                            arstatus = "Reject";
                        }
                        else if (arst == 5)
                        {
                            arstatus = "Revoke";
                        }
                        else if (arst == 6)
                        {
                            arstatus = "Partial Approval";
                        }

                        string artype = "";
                        if (Convert.ToInt32(res1[3]) == 1)
                        {
                            artype = "AR";
                        }
                        else
                        {
                            artype = "MissPunch";
                        }
                        aralldata.Add(new ARViewModel
                        {
                            ARID = Convert.ToInt32(res1[0]),
                            EmpCode = empid,
                            EmpName = empname,
                            EmpDate = Convert.ToDateTime(res1[2]),
                            Type = artype,

                            Status = arstatus

                        });

                    }
                    return Json(new { data = aralldata }, JsonRequestBehavior.AllowGet);
                }
            }
            
                return View();
            
            
            
        }
        public ActionResult AttendanceRegularazationView(int? arid)
        {
            if (arid != null)
            {
                var context = new ATS2019_dbEntities();
                ARViewModel arvm = new ARViewModel();
                ARView aralldata = null;
                var client = new System.Net.Http.HttpClient();
                client.BaseAddress = new Uri("http://192.168.1.24:8080/ar/getardetailbyid");
                var result = client.GetAsync("?id=" + arid).Result;
                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;
                    aralldata = JsonConvert.DeserializeObject<ARView>(jsondata);
                    arvm.ARID = aralldata.mpd_id;
                    arvm.EmpCode = aralldata.employeeid;
                    var empdetails = context.User_Master.Where(c => c.E_Code == arvm.EmpCode).FirstOrDefault();
                    arvm.EmpName = empdetails.E_Fullname;
                    arvm.EmpDesignation = empdetails.E_Designation;
                    arvm.EmpDepartment = empdetails.E_Department;
                    arvm.Status = aralldata.status.ToString();
                    arvm.Type = aralldata.artype.ToString();
                    arvm.EmpDate = Convert.ToDateTime(aralldata.starttime);
                    arvm.Enddate = Convert.ToDateTime(aralldata.endtime);
                    arvm.EmpComment = aralldata.employeecomment;
                    arvm.ManagerComment = aralldata.managercomment;
                }

                return View(arvm);
            }
            else
            {
                return RedirectToAction("ViewAllARDetails");
            }
        }
        public ActionResult Overtime()
        {
            using (var context = new ATS2019_dbEntities())
            {
                return View(context.User_Master.Where(c => c.E_UserName == User.Identity.Name).FirstOrDefault());
            }
        }
        public ActionResult ViewOverTime()
        {
            return View();
        }
        public ActionResult LoadAllOTDetails()
        {
            var context = new ATS2019_dbEntities();
            List<ARViewModel> aralldata = new List<ARViewModel>();
            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri("http://192.168.1.24:8080/ot/");


            if (User.IsInRole("HR") || (User.IsInRole("Admin")))
            {

                var result = client.GetAsync("getallotdetail").Result;
                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;


                    Object arvm = JsonConvert.DeserializeObject(jsondata.ToString());
                    string input = jsondata;
                    string[] resulttt = JSONDecoders.DecodeJSONArray(input);

                    for (int i = 0; i < resulttt.Length; i++)
                    {
                        string[] res1 = JSONDecoders.DecodeJSONArray(resulttt[i]);
                        string arstatus = "";
                        int empid = Convert.ToInt32(res1[1].ToString());
                        string empname = context.User_Master.Where(c => c.E_Code == empid).Select(c => c.E_Fullname).First().ToString();
                        int arst = Convert.ToInt32(res1[3]);
                        if (arst == 1)
                        {
                            arstatus = "Create";
                        }
                        else if (arst == 2)
                        {
                            arstatus = "Withdraw";
                        }
                        else if (arst == 3)
                        {
                            arstatus = "Approved";
                        }
                        else if (arst == 4)
                        {
                            arstatus = "Reject";
                        }
                        else if (arst == 5)
                        {
                            arstatus = "Revoke";
                        }
                        else if (arst == 6)
                        {
                            arstatus = "Partial Approval";
                        }
                        aralldata.Add(new ARViewModel
                        {
                            ARID = Convert.ToInt32(res1[0]),
                            EmpCode = empid,
                            EmpName = empname,
                            EmpDate = Convert.ToDateTime(res1[2]),
                            Status = arstatus

                        });

                    }
                    return Json(new { data = aralldata }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var empidd = context.User_Master.Where(c => c.E_UserName == User.Identity.Name).Select(c => c.E_Code).First();
                var result = client.GetAsync("getotdetail?id=" + empidd).Result;
                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;


                    Object arvm = JsonConvert.DeserializeObject(jsondata.ToString());
                    string input = jsondata;
                    string[] resulttt = JSONDecoders.DecodeJSONArray(input);

                    for (int i = 0; i < resulttt.Length; i++)
                    {
                        string[] res1 = JSONDecoders.DecodeJSONArray(resulttt[i]);
                        string arstatus = "";
                        int empid = Convert.ToInt32(res1[1].ToString());
                        string empname = context.User_Master.Where(c => c.E_Code == empid).Select(c => c.E_Fullname).First().ToString();
                        int arst = Convert.ToInt32(res1[3]);
                        if (arst == 1)
                        {
                            arstatus = "Create";
                        }
                        else if (arst == 2)
                        {
                            arstatus = "Withdraw";
                        }
                        else if (arst == 3)
                        {
                            arstatus = "Approved";
                        }
                        else if (arst == 4)
                        {
                            arstatus = "Reject";
                        }
                        else if (arst == 5)
                        {
                            arstatus = "Revoke";
                        }
                        else if (arst == 6)
                        {
                            arstatus = "Partial Approval";
                        }
                        aralldata.Add(new ARViewModel
                        {
                            ARID = Convert.ToInt32(res1[0]),
                            EmpCode = empid,
                            EmpName = empname,
                            EmpDate = Convert.ToDateTime(res1[2]),
                            Status = arstatus

                        });

                    }
                    return Json(new { data = aralldata }, JsonRequestBehavior.AllowGet);
                }
            }

            return View();
        }

        public ActionResult ViewOTAppoval(int? arid)
        {
            if (arid != null)
            {
                var context = new ATS2019_dbEntities();
                ARViewModel arvm = new ARViewModel();
                OTView aralldata = null;
                var client = new System.Net.Http.HttpClient();
                client.BaseAddress = new Uri("http://192.168.1.24:8080/ot/getotdetailbyid");
                var result = client.GetAsync("?id=" + arid).Result;
                if (result.IsSuccessStatusCode)
                {
                    var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondata = result.Content.ReadAsStringAsync().Result;
                    aralldata = JsonConvert.DeserializeObject<OTView>(jsondata);
                    arvm.ARID = aralldata.ot_id;
                    arvm.EmpCode = aralldata.employeeid;
                    var empdetails = context.User_Master.Where(c => c.E_Code == arvm.EmpCode).FirstOrDefault();
                    arvm.EmpName = empdetails.E_Fullname;
                    arvm.EmpDesignation = empdetails.E_Designation;
                    arvm.EmpDepartment = empdetails.E_Department;
                    arvm.Status = aralldata.status.ToString();
                    arvm.EmpDate = Convert.ToDateTime(aralldata.starttime);
                    arvm.Enddate = Convert.ToDateTime(aralldata.endtime);
                    arvm.EmpComment = aralldata.employeecomment;
                    arvm.ManagerComment = aralldata.managercomment;
                }

                return View(arvm);
            }
            else
            {
                return RedirectToAction("ViewOverTime");
            }
        }
    }
}