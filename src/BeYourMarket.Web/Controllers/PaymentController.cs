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
        private readonly IItemService _itemService;
        private readonly IItemStatService _itemStatService;
        private readonly IItemPictureService _itemPictureService;
        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;
        private readonly ICustomFieldItemService _customFieldItemService;

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
            IItemService itemService,
            IPictureService pictureService,
            IItemPictureService itemPictureService,
            IOrderService orderService,
            ICustomFieldService customFieldService,
            ICustomFieldCategoryService customFieldCategoryService,
            ICustomFieldItemService customFieldItemService,
            ISettingDictionaryService settingDictionaryService,
            IItemStatService itemStatService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService,
            IPluginFinder pluginFinder)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _itemService = itemService;
            _pictureService = pictureService;
            _itemPictureService = itemPictureService;
            _orderService = orderService;
            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldItemService = customFieldItemService;
            _itemStatService = itemStatService;

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
            var order = await _orderService.FindAsync(id);

            if (order == null)
                return new HttpNotFoundResult();

            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IHookPlugin>(order.PaymentPlugin);
            if (descriptor == null)
                return new HttpNotFoundResult("Not found");

            var controllerType = descriptor.Instance<IHookPlugin>().GetControllerType();
            var controller = ContainerManager.GetConfiguredContainer().Resolve(controllerType) as IPaymentController;

            string message = string.Empty;
            var orderResult = controller.OrderAction(id, status, out message);

            var result = new
            {
                Success = orderResult,
                Message = message
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Orders()
        {
            var userId = User.Identity.GetUserId();

            var orders = await _orderService.Query(x => x.Status != (int)Enum_OrderStatus.Created && (x.UserProvider == userId || x.UserReceiver == userId))
                .Include(x => x.Item).SelectAsync();

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
                .Include(x => x.Item)                
                .Include(x => x.Item.ItemType)
                .Include(x => x.Item.ItemPictures)
                .SelectAsync();

            var order = selectQuery.FirstOrDefault();

            if (order == null)
                return new HttpNotFoundResult();

            var model = new PaymentModel()
            {
                ItemOrder = order
            };

            ClearCache();

            return View(model);
        }

        public async Task<ActionResult> Transaction()
        {
            return View();
        }

        public async Task<ActionResult> Order(Order order)
        {
            var item = await _itemService.FindAsync(order.ItemID);

            if (item == null)
                return new HttpNotFoundResult();

            // Check if payment method is setup on user or the platform
            var descriptors = _pluginFinder.GetPluginDescriptors<IHookPlugin>(LoadPluginsMode.InstalledOnly, "Payment").Where(x => x.Enabled);
            if (descriptors.Count() == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[The provider has not setup the payment option yet, please contact the provider.]]]";

                return RedirectToAction("Listing", "Listing", new { id = order.ItemID });
            }

            foreach (var descriptor in descriptors)
            {
                var controllerType = descriptor.Instance<IHookPlugin>().GetControllerType();
                var controller = ContainerManager.GetConfiguredContainer().Resolve(controllerType) as IPaymentController;

                if (!controller.HasPaymentMethod(item.UserID))
                {
                    TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                    TempData[TempDataKeys.UserMessage] = string.Format("[[[The provider has not setup the payment option for {0} yet, please contact the provider.]]]", descriptor.FriendlyName);

                    return RedirectToAction("Listing", "Listing", new { id = order.ItemID });
                }
            }

            if (order.ID == 0)
            {
                order.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                order.Created = DateTime.Now;
                order.Modified = DateTime.Now;
                order.Status = (int)Enum_OrderStatus.Created;
                order.UserProvider = item.UserID;
                order.UserReceiver = User.Identity.GetUserId();
                order.ItemTypeID = order.ItemTypeID;

                if (order.UserProvider == order.UserReceiver)
                {
                    TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                    TempData[TempDataKeys.UserMessage] = "[[[You cannot book the item from yourself!]]]";

                    return RedirectToAction("Listing", "Listing", new { id = order.ItemID });
                }

                if (order.ToDate.HasValue && order.FromDate.HasValue)
                {
                    order.Description = string.Format("{0} #{1} ([[[From]]] {2} [[[To]]] {3})",
                        item.Title, item.ID,
                        order.FromDate.Value.ToShortDateString(), order.ToDate.Value.ToShortDateString());

                    order.Quantity = order.ToDate.Value.Date.AddDays(1).Subtract(order.FromDate.Value.Date).Days;
                    order.Price = order.Quantity * item.Price;
                }
                else if (order.Quantity.HasValue)
                {
                    order.Description = string.Format("{0} #{1}", item.Title, item.ID);
                    order.Quantity = order.Quantity.Value;
                    order.Price = item.Price;
                }
                else
                {
                    // Default
                    order.Description = string.Format("{0} #{1}", item.Title, item.ID);
                    order.Quantity = 1;
                    order.Price = item.Price;
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