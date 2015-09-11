using System.Web;
using System.Web.Optimization;

namespace BeYourMarket.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {            
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
                .Include("~/Scripts/jquery.validate.js")
                .Include("~/Scripts/jquery.validate.unobtrusive.js")
                .Include("~/Scripts/jquery.validation.bootstrap.js")
                .Include("~/Scripts/jquery.validation.init.js"));            

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/app.js"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                      "~/js/sweet-alert/sweetalert.min.js",
                      "~/js/star-rating/star-rating.js",
                      "~/js/ripples/js/ripples.min.js",
                      "~/js/ripples/js/ripples.init.js"                      
                      ));

            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/bootstrap.css",                      
                      "~/js/sweet-alert/sweetalert.css",
                      "~/js/star-rating/star-rating.css",
                      "~/css/animate.css",
                      "~/js/ripples/css/ripples.min.css")
                .Include("~/css/material-design-icons.min.css", new CssRewriteUrlTransformWrapper())
                .Include("~/css/font-awesome/css/font-awesome.css", new CssRewriteUrlTransformWrapper()));
        }
    }
    
    //http://stackoverflow.com/questions/19619086/cssrewriteurltransform-is-not-being-called
    public class CssRewriteUrlTransformWrapper : IItemTransform
    {
        public string Process(string includedVirtualPath, string input)
        {
            return new CssRewriteUrlTransform().Process("~" + VirtualPathUtility.ToAbsolute(includedVirtualPath), input);
        }
    }
}
