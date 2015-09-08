using BeYourMarket.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BeYourMarket.Web.Extensions
{
    public class TreeItem<T>
    {
        public T Item { get; set; }
        public IEnumerable<TreeItem<T>> Children { get; set; }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Generates tree of items from item list
        /// </summary>
        /// 
        /// <typeparam name="T">Type of item in collection</typeparam>
        /// <typeparam name="K">Type of parent_id</typeparam>
        /// 
        /// <param name="collection">Collection of items</param>
        /// <param name="id_selector">Function extracting item's id</param>
        /// <param name="parent_id_selector">Function extracting item's parent_id</param>
        /// <param name="root_id">Root element id</param>
        /// 
        /// <returns>Tree of items</returns>
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(
            this IEnumerable<T> collection,
            Func<T, K> id_selector,
            Func<T, K> parent_id_selector,
            K root_id = default(K))
        {
            foreach (var c in collection.Where(c => parent_id_selector(c).Equals(root_id)))
            {
                yield return new TreeItem<T>
                {
                    Item = c,
                    Children = collection.GenerateTree(id_selector, parent_id_selector, id_selector(c))
                };
            }
        }

        public static SelectList ToSelectList<TEnum>(this TEnum enumObj)
           where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         select new { Id = e, Name = e.ToString() };
            return new SelectList(values, "ID", "Name", enumObj);
        }

        public static ApplicationUser User(this IIdentity identity)
        {
            var userId = identity.GetUserId();
            return userId.User();
        }

        public static ApplicationUser User(this string userId)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var user = userManager.FindById(userId);
            return user;
        }

        public static string UrlReplace(this HttpRequestBase request, string key, string value)
        {
            NameValueCollection nv = new NameValueCollection(request.QueryString);
            nv.Set(key, value);

            UriBuilder u = new UriBuilder(request.Url);
            u.Query = ToQueryString(nv);

            return u.Uri.ToString();
        }

        public static string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return string.Join("&", array);
        }

        public static string PrefixString(this string text, int length = 100)
        {
            return text == null ? string.Empty : text.Substring(0, Math.Min(length, text.Length)) + "...";
        }

        /// <summary>
        /// Transform translation text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string TransformTranslationParameter(this string text)
        {
            return text == null ? string.Empty : text.Replace("[[[", "(((").Replace("]]]", ")))");
        }

        //http://stackoverflow.com/questions/3643613/asp-net-mvc-highlighting-current-page-link-technique
        public static MvcHtmlString MenuItem(
            this HtmlHelper htmlHelper,
            string action,
            string controller,
            string text
        )
        {
            var menu = new TagBuilder("a");

            var response = new HttpResponse(new System.IO.StringWriter());
            var httpContext = new HttpContext(HttpContext.Current.Request, response);
            var routeData = System.Web.Routing.RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            var routevalues = routeData.Values;

            //var currentAction = (string)htmlHelper.ViewContext.RouteData.Values["action"];
            var currentAction = (string)routevalues["action"];
            var currentController = (string)routevalues["controller"];

            if (string.Equals(currentAction, action, StringComparison.CurrentCultureIgnoreCase) && string.Equals(currentController, controller, StringComparison.CurrentCultureIgnoreCase))
            {
                menu.AddCssClass("active");
            }

            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            menu.Attributes.Add("href", url.Action(action, controller));
            menu.SetInnerText(text);

            return MvcHtmlString.Create(menu.ToString());
        }

        public static bool IsActiveMenuItem(this HtmlHelper htmlHelper, string action, string controller, string id = "")
        {
            var response = new HttpResponse(new System.IO.StringWriter());
            var httpContext = new HttpContext(HttpContext.Current.Request, response);
            var routeData = System.Web.Routing.RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            var routevalues = routeData.Values;

            var currentAction = (string)routevalues["action"];
            var currentController = (string)routevalues["controller"];

            bool active = string.Equals(currentAction, action, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(currentController, controller, StringComparison.CurrentCultureIgnoreCase);

            // Check if there is an id
            if (!string.IsNullOrEmpty(id))
            {
                var currentId = routevalues["id"].ToString();

                if (string.IsNullOrEmpty(currentId) || string.IsNullOrEmpty(id))
                    return false;

                return string.Equals(currentId, id, StringComparison.CurrentCultureIgnoreCase) && active;
            }

            return active;
        }
    }
}
