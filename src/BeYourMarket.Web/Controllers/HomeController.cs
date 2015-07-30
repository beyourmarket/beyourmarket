using BeYourMarket.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BeYourMarket.Web.Extensions;
using BeYourMarket.Web.Utilities;
using System.Threading.Tasks;
using BeYourMarket.Model.Models;
using BeYourMarket.Web.Models;
using PagedList;
using BeYourMarket.Web.Models.Grids;
using i18n;
using i18n.Helpers;

namespace BeYourMarket.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Fields
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IContentPageService _contentPageService;
        #endregion

        #region Constructor
        public HomeController(
            ICategoryService categoryService,
            IItemService itemService,
            IContentPageService contentPageService)
        {
            _categoryService = categoryService;
            _itemService = itemService;
            _contentPageService = contentPageService;

        }
        #endregion

        #region Methods
        public async Task<ActionResult> Index(string id)
        {
            if (!string.IsNullOrEmpty(id))
                return RedirectToAction("ContentPage", "Home", new { id = id.ToLowerInvariant() });

            var model = new SearchListingModel();
            await GetSearchResult(model);

            return View(model);
        }

        public async Task<ActionResult> ContentPage(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Index", "Home");

            var slug = id.ToLowerInvariant();
            var contentPageQuery = await _contentPageService.Query(x => x.Slug == slug && x.Published).SelectAsync();
            var contentPage = contentPageQuery.FirstOrDefault();

            if (contentPage == null)
            {
                return new HttpNotFoundResult();
            }

            return View(contentPage);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            var model = new ContactModel();

            if (User.Identity.IsAuthenticated)
            {
                model.Email = User.Identity.User().Email;
            }

            return View(model);
        }

        public async Task<ActionResult> Search(SearchListingModel model)
        {
            await GetSearchResult(model);

            return View("~/Views/Listing/Listings.cshtml", model);
        }

        private async Task GetSearchResult(SearchListingModel model)
        {
            IEnumerable<Item> items = null;

            // Category
            if (model.CategoryID != 0)
                items = await _itemService.Query(x => x.CategoryID == model.CategoryID)
                    .Include(x => x.ItemPictures)
                    .Include(x => x.Category)
                    .Include(x => x.ItemType)
                    .SelectAsync();

            // Search Text
            if (!string.IsNullOrEmpty(model.SearchText))
            {
                model.SearchText = model.SearchText.ToLower();

                if (items != null)
                    items = items.Where(x => x.Title.ToLower().Contains(model.SearchText));
                else
                    items = await _itemService.Query(x => x.Title.ToLower().Contains(model.SearchText)).Include(x => x.ItemPictures).Include(x => x.Category).SelectAsync();
            }

            // Latest
            if (items == null)
                items = await _itemService.Query().OrderBy(x => x.OrderByDescending(y => y.Created)).Include(x => x.ItemPictures).Include(x => x.Category).SelectAsync();

            // Location
            if (!string.IsNullOrEmpty(model.Location))
            {
                items = items.Where(x => !string.IsNullOrEmpty(x.Location) && x.Location.IndexOf(model.Location, StringComparison.OrdinalIgnoreCase) != -1);
            }

            // Picture
            if (model.PhotoOnly)
                items = items.Where(x => x.ItemPictures.Count > 0);

            /// Price
            if (model.PriceFrom.HasValue)
                items = items.Where(x => x.Price >= model.PriceFrom.Value);

            if (model.PriceTo.HasValue)
                items = items.Where(x => x.Price <= model.PriceTo.Value);

            // Show active and enabled only
            var itemsModelList = new List<ItemModel>();
            foreach (var item in items.Where(x => x.Active && x.Enabled).OrderByDescending(x => x.Created))
            {
                itemsModelList.Add(new ItemModel()
                {
                    ItemCurrent = item,
                    UrlPicture = item.ItemPictures.Count == 0 ? ImageHelper.GetItemImagePath(0) : ImageHelper.GetItemImagePath(item.ItemPictures.OrderBy(x => x.Ordering).FirstOrDefault().PictureID)
                });
            }
            var breadCrumb = GetParents(model.CategoryID).Reverse().ToList();

            model.BreadCrumb = breadCrumb;
            model.Categories = CacheHelper.Categories;
            model.Items = itemsModelList;
            model.ItemsPageList = itemsModelList.ToPagedList(model.PageNumber, model.PageSize);
            model.Grid = new ListingModelGrid(model.ItemsPageList.AsQueryable());
        }

        IEnumerable<Category> GetParents(int categoryId)
        {
            Category category = _categoryService.Find(categoryId);
            while (category != null && category.Parent != category.ID)
            {
                yield return category;
                category = _categoryService.Find(category.Parent);
            }
        }

        [ChildActionOnly]
        public ActionResult NavigationSide()
        {
            var rootId = 0;
            var categories = CacheHelper.Categories.ToList();

            var categoryTree = categories.OrderBy(x => x.Parent).ThenBy(x => x.Ordering).ToList().GenerateTree(x => x.ID, x => x.Parent, rootId);

            var contentPages = CacheHelper.ContentPages.Where(x => x.Published).OrderBy(x => x.Ordering);

            var model = new NavigationSideModel()
            {
                CategoryTree = categoryTree,
                ContentPages = contentPages
            };

            return View("_NavigationSide", model);
        }

        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            //var languages = i18n.LanguageHelpers.GetAppLanguages();
            var languages = LanguageHelper.AvailableLanguges.Languages;
            var languageCurrent = ControllerContext.RequestContext.HttpContext.GetPrincipalAppLanguageForRequest();

            var model = new LanguageSelectorModel();
            model.Culture = languageCurrent.GetLanguage();
            model.DisplayName = languageCurrent.GetCultureInfo().NativeName;
            
            foreach (var language in languages)
            {
                if (language.Culture != languageCurrent.GetLanguage() && language.Enabled)
                {                    
                    model.LanguageList.Add(new LanguageSelectorModel()
                    {
                        Culture = language.Culture,
                        DisplayName = language.LanguageTag.CultureInfo.NativeName
                    });
                }
            }

            return PartialView("_LanguageSelector", model);
        }

        [AllowAnonymous]
        public ActionResult SetLanguage(string langtag, string returnUrl)
        {
            // If valid 'langtag' passed.
            i18n.LanguageTag lt = i18n.LanguageTag.GetCachedInstance(langtag);
            if (lt.IsValid())
            {
                // Set persistent cookie in the client to remember the language choice.
                Response.Cookies.Add(new HttpCookie("i18n.langtag")
                {
                    Value = lt.ToString(),
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddYears(1)
                });
            }
            // Owise...delete any 'language' cookie in the client.
            else
            {
                var cookie = Response.Cookies["i18n.langtag"];
                if (cookie != null)
                {
                    cookie.Value = null;
                    cookie.Expires = DateTime.UtcNow.AddMonths(-1);
                }
            }
            // Update PAL setting so that new language is reflected in any URL patched in the 
            // response (Late URL Localization).
            HttpContext.SetPrincipalAppLanguageForRequest(lt);
            // Patch in the new langtag into any return URL.
            if (returnUrl.IsSet())
            {
                returnUrl = LocalizedApplication.Current.UrlLocalizerForApp.SetLangTagInUrlPath(HttpContext, returnUrl, UriKind.RelativeOrAbsolute, lt == null ? null : lt.ToString()).ToString();
            }
            //Redirect user agent as approp.
            return this.Redirect(returnUrl);
        }
        #endregion
    }
}