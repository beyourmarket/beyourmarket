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
using PagedList;
using BeYourMarket.Core.Web;

namespace BeYourMarket.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        #region Fields
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private readonly ISettingService _settingService;
        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly ICategoryService _categoryService;
        private readonly IListingService _listingService;
        private readonly IListingStatService _listingStatservice;
        private readonly IListingPictureService _ListingPictureservice;
        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;
        private readonly ICustomFieldListingService _customFieldListingService;

        private readonly IMessageThreadService _messageThreadService;
        private readonly IMessageService _messageService;
        private readonly IMessageParticipantService _messageParticipantService;
        private readonly IMessageReadStateService _messageReadStateService;

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
        public ManageController(
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
            IMessageService messageService,
            IMessageThreadService messageThreadService,
            IMessageParticipantService messageParticipantService,
            IMessageReadStateService messageReadStateService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _listingService = listingService;
            _pictureService = pictureService;
            _ListingPictureservice = ListingPictureservice;
            _orderService = orderService;

            _messageService = messageService;
            _messageThreadService = messageThreadService;
            _messageParticipantService = messageParticipantService;
            _messageReadStateService = messageReadStateService;

            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldListingService = customFieldListingService;
            _listingStatservice = listingStatservice;

            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;

            _unitOfWorkAsync = unitOfWorkAsync;
        }
        #endregion

        #region Methods - MVC5 default
        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            return RedirectToAction("Dashboard");

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "[[[Failed to verify phone]]]");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        /// <summary>
        /// http://stackoverflow.com/questions/6541302/thread-messaging-system-database-schema-design
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<ActionResult> Messages(string searchText, int? page)
        {
            if (searchText != null)
            {
                page = 1;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var userId = User.Identity.GetUserId();
            var participants = await _messageParticipantService.Query(x => x.UserID == userId).SelectAsync();

            // Get messages for the current user
            var threadIds = participants.Select(x => x.MessageThreadID).ToList();

            var messageThreads = await _messageThreadService
                .Query(x => threadIds.Contains(x.ID))
                .Include(x => x.Messages)
                .Include(x => x.Messages.Select(y => y.AspNetUser))
                .Include(x => x.Messages.Select(y => y.MessageReadStates))
                .Include(x => x.MessageParticipants)
                .Include(x => x.MessageParticipants.Select(y => y.AspNetUser))
                .SelectAsync();

            if (!string.IsNullOrEmpty(searchText))
            {
                messageThreads = messageThreads.Where(x => x.Messages.Where(y => y.Body.Contains(searchText)).Any());
            }

            messageThreads = messageThreads.OrderByDescending(x => x.Messages.Max(y => y.Created));

            var model = messageThreads.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        /// <summary>
        /// return messages sent from a specific user to the current user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ActionResult> Message(int threadId)
        {
            var userIdCurrent = User.Identity.GetUserId();

            var messageThread = await _messageThreadService
                .Query(x => x.ID == threadId)
                .Include(x => x.Messages)
                .Include(x => x.Messages.Select(y => y.AspNetUser))
                .Include(x => x.Messages.Select(y => y.MessageReadStates))
                .Include(x => x.MessageParticipants)
                .Include(x => x.MessageParticipants.Select(y => y.AspNetUser))
                .Include(x => x.Listing)
                .Include(x => x.Listing.ListingReviews)
                .Include(x => x.Listing.ListingPictures)
                .Include(x => x.Listing.ListingType)
                .SelectAsync();

            var model = messageThread.FirstOrDefault();

            // Redirect to inbox if the thread doesn't contain anything
            if (model == null)
                return RedirectToAction("Messages");

            // Redirect to inbox if the thread doesn't contain current user
            if (!model.MessageParticipants.Any(x => x.UserID == userIdCurrent))
                return RedirectToAction("Messages");

            // Update message read states
            var messageReadStates = await _messageReadStateService
                .Query(x => x.UserID == userIdCurrent && !x.ReadDate.HasValue && x.Message.MessageThreadID == threadId)
                .SelectAsync();

            foreach (var messageReadState in messageReadStates)
            {
                messageReadState.ReadDate = DateTime.Now;
                messageReadState.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _messageReadStateService.Update(messageReadState);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Message(int threadId, string messageText)
        {
            var userIdCurrent = User.Identity.GetUserId();

            var messageParticipants = await _messageParticipantService.Query(x => x.MessageThreadID == threadId).SelectAsync();
            if (!messageParticipants.Any(x => x.UserID == userIdCurrent))
                return RedirectToAction("Messages");

            // Add message
            var message = new Message()
            {
                MessageThreadID = threadId,
                UserFrom = userIdCurrent,
                Body = messageText,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
            };

            _messageService.Insert(message);

            await _unitOfWorkAsync.SaveChangesAsync();

            // Add message readstate
            foreach (var messageParticipant in messageParticipants.Where(x => x.UserID != userIdCurrent))
            {
                _messageReadStateService.Insert(new MessageReadState()
                {
                    MessageID = message.ID,
                    UserID = messageParticipant.UserID,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            TempData[TempDataKeys.UserMessage] = "[[[Message is sent.]]]";

            return RedirectToAction("Message", new { threadId = threadId });
        }

        [HttpPost]
        public async Task<ActionResult> MessageAction(List<int> messageIds, int actionType)
        {
            var action = (Enum_MessageAction)actionType;

            switch (action)
            {
                case Enum_MessageAction.MarkAsRead:
                    var messageReadStates = await _messageReadStateService
                        .Query(x => messageIds.Contains(x.MessageID) && !x.ReadDate.HasValue)
                        .SelectAsync();

                    foreach (var messageReadState in messageReadStates)
                    {
                        messageReadState.ReadDate = DateTime.Now;
                        _messageReadStateService.Update(messageReadState);
                    }

                    await _unitOfWorkAsync.SaveChangesAsync();

                    break;
                case Enum_MessageAction.None:
                default:
                    break;
            }

            return RedirectToAction("Messages");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Methods
        public async Task<ActionResult> Dashboard(string searchText)
        {
            var userId = User.Identity.GetUserId();
            var items = await _listingService.Query(x => x.UserID == userId).Include(x => x.ListingPictures).SelectAsync();

            // Filter string
            if (!string.IsNullOrEmpty(searchText))
                items = items.Where(x => x.Title.ToLower().Contains(searchText.ToLower().ToString()));

            var itemsModel = new List<ListingItemModel>();
            foreach (var item in items.OrderByDescending(x => x.Created))
            {
                itemsModel.Add(new ListingItemModel()
                {
                    ListingCurrent = item,
                    UrlPicture = item.ListingPictures.Count == 0 ? ImageHelper.GetListingImagePath(0) : ImageHelper.GetListingImagePath(item.ListingPictures.OrderBy(x => x.Ordering).FirstOrDefault().PictureID)
                });
            }

            var model = new ListingModel()
            {
                Listings = itemsModel
            };

            return View(model);
        }

        public async Task<ActionResult> UserProfile()
        {
            var userId = User.Identity.GetUserId();

            var user = await UserManager.FindByIdAsync(userId);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProfileUpdate(ApplicationUser user, HttpPostedFileBase file)
        {
            var userId = User.Identity.GetUserId();

            var userExisting = await UserManager.FindByIdAsync(userId);

            userExisting.FirstName = user.FirstName;
            userExisting.LastName = user.LastName;
            userExisting.Gender = user.Gender;
            userExisting.PhoneNumber = user.PhoneNumber;

            await UserManager.UpdateAsync(userExisting);

            if (file != null)
            {
                // Format is automatically detected though can be changed.
                ISupportedImageFormat format = new JpegFormat { Quality = 90 };
                Size size = new Size(300, 300);

                //https://naimhamadi.wordpress.com/2014/06/25/processing-images-in-c-easily-using-imageprocessor/
                // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                {
                    var path = Path.Combine(Server.MapPath("~/images/profile"), string.Format("{0}.{1}", userId, "jpg"));

                    // Load, resize, set the format and quality and save an image.
                    imageFactory.Load(file.InputStream)
                        .Resize(size)
                        .Format(format)
                        .Save(path);
                }
            }

            return RedirectToAction("UserProfile", "Manage");
        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}