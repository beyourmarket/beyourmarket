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
using Stripe;

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
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IStripeConnectService _stripConnectService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;
        private readonly ICustomFieldItemService _customFieldItemService;

        private readonly DataCacheService _dataCacheService;
        private readonly SqlDbService _sqlDbService;

        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

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
            IOrderTransactionService orderTransationService,
            IStripeConnectService stripConnectService,
            ICustomFieldService customFieldService,
            ICustomFieldCategoryService customFieldCategoryService,
            ICustomFieldItemService customFieldItemService,
            ISettingDictionaryService settingDictionaryService,
            IItemStatService itemStatService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _itemService = itemService;
            _pictureService = pictureService;
            _itemPictureService = itemPictureService;
            _orderService = orderService;
            _stripConnectService = stripConnectService;
            _orderTransactionService = orderTransationService;
            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldItemService = customFieldItemService;
            _itemStatService = itemStatService;

            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;

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

        public async Task<ActionResult> PaymentSetting(string scope, string code)
        {
            if (!string.IsNullOrEmpty(scope) && !string.IsNullOrEmpty(code))
            {
                var client = new RestClient("https://connect.stripe.com/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddParameter("client_secret", CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeApiKey).Value);
                request.AddParameter("code", code);
                request.AddParameter("grant_type", "authorization_code");

                var response = client.Execute<StripeConnect>(request);

                if (string.IsNullOrEmpty(response.Data.error))
                {
                    var userId = User.Identity.GetUserId();
                    var stripeConnectQuery = await _stripConnectService.Query(x => x.UserID == userId).SelectAsync();
                    var stripeConnect = stripeConnectQuery.FirstOrDefault();

                    // Delete old one and insert new one
                    if (stripeConnect != null)
                        _stripConnectService.Delete(stripeConnect);

                    response.Data.UserID = User.Identity.GetUserId();
                    response.Data.Created = DateTime.Now;
                    response.Data.LastUpdated = DateTime.Now;
                    response.Data.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;

                    _stripConnectService.Insert(response.Data);
                    await _unitOfWorkAsync.SaveChangesAsync();

                    TempData[TempDataKeys.UserMessage] = "Connnect to stripe successfully!";

                    return RedirectToAction("PaymentSetting");
                }
                else
                {
                    TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                    TempData[TempDataKeys.UserMessage] = response.Data.error_description;
                }
            }
            else
            {
                var userId = User.Identity.GetUserId();
                var stripeConnectQuery = await _stripConnectService.Query(x => x.UserID == userId).SelectAsync();
                var stripeConnect = stripeConnectQuery.FirstOrDefault();

                if (stripeConnect != null)
                    return View(Enum_StripeConnectStatus.Authorized);
            }

            return View(Enum_StripeConnectStatus.None);
        }

        public async Task<ActionResult> PaymentSettingDeauthorize()
        {
            var userId = User.Identity.GetUserId();
            var stripeConnectQuery = await _stripConnectService.Query(x => x.UserID == userId).SelectAsync();
            var stripeConnect = stripeConnectQuery.FirstOrDefault();

            // Delete old one and insert new one
            if (stripeConnect != null)
                _stripConnectService.Delete(stripeConnect);

            var client = new RestClient("https://connect.stripe.com/oauth/deauthorize");

            var request = new RestRequest(Method.POST);
            request.AddParameter("client_secret", CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeApiKey).Value);
            request.AddParameter("client_id", CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeClientID).Value);
            request.AddParameter("stripe_user_id", stripeConnect.stripe_user_id);

            var response = client.Execute(request);

            await _unitOfWorkAsync.SaveChangesAsync();

            TempData[TempDataKeys.UserMessage] = "Disconnnect to stripe successfully!";

            return RedirectToAction("PaymentSetting");
        }

        public async Task<ActionResult> Payment(int id)
        {
            var selectQuery = await _orderService.Query(x => x.ID == id).Include(x => x.Item).Include(x => x.Item.ItemPictures).SelectAsync();

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
            var userId = User.Identity.GetUserId();

            var orders = await _orderService.Query(x => x.Status == (int)Enum_OrderStatus.Confirmed && (x.UserProvider == userId || x.UserReceiver == userId)).SelectAsync();

            var orderIdPayment = orders.Where(x => x.UserProvider == userId).Select(x => x.ID);
            var orderIdPayout = orders.Where(x => x.UserReceiver == userId).Select(x => x.ID);

            var transactionPayment = await _orderTransactionService.Query(x => orderIdPayment.Contains(x.OrderID)).SelectAsync();
            var transactionPayout = await _orderTransactionService.Query(x => orderIdPayout.Contains(x.OrderID)).SelectAsync();

            var transactionGridPayment = new TransactionGrid(transactionPayment.AsQueryable().OrderByDescending(x => x.Created));
            var transactionGridPayout = new TransactionGrid(transactionPayout.AsQueryable().OrderByDescending(x => x.Created));

            var model = new OrderTransactionModel()
            {
                TransactionPayment = transactionGridPayment,
                TransactionPayout = transactionGridPayout
            };

            return View(model);
        }

        public async Task<ActionResult> Order(Order order)
        {
            var item = await _itemService.FindAsync(order.ItemID);

            if (item == null)
                return new HttpNotFoundResult();

            // Get payment method
            var stripeConnectQuery = await _stripConnectService.Query(x => x.UserID == item.UserID).SelectAsync();

            if (!stripeConnectQuery.Any())
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "The provider has not setup the payment option yet, please contact the provider.";

                return RedirectToAction("Listing", "Listing", new { id = order.ItemID });
            }

            if (order.ID == 0)
            {
                order.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                order.Created = DateTime.Now;
                order.Modified = DateTime.Now;
                order.Status = (int)Enum_OrderStatus.Created;
                order.UserProvider = item.UserID;
                order.UserReceiver = User.Identity.GetUserId();

                if (order.UserProvider == order.UserReceiver)
                {
                    TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                    TempData[TempDataKeys.UserMessage] = "You cannot book the item from yourself!";

                    return RedirectToAction("Listing", "Listing", new { id = order.ItemID });
                }

                if (order.ToDate.HasValue && order.FromDate.HasValue)
                {
                    order.Description = string.Format("{0} #{1} (From {2} To {3})",
                        item.Title, item.ID,
                        order.FromDate.Value.ToShortDateString(), order.ToDate.Value.ToShortDateString());

                    order.Quantity = order.ToDate.Value.Date.AddDays(1).Subtract(order.FromDate.Value.Date).Days;
                    order.Price = order.Quantity * item.Price;
                }
                else
                {
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

        [HttpPost]
        public async Task<ActionResult> Payment(int id, string stripeToken, string stripeEmail)
        {
            var selectQuery = await _orderService.Query(x => x.ID == id).Include(x => x.Item).SelectAsync();

            var order = selectQuery.FirstOrDefault();

            if (order == null)
                return new HttpNotFoundResult();

            var stripeConnectQuery = await _stripConnectService.Query(x => x.UserID == order.UserProvider).SelectAsync();
            var stripeConnect = stripeConnectQuery.FirstOrDefault();

            if (stripeConnect == null)
                return new HttpNotFoundResult();

            //https://stripe.com/docs/checkout
            var charge = new StripeChargeCreateOptions();

            // always set these properties
            charge.Amount = order.PriceInCents;
            charge.Currency = CacheHelper.Settings.Currency;
            charge.Card = new StripeCreditCardOptions()
            {
                TokenId = stripeToken
            };

            charge.ApplicationFee = 1000;
            charge.Capture = false;
            charge.Description = order.Description;
            charge.Destination = stripeConnect.stripe_user_id;
            var chargeService = new StripeChargeService(CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeApiKey).Value);
            StripeCharge stripeCharge = chargeService.Create(charge);

            // Update order status
            order.Status = (int)Enum_OrderStatus.Pending;
            _orderService.Update(order);

            // Save transaction
            var transaction = new OrderTransaction()
            {
                OrderID = id,
                ChargeID = stripeCharge.Id,
                StripeEmail = stripeEmail,
                StripeToken = stripeToken,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                FailureCode = stripeCharge.FailureCode,
                FailureMessage = stripeCharge.FailureMessage,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            };

            _orderTransactionService.Insert(transaction);

            await _unitOfWorkAsync.SaveChangesAsync();

            ClearCache();

            // Payment succeeded
            if (string.IsNullOrEmpty(stripeCharge.FailureCode))
            {
                TempData[TempDataKeys.UserMessage] = "Thanks for your order! You payment will not be charged until the provider accepted your request.";
                return RedirectToAction("Orders", "Payment");                
            }
            else
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = stripeCharge.FailureMessage;

                return RedirectToAction("Payment");
            }
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