using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BeYourMarket.Service;
using System.Threading.Tasks;
using BeYourMarket.Model.Models;
using Repository.Pattern.UnitOfWork;
using Newtonsoft.Json;
using BeYourMarket.Web.Extensions;
using BeYourMarket.Web.Models.Grids;
using BeYourMarket.Web.Models;
using BeYourMarket.Web.Utilities;
using ImageProcessor.Imaging.Formats;
using System.Drawing;
using ImageProcessor;
using System.IO;
using System.Text;
using BeYourMarket.Model.Enum;
using RestSharp;
using BeYourMarket.Web.Areas.Admin.Models;
using Postal;
using System.Net.Mail;
using System.Net;
using BeYourMarket.Service.Models;
using BeYourMarket.Core.Plugins;
using BeYourMarket.Core.Controllers;
using BeYourMarket.Core;
using Microsoft.Practices.Unity;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PaymentController : Controller
    {
        #region Fields
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        private readonly ISettingService _settingService;
        private readonly ISettingDictionaryService _settingDictionaryService;

        private readonly ICategoryService _categoryService;
        private readonly IListingService _listingService;

        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;

        private readonly IContentPageService _contentPageService;

        private readonly IOrderService _orderService;

        private readonly IEmailTemplateService _emailTemplateService;

        private readonly DataCacheService _dataCacheService;
        private readonly SqlDbService _sqlDbService;

        private readonly IPluginFinder _pluginFinder;

        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        #endregion

        #region Properties
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

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        #endregion

        #region Constructor
        public PaymentController(
            IUnitOfWorkAsync unitOfWorkAsync,
            ISettingService settingService,
            ICategoryService categoryService,
            IListingService listingService,
            ICustomFieldService customFieldService,
            ICustomFieldCategoryService customFieldCategoryService,
            IContentPageService contentPageService,
            IOrderService orderService,
            ISettingDictionaryService settingDictionaryService,
            IEmailTemplateService emailTemplateService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService,
            IPluginFinder pluginFinder)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _listingService = listingService;
            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;

            _orderService = orderService;

            _emailTemplateService = emailTemplateService;
            _contentPageService = contentPageService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;

            _pluginFinder = pluginFinder;
        }
        #endregion

        #region Methods
        public async Task<ActionResult> Order()
        {
            var userId = User.Identity.GetUserId();

            var orders = await _orderService.Query(x => x.Status != (int)Enum_OrderStatus.Created)
                .Include(x => x.Listing).Include(x => x.AspNetUserProvider).Include(x => x.AspNetUserReceiver).SelectAsync();

            var grid = new OrdersGrid(orders.AsQueryable().OrderByDescending(x => x.Created));

            var model = new OrderModel()
            {
                Grid = grid
            };

            return View(model);
        }

        public async Task<ActionResult> Transaction()
        {
            return View();
        }

        public async Task<ActionResult> PaymentSetting()
        {
            // Get payment info
            var model = new PaymentSettingModel()
            {
                Setting = CacheHelper.Settings,
                PaymentPlugins = _pluginFinder.GetPluginDescriptors(LoadPluginsMode.InstalledOnly, "Payment").ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PaymentSettingUpdate(PaymentSettingModel model)
        {
            var setting = _settingService.Queryable().FirstOrDefault();

            setting.TransactionFeePercent = model.Setting.TransactionFeePercent;
            setting.TransactionMinimumFee = model.Setting.TransactionMinimumFee;
            setting.TransactionMinimumSize = model.Setting.TransactionMinimumSize;
            setting.LastUpdated = DateTime.Now;

            setting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

            _settingService.Update(setting);

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.RemoveCachedItem(CacheKeys.SettingDictionary);
            _dataCacheService.RemoveCachedItem(CacheKeys.Settings);

            return RedirectToAction("PaymentSetting");
        }

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
        #endregion
    }
}