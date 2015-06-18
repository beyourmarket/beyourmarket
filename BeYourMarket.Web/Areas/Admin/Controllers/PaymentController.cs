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
using Stripe;
using BeYourMarket.Web.Areas.Admin.Models;
using Postal;
using System.Net.Mail;
using System.Net;
using BeYourMarket.Service.Models;

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
        private readonly IItemService _itemService;

        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;

        private readonly IContentPageService _contentPageService;

        private readonly IOrderService _orderService;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IStripeConnectService _stripConnectService;

        private readonly IEmailTemplateService _emailTemplateService;

        private readonly DataCacheService _dataCacheService;
        private readonly SqlDbService _sqlDbService;

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
            IItemService itemService,
            ICustomFieldService customFieldService,
            ICustomFieldCategoryService customFieldCategoryService,
            IContentPageService contentPageService,
            IOrderService orderService,
            IOrderTransactionService orderTransationService,
            IStripeConnectService stripConnectService,
            ISettingDictionaryService settingDictionaryService,
            IEmailTemplateService emailTemplateService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _itemService = itemService;
            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;

            _orderService = orderService;
            _stripConnectService = stripConnectService;
            _orderTransactionService = orderTransationService;

            _emailTemplateService = emailTemplateService;
            _contentPageService = contentPageService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;
        }
        #endregion

        #region Methods
        public async Task<ActionResult> Order()
        {
            var userId = User.Identity.GetUserId();

            var orders = await _orderService.Query(x => x.Status != (int)Enum_OrderStatus.Created)
                .Include(x => x.Item).Include(x => x.AspNetUser).Include(x => x.AspNetUser1).SelectAsync();

            var grid = new OrdersGrid(orders.AsQueryable().OrderByDescending(x => x.Created));

            var model = new OrderModel()
            {
                Grid = grid
            };

            return View(model);
        }

        public async Task<ActionResult> Transaction()
        {
            var orders = await _orderService.Query(x => x.Status == (int)Enum_OrderStatus.Confirmed).SelectAsync();

            var transactionPayment = await _orderTransactionService.Query().SelectAsync();

            var transactionGridPayment = new TransactionGrid(transactionPayment.AsQueryable().OrderByDescending(x => x.Created));

            var model = new OrderTransactionModel()
            {
                TransactionPayment = transactionGridPayment
            };

            return View(model);
        }

        public async Task<ActionResult> PaymentSetting()
        {
            // Get payment info
            var model = new PaymentSettingModel()
            {
                Setting = CacheHelper.Settings,
                StripeClientID = CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeClientID).Value,
                StripeApiKey = CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeApiKey).Value,
                StripePublishableKey = CacheHelper.GetSettingDictionary(Enum_SettingKey.StripePublishableKey).Value
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

            var stripeApiKey = await _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, Enum_SettingKey.StripeApiKey);
            stripeApiKey.Value = model.StripeApiKey;
            _settingDictionaryService.SaveSettingDictionary(stripeApiKey);

            var stripeClientID = await _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, Enum_SettingKey.StripeClientID);
            stripeClientID.Value = model.StripeClientID;
            _settingDictionaryService.SaveSettingDictionary(stripeClientID);

            var stripePublishableKey = await _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, Enum_SettingKey.StripePublishableKey);
            stripePublishableKey.Value = model.StripePublishableKey;
            _settingDictionaryService.SaveSettingDictionary(stripePublishableKey);

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

            // Get the latest successful transaction
            var transactionQuery = await _orderTransactionService.Query(x => x.OrderID == id && string.IsNullOrEmpty(x.FailureCode)).SelectAsync();
            var transaction = transactionQuery.OrderByDescending(x => x.Created).FirstOrDefault();

            if (transaction == null)
            {
                var resultFailure = new { Success = "false", Message = "Transaction not found" };
                return Json(resultFailure, JsonRequestBehavior.AllowGet);
            }

            if (status == (int)Enum_OrderStatus.Cancelled)
            {
                // Update order
                order.Modified = DateTime.Now;
                order.Status = status;
                order.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
                _orderService.Update(order);

            }
            else if (status == (int)Enum_OrderStatus.Confirmed)
            {
                // Update order
                order.Modified = DateTime.Now;
                order.Status = status;
                order.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
                _orderService.Update(order);

                // Update Transaction
                transaction.IsCaptured = true;
                transaction.LastUpdated = DateTime.Now;
                _orderTransactionService.Update(transaction);

                // Capture payment
                var chargeService = new StripeChargeService(CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeApiKey).Value);
                StripeCharge stripeCharge = chargeService.Capture(transaction.ChargeID);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            var result = new { Success = "true", Message = "" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}