using System.Web.Optimization;
using MvcLib.Common;

namespace $rootnamespace$.App_Start
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/codemirror-js")
                 .Include("~/Scripts/CodeMirror/codemirror.js")
                 .Include("~/Scripts/CodeMirror/clike.js")
                 .Include("~/Scripts/CodeMirror/htmlmixed.js")
                 .Include("~/Scripts/CodeMirror/css.js")
                 .Include("~/Scripts/CodeMirror/javascript.js")
                 .Include("~/Scripts/CodeMirror/xml.js")
                 .Include("~/Scripts/CodeMirror/razor.js")
                 .Include("~/Scripts/CodeMirror/htmlembedded.js")
                 .Include("~/Scripts/CodeMirror/dialog.js")
                 .Include("~/Scripts/CodeMirror/searchcursor.js")
                 .Include("~/Scripts/CodeMirror/search.js")
                 .Include("~/Scripts/CodeMirror/foldcode.js")
                 .Include("~/Scripts/CodeMirror/indent-fold.js")
                 .Include("~/Scripts/CodeMirror/brace-fold.js")
                 .Include("~/Scripts/CodeMirror/xml-fold.js")
                 .Include("~/Scripts/CodeMirror/matchtags.js")
                 .Include("~/Scripts/CodeMirror/matchbrackets.js")
                 .Include("~/Scripts/CodeMirror/closebrackets.js")
                 .Include("~/Scripts/CodeMirror/closetag.js")
                 .Include("~/Scripts/CodeMirror/active-line.js")
                 .Include("~/Scripts/CodeMirror/fullscreen.js"));

            bundles.Add(new StyleBundle("~/bundles/codemirror-css")
                .Include("~/Content/codemirror/codemirror.css")
                .Include("~/Content/codemirror/dialog.css")
                .Include("~/Content/codemirror/fullscreen.css")
                .Include("~/Content/codemirror/xq-light.css")
                .Include("~/Content/codemirror/pastel-on-dark.css")
                .Include("~/Content/codemirror/mbo.css")
                .Include("~/Content/codemirror/monokai.css")
                .Include("~/Content/codemirror/eclipse.css")
                .Include("~/Content/codemirror/vibrant-ink.css"));


            BundleTable.EnableOptimizations = !Config.IsInDebugMode;
        }
    }
}
