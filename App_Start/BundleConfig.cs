using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace ATS2019_2.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/gridmvc.js",
                "~/Scripts/gridmvc.lang.ru.js",
                "~/Scripts/gridmvc.min.js",
                "~/Scripts/home.js",
                "~/Scripts/jquery-1.6.4-csdoc.js",
                "~/Scripts/jquery-1.6.4.js",
                "~/Scripts/jquery-1.6.4.min.js",
                "~/Scripts/jquery.signalR-2.4.0.js",
                "~/Scripts/jquery.signalR-2.4.0.min.js"
                ));
            



            bundles.Add(new StyleBundle("~/Content/mastercss").Include(
                    "~/Content/adduser.css",
                    "~/Content/dashboard.css",
                    "~/Content/dash2.css",
                    "~/Content/profile.css"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
              
                 "~/Content/Gridmvc.css",
                 "~/Content/login.css",
                 "~/Content/login_2.css",
                 "~/Content/login_home.css"
                
             ));
            BundleTable.EnableOptimizations = true;
        }
    }
}