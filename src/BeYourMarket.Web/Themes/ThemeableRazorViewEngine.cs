using BeYourMarket.Service;
using BeYourMarket.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BeYourMarket.Web.Themes
{
    /// <summary>
    /// ThemeableRazorViewEngine (inspired by NopCommerce)
    /// </summary>
    public class ThemeableRazorViewEngine : RazorViewEngine
    {
        internal Func<string, string> GetExtensionThunk;

        public ThemeableRazorViewEngine()
        {
            GetExtensionThunk = new Func<string, string>(VirtualPathUtility.GetExtension);

            AreaViewLocationFormats = new[]
             {
                 //themes
                 "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
                 "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",
                                              
                 //default
                 "~/Areas/{2}/Views/{1}/{0}.cshtml",
                 "~/Areas/{2}/Views/Shared/{0}.cshtml",
             };

            AreaMasterLocationFormats = new[]
             {
                 //themes
                 "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
                 "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",


                 //default
                 "~/Areas/{2}/Views/{1}/{0}.cshtml",
                 "~/Areas/{2}/Views/Shared/{0}.cshtml",
             };

            AreaPartialViewLocationFormats = new[]
             {
                 //themes
                 "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
                 "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",
                                                    
                 //default
                 "~/Areas/{2}/Views/{1}/{0}.cshtml",
                 "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };

            ViewLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Emails/{0}.cshtml",
            };

            MasterLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                "~/Themes/{2}/Views/Shared/{0}.cshtml", 

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Emails/{0}.cshtml",
            };

            PartialViewLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Emails/{0}.cshtml",
            };

            FileExtensions = new[] { "cshtml" };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            string layoutPath = null;
            var runViewStartPages = false;
            IEnumerable<string> fileExtensions = base.FileExtensions;
            return new RazorView(controllerContext, partialPath, layoutPath, runViewStartPages, fileExtensions);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            string layoutPath = masterPath;
            var runViewStartPages = true;
            IEnumerable<string> fileExtensions = base.FileExtensions;
            return new RazorView(controllerContext, viewPath, layoutPath, runViewStartPages, fileExtensions);
        }

        protected virtual bool IsSpecificPath(string name)
        {
            char ch = name[0];
            if (ch != '~')
            {
                return (ch == '/');
            }
            return true;
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            string[] strViewPath;
            string[] strViewMasterPath;

            var viewPath = GetPath(controllerContext, viewName, "View", useCache, ViewLocationFormats, AreaMasterLocationFormats, out strViewPath);

            // Set default layout
            if (string.IsNullOrEmpty(masterName) && !controllerContext.IsChildAction)
                masterName = "_Layout";

            var viewMasterPath = GetPath(controllerContext, masterName, "Master", useCache, MasterLocationFormats, AreaMasterLocationFormats, out strViewMasterPath);

            if (!string.IsNullOrEmpty(viewPath))
            {
                var view = CreateView(controllerContext, viewPath, viewMasterPath);
                return new ViewEngineResult(view, this);
            }
            else
                return new ViewEngineResult(new string[] { viewName });
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            string[] strViewPath;
            var viewPath = GetPath(controllerContext, partialViewName, "Partial", useCache, PartialViewLocationFormats, AreaPartialViewLocationFormats, out strViewPath);

            if (!string.IsNullOrEmpty(viewPath))
            {
                var view = CreatePartialView(controllerContext, viewPath);
                return new ViewEngineResult(view, this);
            }
            else
                return new ViewEngineResult(new string[] { partialViewName });
        }

        private string GetPath(ControllerContext controllerContext, string viewName, string cacheKeyPrefix, bool useCache, string[] viewLocationFormats, string[] viewAreaLocationFormats, out string[] searchedLocations)
        {
            searchedLocations = null;

            if (string.IsNullOrEmpty(viewName))
                return string.Empty;

            string areaName = GetAreaName(controllerContext.RouteData);
            string controllerName = controllerContext.RouteData.GetRequiredString("controller");

            List<ViewLocation> viewLocations = GetViewLocations(viewLocationFormats, !string.IsNullOrEmpty(areaName) ? viewAreaLocationFormats : null);
            if (viewLocations.Count == 0)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Properties cannot be null or empty.", new object[] { cacheKeyPrefix }));
            }

            bool isSpecificPath = IsSpecificPath(viewName);
            string key = this.CreateCacheKey(cacheKeyPrefix, viewName, isSpecificPath ? string.Empty : controllerName, areaName, CacheHelper.Settings.Theme);

            // return if there is a cache
            if (useCache)
            {
                var cached = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, key);
                if (cached != null)
                {
                    return cached;
                }
            }

            // return the view
            if (isSpecificPath)
            {
                return GetPathFromSpecificName(controllerContext, viewName, key, ref searchedLocations);
            }

            return GetPathFromGeneralName(controllerContext, viewName, controllerName, areaName, key, viewLocations, ref searchedLocations);
        }

        private string GetPathFromGeneralName(ControllerContext controllerContext, string viewName, string controllerName, string areaName, string cacheKey, List<ViewLocation> viewLocations, ref string[] searchedLocations)
        {
            var virtualPath = string.Empty;
            foreach (var viewLocation in viewLocations)
            {
                virtualPath = viewLocation.Format(viewName, controllerName, areaName, CacheHelper.Settings.Theme);

                if (FileExists(controllerContext, virtualPath))
                {
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
                    return virtualPath;
                }
            }

            virtualPath = string.Empty;
            searchedLocations = new string[] { viewName };

            return virtualPath;
        }

        protected virtual string GetPathFromSpecificName(ControllerContext controllerContext, string viewName, string cacheKey, ref string[] searchedLocations)
        {
            string virtualPath = viewName;
            if (!this.FilePathIsSupported(viewName) || !this.FileExists(controllerContext, viewName))
            {
                virtualPath = string.Empty;
                searchedLocations = new string[] { viewName };
            }

            ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
            return virtualPath;
        }

        protected virtual List<ViewLocation> GetViewLocations(string[] viewLocationFormats, string[] areaViewLocationFormats)
        {
            var list = new List<ViewLocation>();
            if (areaViewLocationFormats != null)
            {
                list.AddRange(areaViewLocationFormats.Select(str => new AreaAwareViewLocation(str)).Cast<ViewLocation>());
            }
            if (viewLocationFormats != null)
            {
                list.AddRange(viewLocationFormats.Select(str2 => new ViewLocation(str2)));
            }
            return list;
        }

        protected virtual string GetAreaName(RouteData routeData)
        {
            object obj2;
            if (routeData.DataTokens.TryGetValue("area", out obj2))
            {
                return (obj2 as string);
            }
            return GetAreaName(routeData.Route);
        }

        protected virtual string GetAreaName(RouteBase route)
        {
            var area = route as IRouteWithArea;
            if (area != null)
            {
                return area.Area;
            }
            var route2 = route as Route;
            if ((route2 != null) && (route2.DataTokens != null))
            {
                return (route2.DataTokens["area"] as string);
            }
            return null;
        }

        protected virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string theme)
        {
            return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}", new object[] { base.GetType().AssemblyQualifiedName, prefix, name, controllerName, areaName, theme });
        }

        protected virtual bool FilePathIsSupported(string virtualPath)
        {
            if (this.FileExtensions == null)
            {
                return true;
            }
            string str = this.GetExtensionThunk(virtualPath).TrimStart(new char[] { '.' });
            return this.FileExtensions.Contains<string>(str, StringComparer.OrdinalIgnoreCase);
        }
    }

    public class AreaAwareViewLocation : ViewLocation
    {
        public AreaAwareViewLocation(string virtualPathFormatString)
            : base(virtualPathFormatString)
        {
        }

        public override string Format(string viewName, string controllerName, string areaName, string theme)
        {
            return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, areaName, theme);
        }
    }

    public class ViewLocation
    {
        protected readonly string _virtualPathFormatString;

        public ViewLocation(string virtualPathFormatString)
        {
            _virtualPathFormatString = virtualPathFormatString;
        }

        public virtual string Format(string viewName, string controllerName, string areaName, string theme)
        {
            return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, theme);
        }
    }
}