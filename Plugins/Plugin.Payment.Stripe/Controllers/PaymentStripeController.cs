using BeYourMarket.Core.Web;
using BeYourMarket.Model.Enum;
using BeYourMarket.Model.Models;
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

namespace Plugin.Payment.Stripe.Controllers
{
    public class PaymentStripeController : Controller
    {
        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly DataCacheService _dataCacheService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly IOrderService _orderService;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IStripeConnectService _stripConnectService;

        public PaymentStripeController(
            ISettingDictionaryService settingDictionaryService,
            IUnitOfWorkAsync unitOfWorkAsync,
            DataCacheService dataCacheService,
            IOrderService orderService,
            IOrderTransactionService orderTransationService,
            IStripeConnectService stripConnectService)
        {
            _settingDictionaryService = settingDictionaryService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;

            _orderService = orderService;
            _orderTransactionService = orderTransationService;

            _stripConnectService = stripConnectService;
        }

        #region FrontEnd Method
        /// <summary>
        /// Frontend
        /// </summary>
        /// <returns></returns>
        public ActionResult Payment(Order additionalData)
        {
            return View("~/Plugins/Plugin.Payment.Stripe/Views/Payment.cshtml", additionalData);
        }

        [HttpPost]
        public async Task<ActionResult> Payment(int id, string stripeToken, string stripeEmail)
        {
            var selectQuery = await _orderService.Query(x => x.ID == id).Include(x => x.Item).SelectAsync();

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
            charge.Card = new StripeCreditCardOptions()
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
            request.AddParameter("client_secret", CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeApiKey).Value);
            request.AddParameter("client_id", CacheHelper.GetSettingDictionary(Enum_SettingKey.StripeClientID).Value);
            request.AddParameter("stripe_user_id", stripeConnect.stripe_user_id);

            var response = client.Execute(request);

            _unitOfWorkAsync.SaveChanges();

            TempData[TempDataKeys.UserMessage] = "Disconnnect to stripe successfully!";

            return RedirectToAction("PaymentSetting", "Payment", new { area = "" });
        }

        public ActionResult PaymentSetting(string scope, string code)
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
                    _unitOfWorkAsync.SaveChanges();

                    TempData[TempDataKeys.UserMessage] = "Connnect to stripe successfully!";

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
        #endregion

        #region Admin Method
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


            return RedirectToAction("Plugins", "Plugin", new { area = "Admin" });
        }
        #endregion
    }
}
