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
using i18n;
using i18n.Helpers;
using BeYourMarket.Core.Web;
using BeYourMarket.Core.Plugins;
using BeYourMarket.Core;
using BeYourMarket.Core.Controllers;
using Microsoft.Practices.Unity;

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
        private readonly IItemService _itemService;

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
            IItemService itemService,
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
            _itemService = itemService;
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
            if (string.IsNullOrEmpty(id))
                return new HttpNotFoundResult();

            var model = await UserManager.FindByIdAsync(id);

            //http://stackoverflow.com/questions/24588758/how-to-iterate-roles-in-ienumerableapplicationuser-and-display-role-names-in-r
            //http://stackoverflow.com/questions/27347802/how-to-list-users-with-role-names-in-asp-net-mvc-5
            var roleAdministrator = await RoleManager.FindByNameAsync(Enum_UserType.Administrator.ToString());
            model.RoleAdministrator = model.Roles.Any(x => x.RoleId == roleAdministrator.Id);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> UserUpdate(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(user.Id))
                return new HttpNotFoundResult();

            var existingUser = await UserManager.FindByIdAsync(user.Id);
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

            return RedirectToAction("Users");
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
            settingExisting.BookingEnabled = setting.BookingEnabled;
            settingExisting.BookingText = setting.BookingText;
            settingExisting.Currency = setting.Currency;

            settingExisting.AgreementRequired = setting.AgreementRequired;
            settingExisting.AgreementLabel = setting.AgreementLabel;
            settingExisting.AgreementText = setting.AgreementText;
            settingExisting.SignupText = setting.SignupText;

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
            email.From = CacheHelper.Settings.EmailContact;
            email.Subject = "[Testing] " + emailTemplate.Subject;
            email.Body = emailTemplate.Body;

            EmailHelper.SendEmail(email);

            TempData[TempDataKeys.UserMessage] = "Message sent succesfully!";

            return RedirectToAction("EmailTemplateUpdate", new { id = id });
        }

        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            var languages = i18n.LanguageHelpers.GetAppLanguages();
            var languageCurrent = ControllerContext.RequestContext.HttpContext.GetPrincipalAppLanguageForRequest();

            var model = new LanguageSelectorModel();
            model.Culture = languageCurrent.GetLanguage();
            model.DisplayName = languageCurrent.GetCultureInfo().DisplayName;

            foreach (var language in languages)
            {
                if (language.Key != languageCurrent.GetLanguage())
                {
                    model.LanguageList.Add(new LanguageSelectorModel()
                    {
                        Culture = language.Key,
                        DisplayName = language.Value.CultureInfo.DisplayName
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
        #endregion
    }
}