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
using i18n;
using i18n.Helpers;
using BeYourMarket.Core.Web;
using BeYourMarket.Core.Plugins;
using BeYourMarket.Core;
using BeYourMarket.Core.Controllers;
using Microsoft.Practices.Unity;
using System.Globalization;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ManageController : Controller
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
        public ManageController(
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
        public ActionResult Index()
        {
            var model = CacheHelper.Statistics;
            model.TransactionCount = 0;

            // Get transaction count
            var descriptors = _pluginFinder.GetPluginDescriptors(LoadPluginsMode.InstalledOnly, "Payment");
            foreach (var descriptor in descriptors)
            {
                var controllerType = descriptor.Instance<IHookPlugin>().GetControllerType();
                var controller = ContainerManager.GetConfiguredContainer().Resolve(controllerType) as IPaymentController;
                model.TransactionCount += controller.GetTransactionCount();
            }

            return View("Dashboard", model);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        public async Task<ActionResult> UserUpdate(string id)
        {
            var model = new ApplicationUser();

            if (string.IsNullOrEmpty(id))
            {
                return View(model);
            }

            model = await UserManager.FindByIdAsync(id);

            //http://stackoverflow.com/questions/24588758/how-to-iterate-roles-in-ienumerableapplicationuser-and-display-role-names-in-r
            //http://stackoverflow.com/questions/27347802/how-to-list-users-with-role-names-in-asp-net-mvc-5
            var roleAdministrator = await RoleManager.FindByNameAsync(Enum_UserType.Administrator.ToString());
            model.RoleAdministrator = model.Roles.Any(x => x.RoleId == roleAdministrator.Id);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> UserDelete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpNotFoundResult();

            var model = await UserManager.FindByIdAsync(id);

            //http://stackoverflow.com/questions/24588758/how-to-iterate-roles-in-ienumerableapplicationuser-and-display-role-names-in-r
            //http://stackoverflow.com/questions/27347802/how-to-list-users-with-role-names-in-asp-net-mvc-5
            var roleAdministrator = await RoleManager.FindByNameAsync(Enum_UserType.Administrator.ToString());
            model.RoleAdministrator = model.Roles.Any(x => x.RoleId == roleAdministrator.Id);

            if (model.RoleAdministrator)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You cannot delete Administrator, change the user role first.]]]";
                return RedirectToAction("Users");
            }

            // delete user
            await UserManager.DeleteAsync(model);
            _dataCacheService.RemoveCachedItem(CacheKeys.Statistics);

            TempData[TempDataKeys.UserMessage] = string.Format("[[[User {0} is deleted.]]]", model.FullName);
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<ActionResult> UserUpdate(ApplicationUser user)
        {
            // Create user if there is no user id
            var existingUser = await UserManager.FindByIdAsync(user.Id);
            if (existingUser == null)
            {
                user.UserName = user.Email;
                user.Email = user.Email;
                user.RegisterDate = DateTime.Now;
                user.RegisterIP = System.Web.HttpContext.Current.Request.GetVisitorIP();
                user.LastAccessDate = DateTime.Now;
                user.LastAccessIP = System.Web.HttpContext.Current.Request.GetVisitorIP();

                //var result = await UserManager.CreateAsync(user);
                var result = await UserManager.CreateAsync(user,Request.Form["Password"]);

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View(user);
                }

                // Update cache
                _dataCacheService.RemoveCachedItem(CacheKeys.Statistics);
            }

            existingUser = await UserManager.FindByIdAsync(user.Id);
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Gender = user.Gender;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            existingUser.Email = user.Email;
            existingUser.EmailConfirmed = user.EmailConfirmed;
            existingUser.AcceptEmail = user.AcceptEmail;
            existingUser.Disabled = user.Disabled;

            // roles handling
            if (user.RoleAdministrator)
            {
                await UserManager.AddToRoleAsync(existingUser.Id, Enum_UserType.Administrator.ToString());
            }
            else
            {
                await UserManager.RemoveFromRoleAsync(existingUser.Id, Enum_UserType.Administrator.ToString());
            }

            await UserManager.UpdateAsync(existingUser);

            // Update new password if there is one
            var newPassword = Request.Form["Password"].ToString();
            if (!string.IsNullOrEmpty(newPassword))
            {
                var resetToken = await UserManager.GeneratePasswordResetTokenAsync(existingUser.Id);
                await UserManager.ResetPasswordAsync(existingUser.Id, resetToken, newPassword);
            }

            return RedirectToAction("Users");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public ActionResult Users()
        {
            return View(UserManager.Users.ToList());
        }

        public ActionResult SettingsEmail()
        {
            var setting = _settingService.Queryable().FirstOrDefault();

            return View(setting);
        }

        [HttpPost]
        public async Task<ActionResult> SettingsEmailUpdate(Setting setting)
        {
            var settingExisting = _settingService.Queryable().FirstOrDefault();

            settingExisting.SmtpHost = setting.SmtpHost;
            settingExisting.SmtpPassword = setting.SmtpPassword;
            settingExisting.SmtpPort = setting.SmtpPort;
            settingExisting.SmtpUserName = setting.SmtpUserName;
            settingExisting.SmtpPassword = setting.SmtpPassword;
            settingExisting.SmtpSSL = setting.SmtpSSL;
            settingExisting.EmailDisplayName = setting.EmailDisplayName;
            settingExisting.EmailAddress = setting.EmailAddress;

            settingExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

            _settingService.Update(settingExisting);

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.UpdateCache(CacheKeys.Settings, settingExisting);

            return RedirectToAction("SettingsEmail");
        }

        public ActionResult Settings()
        {
            var setting = _settingService.Queryable().FirstOrDefault();

            if (string.IsNullOrEmpty(setting.DateFormat))
            {
                setting.TimeFormat = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;
            }

            if (string.IsNullOrEmpty(setting.DateFormat))
            {
                setting.DateFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            }

            return View(setting);
        }

        [HttpPost]
        public async Task<ActionResult> SettingsUpdate(Setting setting)
        {
            var settingExisting = _settingService.Queryable().FirstOrDefault();

            settingExisting.Name = setting.Name;
            settingExisting.Description = setting.Description;
            settingExisting.Slogan = setting.Slogan;
            settingExisting.SearchPlaceHolder = setting.SearchPlaceHolder;

            settingExisting.EmailContact = setting.EmailContact;
            settingExisting.EmailConfirmedRequired = setting.EmailConfirmedRequired;

            settingExisting.Currency = setting.Currency;

            settingExisting.AgreementRequired = setting.AgreementRequired;
            settingExisting.AgreementLabel = setting.AgreementLabel;
            settingExisting.AgreementText = setting.AgreementText;
            settingExisting.SignupText = setting.SignupText;

            settingExisting.Theme = setting.Theme;

            settingExisting.DateFormat = setting.DateFormat;
            settingExisting.TimeFormat = setting.TimeFormat;

            settingExisting.ListingReviewEnabled = setting.ListingReviewEnabled;
            settingExisting.ListingReviewMaxPerDay = setting.ListingReviewMaxPerDay;

            settingExisting.LastUpdated = setting.LastUpdated;
            settingExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

            _settingService.Update(settingExisting);

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.UpdateCache(CacheKeys.Settings, settingExisting);

            return RedirectToAction("Settings");
        }

        public ActionResult EmailTemplates()
        {
            var grid = new EmailTemplatesGrid(_emailTemplateService.Queryable().OrderByDescending(x => x.Created));

            return View(grid);
        }

        public async Task<ActionResult> EmailTemplateUpdate(int? id)
        {
            var model = await _emailTemplateService.FindAsync(id);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> EmailTemplateUpdate(EmailTemplate emailTemplate)
        {
            var userId = User.Identity.GetUserId();

            if (emailTemplate.ID == 0)
            {
                emailTemplate.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                emailTemplate.Created = DateTime.Now;
                emailTemplate.LastUpdated = DateTime.Now;

                _emailTemplateService.Insert(emailTemplate);
            }
            else
            {
                var emailTempalteExisting = await _emailTemplateService.FindAsync(emailTemplate.ID);

                emailTempalteExisting.Subject = emailTemplate.Subject;
                emailTempalteExisting.Body = emailTemplate.Body;
                emailTempalteExisting.Slug = emailTemplate.Slug;

                emailTempalteExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
                emailTempalteExisting.LastUpdated = DateTime.Now;

                _emailTemplateService.Update(emailTempalteExisting);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.RemoveCachedItem(CacheKeys.EmailTemplates);

            return RedirectToAction("EmailTemplates");
        }

        [HttpPost]
        public async Task<ActionResult> EmailTemplateTest(int id)
        {
            var emailTemplate = await _emailTemplateService.FindAsync(id);

            if (emailTemplate == null)
                return new HttpNotFoundResult();

            dynamic email = new Postal.Email("Email");
            email.To = CacheHelper.Settings.EmailContact;
            email.From = CacheHelper.Settings.EmailAddress;
            email.Subject = "[[[Testing]]] - " + emailTemplate.Subject;
            email.Body = emailTemplate.Body;

            EmailHelper.SendEmail(email);

            TempData[TempDataKeys.UserMessage] = string.Format("[[[Message sent to {0} succesfully!]]]", CacheHelper.Settings.EmailContact);

            return RedirectToAction("EmailTemplateUpdate", new { id = id });
        }

        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
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
                Response.Cookies.Add(new System.Web.HttpCookie("i18n.langtag")
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

        public ActionResult SettingsLanguage()
        {
            var availableLangauges = i18n.LanguageHelpers.GetAppLanguages();

            var modelFile = LanguageHelper.GetLanguages();
            var model = new LanguageSettingModel()
            {
                DefaultCulture = modelFile.DefaultCulture
            };

            foreach (var lang in availableLangauges)
            {
                var languageSetting = new LanguageSetting();
                languageSetting.Culture = lang.Key;
                languageSetting.LanguageTag = lang.Value;

                var existingLang = modelFile.Languages.Find(x => x.Culture == lang.Key);
                if (existingLang != null)
                {
                    languageSetting.Enabled = existingLang.Enabled;
                }

                model.Languages.Add(languageSetting);
            }

            return View(model);
        }

        public ActionResult SettingsLanguageUpdate(LanguageSettingModel model)
        {
            // set languageTag as it's not posted back
            foreach (var item in model.Languages)
            {
                item.LanguageTag = new LanguageTag(item.Culture);
            }

            var languagesEnabled = model.Languages.Where(x => x.Enabled);

            if (languagesEnabled.Count() == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[At least one language should be enabled!]]]";
                return View("SettingsLanguage", model);
            }

            if (!languagesEnabled.Any(x => x.Culture == model.DefaultCulture))
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[Default language must be enabled first!]]]";
                return View("SettingsLanguage", model);
            }

            LanguageHelper.SaveLanguages(model);

            // Update cache
            LanguageHelper.Refresh();

            return SetLanguage(model.DefaultCulture, Url.Action("SettingsLanguage", "Manage"));
        }

        #endregion
    }
}