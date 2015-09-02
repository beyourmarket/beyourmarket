using BeYourMarket.Core.Web;
using BeYourMarket.Model.Enum;
using BeYourMarket.Service;
using Plugin.Payment.Models;
using Repository.Pattern.UnitOfWork;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using RestSharp;
using BeYourMarket.Core.Controllers;
using Plugin.Payment.Stripe.Models;
using Plugin.Payment.Stripe.Models.Grids;
using Plugin.Payment.Services;
using Plugin.Payment.Stripe.Data;
using Microsoft.Practices.Unity;
using BeYourMarket.Service.Models;
using i18n;

namespace Plugin.Payment.Stripe.Controllers
{
    public class PaymentStripeController : Controller, IPaymentController
    {
        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly DataCacheService _dataCacheService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly IOrderService _orderService;

        private readonly IStripeTransactionService _transactionService;
        private readonly IStripeConnectService _stripConnectService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsyncStripe;

        public PaymentStripeController(
            ISettingDictionaryService settingDictionaryService,
            IUnitOfWorkAsync unitOfWorkAsync,
            DataCacheService dataCacheService,
            IOrderService orderService,
            IStripeTransactionService transationService,
            IStripeConnectService stripConnectService,
            [Dependency("unitOfWorkStripe")] IUnitOfWorkAsync unitOfWorkAsyncStripe)
        {
            _settingDictionaryService = settingDictionaryService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;

            _orderService = orderService;

            _transactionService = transationService;
            _stripConnectService = stripConnectService;
            _unitOfWorkAsyncStripe = unitOfWorkAsyncStripe;
        }

        #region FrontEnd Method
        /// <summary>
        /// Frontend
        /// </summary>
        /// <returns></returns>
        public ActionResult Payment(BeYourMarket.Model.Models.Order additionalData)
        {
            return View("~/Plugins/Plugin.Payment.Stripe/Views/Payment.cshtml", additionalData);
        }

        [HttpPost]
        public async Task<ActionResult> Payment(int id, string stripeToken, string stripeEmail)
        {
            var selectQuery = await _orderService.Query(x => x.ID == id).Include(x => x.Listing).SelectAsync();

            // Check if order exists
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
            charge.Source = new StripeSourceOptions()
            {
                TokenId = stripeToken
            };

            // set booking fee
            var bookingFee = (int)Math.Round(CacheHelper.Settings.TransactionFeePercent * order.PriceInCents);
            if (bookingFee < CacheHelper.Settings.TransactionMinimumFee * 100)
                bookingFee = (int)(CacheHelper.Settings.TransactionMinimumFee * 100);

            charge.ApplicationFee = bookingFee;
            charge.Capture = false;
            charge.Description = order.Description;
            charge.Destination = stripeConnect.stripe_user_id;
            var chargeService = new StripeChargeService(CacheHelper.GetSettingDictionary("StripeApiKey").Value);
            StripeCharge stripeCharge = chargeService.Create(charge);

            // Update order status
            order.Status = (int)Enum_OrderStatus.Pending;
            order.PaymentPlugin = StripePlugin.PluginName;
            _orderService.Update(order);

            // Save transaction
            var transaction = new StripeTransaction()
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

            _transactionService.Insert(transaction);

            await _unitOfWorkAsync.SaveChangesAsync();
            await _unitOfWorkAsyncStripe.SaveChangesAsync();

            ClearCache();

            // Payment succeeded
            if (string.IsNullOrEmpty(stripeCharge.FailureCode))
            {
                // Send message to the user
                var message = new MessageSendModel()
                {
                    UserFrom = order.UserReceiver,
                    UserTo = order.UserProvider,
                    Subject = order.Listing.Title,
                    ListingID = order.ListingID,
                    Body = HttpContext.ParseAndTranslate(string.Format(
                    "[[[Order Requested - %0 - Total Price %1 %2. <a href=\"%3\">See Details</a>|||{0}|||{1}|||{2}|||{3}]]]",
                    order.Description,
                    order.Price,
                    order.Currency,
                    Url.Action("Orders", "Payment")))
                };

                await MessageHelper.SendMessage(message);

                TempData[TempDataKeys.UserMessage] = "[[[Thanks for your order! You payment will not be charged until the provider accepted your request.]]]";
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

        public ActionResult PaymentSettingDeauthorize()
        {
            var userId = User.Identity.GetUserId();
            var stripeConnectQuery = _stripConnectService.Query(x => x.UserID == userId).Select();
            var stripeConnect = stripeConnectQuery.FirstOrDefault();

            // Delete old one and insert new one
            if (stripeConnect != null)
                _stripConnectService.Delete(stripeConnect);

            var client = new RestClient("https://connect.stripe.com/oauth/deauthorize");

            var request = new RestRequest(Method.POST);
            request.AddParameter("client_secret", CacheHelper.GetSettingDictionary("StripeApiKey").Value);
            request.AddParameter("client_id", CacheHelper.GetSettingDictionary("StripeClientID").Value);
            request.AddParameter("stripe_user_id", stripeConnect.stripe_user_id);

            var response = client.Execute(request);

            _unitOfWorkAsyncStripe.SaveChanges();

            TempData[TempDataKeys.UserMessage] = "[[[Disconnnect to stripe successfully!]]]";

            return RedirectToAction("PaymentSetting", "Payment", new { area = "" });
        }

        public ActionResult PaymentSetting(string scope, string code)
        {
            if (!string.IsNullOrEmpty(scope) && !string.IsNullOrEmpty(code))
            {
                var client = new RestClient("https://connect.stripe.com/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddParameter("client_secret", CacheHelper.GetSettingDictionary("StripeApiKey").Value);
                request.AddParameter("code", code);
                request.AddParameter("grant_type", "authorization_code");

                var response = client.Execute<StripeConnect>(request);

                if (string.IsNullOrEmpty(response.Data.error))
                {
                    var userId = User.Identity.GetUserId();
                    var stripeConnectQuery = _stripConnectService.Query(x => x.UserID == userId).Select();
                    var stripeConnect = stripeConnectQuery.FirstOrDefault();

                    // Delete old one and insert new one
                    if (stripeConnect != null)
                        _stripConnectService.Delete(stripeConnect);

                    response.Data.UserID = User.Identity.GetUserId();
                    response.Data.Created = DateTime.Now;
                    response.Data.LastUpdated = DateTime.Now;
                    response.Data.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;

                    _stripConnectService.Insert(response.Data);
                    _unitOfWorkAsyncStripe.SaveChanges();

                    TempData[TempDataKeys.UserMessage] = "[[[Connnect to stripe successfully!]]]";

                    return View("~/Plugins/Plugin.Payment.Stripe/Views/PaymentSetting.cshtml", Plugin.Payment.Stripe.StripePlugin.Enum_StripeConnectStatus.Authorized);
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
                var stripeConnectQuery = _stripConnectService.Query(x => x.UserID == userId).Select();
                var stripeConnect = stripeConnectQuery.FirstOrDefault();

                if (stripeConnect != null)
                    return View("~/Plugins/Plugin.Payment.Stripe/Views/PaymentSetting.cshtml", Plugin.Payment.Stripe.StripePlugin.Enum_StripeConnectStatus.Authorized);
            }

            return View("~/Plugins/Plugin.Payment.Stripe/Views/PaymentSetting.cshtml", Plugin.Payment.Stripe.StripePlugin.Enum_StripeConnectStatus.None);
        }

        public bool OrderAction(int id, int status, out string message)
        {
            message = string.Empty;

            var order = _orderService.Find(id);

            // Get the latest successful transaction
            var transactionQuery = _transactionService.Query(x => x.OrderID == id && string.IsNullOrEmpty(x.FailureCode)).Select();
            var transaction = transactionQuery.OrderByDescending(x => x.Created).FirstOrDefault();

            if (transaction == null)
            {
                message = "[[[Transaction not found]]]";
                return false;
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
                _transactionService.Update(transaction);

                // Capture payment
                var chargeService = new StripeChargeService(CacheHelper.GetSettingDictionary(StripePlugin.SettingStripeApiKey).Value);
                StripeCharge stripeCharge = chargeService.Capture(transaction.ChargeID);
            }

            _unitOfWorkAsync.SaveChanges();
            _unitOfWorkAsyncStripe.SaveChanges();

            return true;
        }

        public bool HasPaymentMethod(string userId)
        {
            return _stripConnectService.Query(x => x.UserID == userId).Select().Any();
        }

        public int GetTransactionCount()
        {
            return _transactionService.Queryable().Count();
        }

        public ActionResult Transaction()
        {
            var userId = User.Identity.GetUserId();

            var orders = _orderService.Query(x => x.Status == (int)Enum_OrderStatus.Confirmed && (x.UserProvider == userId || x.UserReceiver == userId)).Select();

            var orderIdPayment = orders.Where(x => x.UserProvider == userId).Select(x => x.ID);
            var orderIdPayout = orders.Where(x => x.UserReceiver == userId).Select(x => x.ID);

            var transactionPayment = _transactionService.Query(x => orderIdPayment.Contains(x.OrderID)).Select();
            var transactionPayout = _transactionService.Query(x => orderIdPayout.Contains(x.OrderID)).Select();

            // Set orders
            foreach (var item in transactionPayment)
            {
                item.Order = orders.Where(x => x.ID == item.OrderID).FirstOrDefault();
            }

            foreach (var item in transactionPayout)
            {
                item.Order = orders.Where(x => x.ID == item.OrderID).FirstOrDefault();
            }

            var transactionGridPayment = new TransactionGrid(transactionPayment.AsQueryable().OrderByDescending(x => x.Created));
            var transactionGridPayout = new TransactionGrid(transactionPayout.AsQueryable().OrderByDescending(x => x.Created));

            var model = new OrderTransactionModel()
            {
                TransactionPayment = transactionGridPayment,
                TransactionPayout = transactionGridPayout
            };

            return View("~/Plugins/Plugin.Payment.Stripe/Views/Transaction.cshtml", model);
        }
        #endregion

        #region Admin Method
        [Authorize(Roles = "Administrator")]
        public ActionResult Configure()
        {
            // Get payment info
            var model = new PaymentSettingModel()
            {
                Setting = CacheHelper.Settings,
                StripeClientID = CacheHelper.GetSettingDictionary(StripePlugin.SettingStripeClientID).Value,
                StripeApiKey = CacheHelper.GetSettingDictionary(StripePlugin.SettingStripeApiKey).Value,
                StripePublishableKey = CacheHelper.GetSettingDictionary(StripePlugin.SettingStripePublishableKey).Value
            };

            return View("~/Plugins/Plugin.Payment.Stripe/Views/Configure.cshtml", model);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult Configure(PaymentSettingModel model)
        {
            var stripeApiKey = _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, StripePlugin.SettingStripeApiKey);
            stripeApiKey.Value = model.StripeApiKey;
            _settingDictionaryService.SaveSettingDictionary(stripeApiKey);

            var stripeClientID = _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, StripePlugin.SettingStripeClientID);
            stripeClientID.Value = model.StripeClientID;
            _settingDictionaryService.SaveSettingDictionary(stripeClientID);

            var stripePublishableKey = _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, StripePlugin.SettingStripePublishableKey);
            stripePublishableKey.Value = model.StripePublishableKey;
            _settingDictionaryService.SaveSettingDictionary(stripePublishableKey);

            _unitOfWorkAsync.SaveChanges();

            _dataCacheService.RemoveCachedItem(CacheKeys.SettingDictionary);
            _dataCacheService.RemoveCachedItem(CacheKeys.Settings);

            TempData[TempDataKeys.UserMessage] = "[[[Plugin updated!]]]";

            return RedirectToAction("Plugins", "Plugin", new { area = "Admin" });
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult TransactionOverview()
        {
            var orders = _orderService.Query(x => x.Status == (int)Enum_OrderStatus.Confirmed).Select();

            var transactionPayment = _transactionService.Query().Select();

            // set orders
            foreach (var payment in transactionPayment)
            {
                payment.Order = orders.Where(x => x.ID == payment.OrderID).FirstOrDefault();
            }

            var transactionGridPayment = new TransactionGrid(transactionPayment.AsQueryable().OrderByDescending(x => x.Created));

            var model = new OrderTransactionModel()
            {
                TransactionPayment = transactionGridPayment
            };

            return View("~/Plugins/Plugin.Payment.Stripe/Views/TransactionOverview.cshtml", model);
        }
        #endregion

    }
}
