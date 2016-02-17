using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BeYourMarket.Web.Models;
using BeYourMarket.Model.Models;
using BeYourMarket.Web.Utilities;
using BeYourMarket.Service;
using Repository.Pattern.UnitOfWork;
using ImageProcessor.Imaging.Formats;
using System.Drawing;
using ImageProcessor;
using System.IO;
using System.Collections.Generic;
using BeYourMarket.Model.Enum;
using BeYourMarket.Web.Models.Grids;
using RestSharp;
using BeYourMarket.Core.Web;
using BeYourMarket.Core.Plugins;
using BeYourMarket.Core;
using Microsoft.Practices.Unity;
using BeYourMarket.Core.Controllers;
using BeYourMarket.Service.Models;
using BeYourMarket.Web.Extensions;
using i18n;

namespace BeYourMarket.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        #region Fields
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private readonly ISettingService _settingService;
        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly ICategoryService _categoryService;
        private readonly IListingService _listingService;
        private readonly IListingStatService _listingStatservice;
        private readonly IListingPictureService _listingPictureservice;
        private readonly IListingReviewService _listingReviewService;
        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;
        private readonly ICustomFieldListingService _customFieldListingService;

        private readonly DataCacheService _dataCacheService;
        private readonly SqlDbService _sqlDbService;

        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        private readonly IPluginFinder _pluginFinder;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        #endregion

        #region Constructors
        public PaymentController(
            IUnitOfWorkAsync unitOfWorkAsync,
            ISettingService settingService,
            ICategoryService categoryService,
            IListingService listingService,
            IPictureService pictureService,
            IListingPictureService ListingPictureservice,
            IOrderService orderService,
            ICustomFieldService customFieldService,
            ICustomFieldCategoryService customFieldCategoryService,
            ICustomFieldListingService customFieldListingService,
            ISettingDictionaryService settingDictionaryService,
            IListingStatService listingStatservice,
            IListingReviewService listingReviewService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService,
            IPluginFinder pluginFinder)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;

            _listingService = listingService;
            _pictureService = pictureService;
            _listingPictureservice = ListingPictureservice;
            _listingStatservice = listingStatservice;
            _listingReviewService = listingReviewService;

            _orderService = orderService;
            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldListingService = customFieldListingService;

            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;

            _pluginFinder = pluginFinder;

            _unitOfWorkAsync = unitOfWorkAsync;
        }
        #endregion

        #region Methods
        [HttpPost]
        public async Task<ActionResult> OrderAction(int id, int status)
        {
            var orderQuery = await _orderService.Query(x => x.ID == id).Include(x => x.Listing).SelectAsync();
            var order = orderQuery.FirstOrDefault();

            if (order == null)
                return new HttpNotFoundResult();

            var currentUserId = User.Identity.GetUserId();
            // Unauthorized access
            if (order.UserProvider != currentUserId && order.UserReceiver != currentUserId)
                return new HttpUnauthorizedResult();

            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IHookPlugin>(order.PaymentPlugin);
            if (descriptor == null)
                return new HttpNotFoundResult("Not found");

            var controllerType = descriptor.Instance<IHookPlugin>().GetControllerType();
            var controller = ContainerManager.GetConfiguredContainer().Resolve(controllerType) as IPaymentController;

            string message = string.Empty;
            var orderResult = controller.OrderAction(id, status, out message);

            var orderStatus = (Enum_OrderStatus)status;
            var orderStatusText = string.Empty;

            switch (orderStatus)
            {
                case Enum_OrderStatus.Created:
                case Enum_OrderStatus.Pending:
                    orderStatusText = "[[[Pending]]]";
                    break;
                case Enum_OrderStatus.Confirmed:
                    orderStatusText = "[[[Confirmed]]]";
                    break;
                case Enum_OrderStatus.Cancelled:
                    orderStatusText = "[[[Cancelled]]]";
                    break;
                default:
                    orderStatusText = orderStatus.ToString();
                    break;
            }

            var result = new
            {
                Success = orderResult,
                Message = message
            };

            if (orderResult)
            {
                // Send message to the user
                var messageSend = new MessageSendModel()
                {
                    UserFrom = currentUserId,
                    UserTo = order.UserProvider == currentUserId ? order.UserReceiver : order.UserProvider,
                    ListingID = order.ListingID,
                    Subject = order.Listing.Title,
                    Body = HttpContext.ParseAndTranslate(string.Format(
                    "[[[Order %0 - %1 - Total Price %2 %3. <a href=\"%4\">See Details</a>|||{0}|||{1}|||{2}|||{3}|||{4}]]]",
                    HttpContext.ParseAndTranslate(orderStatusText),
                    HttpContext.ParseAndTranslate(order.Description),
                    order.Price,
                    order.Currency,
                    Url.Action("Orders")))
                };

                await MessageHelper.SendMessage(messageSend);
            }

            TempData[TempDataKeys.UserMessage] = string.Format("[[[The order is %0.|||{0}]]]", orderStatus);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Orders()
        {
            var userId = User.Identity.GetUserId();

            var orders = await _orderService
                .Query(x => (x.UserProvider == userId || x.UserReceiver == userId))
                .Include(x => x.Listing)
                .Include(x => x.AspNetUserProvider)
                .Include(x => x.AspNetUserReceiver)
                .Include(x => x.ListingReviews)
                .SelectAsync();

            var grid = new OrdersGrid(orders.AsQueryable().OrderByDescending(x => x.Created));

            var model = new OrderModel()
            {
                Grid = grid
            };

            return View(model);
        }

        public ActionResult PaymentSetting()
        {
            return View();
        }

        public async Task<ActionResult> Payment(int id)
        {
            var selectQuery = await _orderService.Query(x => x.ID == id)
                .Include(x => x.Listing)
                .Include(x => x.Listing.ListingType)
                .Include(x => x.Listing.ListingPictures)
                .SelectAsync();

            var order = selectQuery.FirstOrDefault();

            if (order == null)
                return new HttpNotFoundResult();

            var model = new PaymentModel()
            {
                ListingOrder = order
            };

            ClearCache();

            return View(model);
        }

        public async Task<ActionResult> Transaction()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Order(Order order)
        {
            var listing = await _listingService.FindAsync(order.ListingID);

            if (listing == null)
                return new HttpNotFoundResult();

            // Redirect if not authenticated
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { ReturnUrl = Url.Action("Listing", "Listing", new { id = order.ListingID }) });

            var userCurrent = User.Identity.User();

            // Check if payment method is setup on user or the platform
            var descriptors = _pluginFinder.GetPluginDescriptors<IHookPlugin>(LoadPluginsMode.InstalledOnly, "Payment").Where(x => x.Enabled);
            if (descriptors.Count() == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[The provider has not setup the payment option yet, please contact the provider.]]]";

                return RedirectToAction("Listing", "Listing", new { id = order.ListingID });
            }

            foreach (var descriptor in descriptors)
            {
                var controllerType = descriptor.Instance<IHookPlugin>().GetControllerType();
                var controller = ContainerManager.GetConfiguredContainer().Resolve(controllerType) as IPaymentController;

                if (!controller.HasPaymentMethod(listing.UserID))
                {
                    TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                    TempData[TempDataKeys.UserMessage] = string.Format("[[[The provider has not setup the payment option for {0} yet, please contact the provider.]]]", descriptor.FriendlyName);

                    return RedirectToAction("Listing", "Listing", new { id = order.ListingID });
                }
            }

            if (order.ID == 0)
            {
                order.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                order.Created = DateTime.Now;
                order.Modified = DateTime.Now;
                order.Status = (int)Enum_OrderStatus.Created;
                order.UserProvider = listing.UserID;
                order.UserReceiver = userCurrent.Id;
                order.ListingTypeID = order.ListingTypeID;
                order.Currency = listing.Currency;

                if (order.UserProvider == order.UserReceiver)
                {
                    TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                    TempData[TempDataKeys.UserMessage] = "[[[You cannot book the item from yourself!]]]";

                    return RedirectToAction("Listing", "Listing", new { id = order.ListingID });
                }

                if (order.ToDate.HasValue && order.FromDate.HasValue)
                {
                    order.Description = HttpContext.ParseAndTranslate(
                        string.Format("{0} #{1} ([[[From]]] {2} [[[To]]] {3})",
                        listing.Title,
                        listing.ID,
                        order.FromDate.Value.ToShortDateString(),
                        order.ToDate.Value.ToShortDateString()));

                    order.Quantity = order.ToDate.Value.Date.AddDays(1).Subtract(order.FromDate.Value.Date).Days;
                    order.Price = order.Quantity * listing.Price;
                }
                else if (order.Quantity.HasValue)
                {
                    order.Description = string.Format("{0} #{1}", listing.Title, listing.ID);
                    order.Quantity = order.Quantity.Value;
                    order.Price = order.Quantity * listing.Price;
                }
                else
                {
                    // Default
                    order.Description = string.Format("{0} #{1}", listing.Title, listing.ID);
                    order.Quantity = 1;
                    order.Price = listing.Price;
                }

                _orderService.Insert(order);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            ClearCache();

            return RedirectToAction("Payment", new { id = order.ID });
        }

        private void ClearCache()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
        #endregion
    }
}
