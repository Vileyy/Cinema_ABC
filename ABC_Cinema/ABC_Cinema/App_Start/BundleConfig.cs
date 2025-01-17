﻿using System.Web;
using System.Web.Optimization;

namespace ABC_Cinema
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/Scripts/js").Include(
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/jquery-3.4.1.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/Login/css").Include("~/Content/loginform.css"));
            bundles.Add(new StyleBundle("~/Content/Dashboard/css").Include("~/Content/dashboard.css"));
            bundles.Add(new StyleBundle("~/Content/User/css").Include("~/Content/style_user.css"));
        }
    }
}
