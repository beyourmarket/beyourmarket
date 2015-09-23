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
using BeYourMarket.Service.Models;
using Microsoft.Practices.Unity;
using BeYourMarket.Web.Extensions;

namespace BeYourMarket.Web.Controllers
{
    [Authorize]
    public class ListingController : Controller
    {
        #region Fields
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private readonly ISettingService _settingService;
        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly ICategoryService _categoryService;

        private readonly IListingService _listingService;
        private readonly IListingStatService _ListingStatservice;
        private readonly IListingPictureService _listingPictureservice;
        private readonly IListingReviewService _listingReviewService;

        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;
        private readonly ICustomFieldListingService _customFieldListingService;

        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IMessageService _messageService;
        private readonly IMessageThreadService _messageThreadService;
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

        #region Contructors
        public ListingController(
           IUnitOfWorkAsync unitOfWorkAsync,
           ISettingService settingService,
           ICategoryService categoryService,
           IListingService listingService,
           IPictureService pictureService,
           IListingPictureService listingPictureservice,
           IOrderService orderService,
           ICustomFieldService customFieldService,
           ICustomFieldCategoryService customFieldCategoryService,
           ICustomFieldListingService customFieldListingService,
           ISettingDictionaryService settingDictionaryService,
           IListingStatService listingStatservice,
            IListingReviewService listingReviewService,
           IEmailTemplateService emailTemplateService,
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
            _listingReviewService = listingReviewService;
            _pictureService = pictureService;
            _listingPictureservice = listingPictureservice;
            _orderService = orderService;
            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldListingService = customFieldListingService;
            _ListingStatservice = listingStatservice;
            _emailTemplateService = emailTemplateService;
            _messageService = messageService;
            _messageParticipantService = messageParticipantService;
            _messageReadStateService = messageReadStateService;
            _messageThreadService = messageThreadService;
            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;

            _unitOfWorkAsync = unitOfWorkAsync;
        }
        #endregion

        #region Methods
        //http://stackoverflow.com/questions/11774741/load-partial-view-depending-on-dropdown-selection-in-mvc3
        public async Task<ActionResult> ListingPartial(int categoryID)
        {
            // Custom fields
            var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == categoryID).Include(x => x.MetaField.ListingMetas).SelectAsync();
            var customFieldCategories = customFieldCategoryQuery.ToList();
            var customFieldModel = new CustomFieldListingModel()
            {
                MetaCategories = customFieldCategories
            };

            return PartialView("_CategoryCustomFields", customFieldModel);
        }

        public async Task<ActionResult> ListingTypesPartial(int categoryID, int listingID)
        {
            var model = new ListingUpdateModel();
            model.ListingTypes = CacheHelper.ListingTypes.Where(x => x.CategoryListingTypes.Any(y => y.CategoryID == categoryID)).ToList();
            model.ListingItem = new Listing();

            if (listingID > 0)
                model.ListingItem = await _listingService.FindAsync(listingID);

            model.ListingTypeID = model.ListingItem.ListingTypeID;

            return PartialView("_ListingTypes", model);
        }

        [HttpGet]
        public ActionResult ListingType(int listingTypeID)
        {
            var listingType = CacheHelper.ListingTypes.Where(x => x.ID == listingTypeID).FirstOrDefault();

            if (listingType == null)
                return new JsonResult();

            return Json(new
            {
                PaymentEnabled = listingType.PaymentEnabled,
                PriceEnabled = listingType.PriceEnabled
            }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ListingUpdate(int? id)
        {
            if (CacheHelper.Categories.Count == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[There are not categories available yet.]]]";
            }

            Listing listing;

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            var model = new ListingUpdateModel()
            {
                Categories = CacheHelper.Categories
            };

            if (id.HasValue)
            {
                // return unauthorized if not authenticated
                if (!User.Identity.IsAuthenticated)
                    return new HttpUnauthorizedResult();

                if (await NotMeListing(id.Value))
                    return new HttpUnauthorizedResult();

                listing = await _listingService.FindAsync(id);

                if (listing == null)
                    return new HttpNotFoundResult();

                // Pictures
                var pictures = await _listingPictureservice.Query(x => x.ListingID == id).SelectAsync();

                var picturesModel = pictures.Select(x =>
                    new PictureModel()
                    {
                        ID = x.PictureID,
                        Url = ImageHelper.GetListingImagePath(x.PictureID),
                        ListingID = x.ListingID,
                        Ordering = x.Ordering
                    }).OrderBy(x => x.Ordering).ToList();

                model.Pictures = picturesModel;
            }
            else
            {
                listing = new Listing()
                {
                    CategoryID = CacheHelper.Categories.Any() ? CacheHelper.Categories.FirstOrDefault().ID : 0,
                    Created = DateTime.Now.Date,
                    LastUpdated = DateTime.Now.Date,
                    Expiration = DateTime.MaxValue,
                    Enabled = true,
                    Active = true,
                };

                if (User.Identity.IsAuthenticated)
                {
                    listing.ContactEmail = user.Email;
                    listing.ContactName = string.Format("{0} {1}", user.FirstName, user.LastName);
                    listing.ContactPhone = user.PhoneNumber;
                }
            }

            // Populate model with listing
            await PopulateListingUpdateModel(listing, model);

            return View("~/Views/Listing/ListingUpdate.cshtml", model);
        }

        private async Task<ListingUpdateModel> PopulateListingUpdateModel(Listing listing, ListingUpdateModel model)
        {
            model.ListingItem = listing;

            // Custom fields
            var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == listing.CategoryID).Include(x => x.MetaField.ListingMetas).SelectAsync();
            var customFieldCategories = customFieldCategoryQuery.ToList();
            var customFieldModel = new CustomFieldListingModel()
            {
                ListingID = listing.ID,
                MetaCategories = customFieldCategories
            };

            model.CustomFields = customFieldModel;
            model.UserID = listing.UserID;
            model.CategoryID = listing.CategoryID;
            model.ListingTypeID = listing.ListingTypeID;

            // Listing types
            model.ListingTypes = CacheHelper.ListingTypes.Where(x => x.CategoryListingTypes.Any(y => y.CategoryID == model.CategoryID)).ToList();

            // Listing Categories
            model.Categories = CacheHelper.Categories;

            return model;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Listing(int id)
        {
            var itemQuery = await _listingService.Query(x => x.ID == id)
                .Include(x => x.Category)
                .Include(x => x.ListingMetas)
                .Include(x => x.ListingMetas.Select(y => y.MetaField))
                .Include(x => x.ListingStats)
                .Include(x => x.ListingType)
                .SelectAsync();

            var item = itemQuery.FirstOrDefault();

            if (item == null)
                return new HttpNotFoundResult();

            var orders = _orderService.Queryable()
                .Where(x => x.ListingID == id
                    && (x.Status != (int)Enum_OrderStatus.Pending || x.Status != (int)Enum_OrderStatus.Confirmed)
                    && (x.FromDate.HasValue && x.ToDate.HasValue)
                    && (x.FromDate >= DateTime.Now || x.ToDate >= DateTime.Now))
                    .ToList();

            List<DateTime> datesBooked = new List<DateTime>();
            foreach (var order in orders)
            {
                for (DateTime date = order.FromDate.Value; date <= order.ToDate.Value; date = date.Date.AddDays(1))
                {
                    datesBooked.Add(date);
                }
            }

            var pictures = await _listingPictureservice.Query(x => x.ListingID == id).SelectAsync();

            var picturesModel = pictures.Select(x =>
                new PictureModel()
                {
                    ID = x.PictureID,
                    Url = ImageHelper.GetListingImagePath(x.PictureID),
                    ListingID = x.ListingID,
                    Ordering = x.Ordering
                }).OrderBy(x => x.Ordering).ToList();

            var reviews = await _listingReviewService
                .Query(x => x.UserTo == item.UserID)
                .Include(x => x.AspNetUserFrom)
                .SelectAsync();

            var user = await UserManager.FindByIdAsync(item.UserID);

            var itemModel = new ListingItemModel()
            {
                ListingCurrent = item,
                Pictures = picturesModel,
                DatesBooked = datesBooked,
                User = user,
                ListingReviews = reviews.ToList()
            };

            // Update stat count
            var itemStat = item.ListingStats.FirstOrDefault();
            if (itemStat == null)
            {
                _ListingStatservice.Insert(new ListingStat()
                {
                    ListingID = id,
                    CountView = 1,
                    Created = DateTime.Now,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });
            }
            else
            {
                itemStat.CountView++;
                itemStat.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
                _ListingStatservice.Update(itemStat);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            return View("~/Views/Listing/Listing.cshtml", itemModel);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ListingUpdate(Listing listing, FormCollection form, IEnumerable<HttpPostedFileBase> files)
        {
            if (CacheHelper.Categories.Count == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[There are not categories available yet.]]]";

                return RedirectToAction("Listing", new { id = listing.ID });
            }

            var userIdCurrent = User.Identity.GetUserId();

            // Register account if not login
            if (!User.Identity.IsAuthenticated)
            {
                var accountController = BeYourMarket.Core.ContainerManager.GetConfiguredContainer().Resolve<AccountController>();

                var modelRegister = new RegisterViewModel()
                {
                    Email = listing.ContactEmail,
                    Password = form["Password"],
                    ConfirmPassword = form["ConfirmPassword"],
                };

                // Parse first and last name
                var names = listing.ContactName.Split(' ');
                if (names.Length == 1)
                {
                    modelRegister.FirstName = names[0];
                }
                else if (names.Length == 2)
                {
                    modelRegister.FirstName = names[0];
                    modelRegister.LastName = names[1];
                }
                else if (names.Length > 2)
                {
                    modelRegister.FirstName = names[0];
                    modelRegister.LastName = listing.ContactName.Substring(listing.ContactName.IndexOf(" ") + 1);
                }

                // Register account
                var resultRegister = await accountController.RegisterAccount(modelRegister);

                // Add errors
                AddErrors(resultRegister);

                // Show errors if not succeed
                if (!resultRegister.Succeeded)
                {
                    var model = new ListingUpdateModel()
                    {
                        ListingItem = listing
                    };
                    // Populate model with listing
                    await PopulateListingUpdateModel(listing, model);
                    return View("ListingUpdate", model);
                }

                // update current user id
                var user = await UserManager.FindByNameAsync(listing.ContactEmail);
                userIdCurrent = user.Id;
            }

            bool updateCount = false;

            int nextPictureOrderId = 0;

            // Set default listing type ID
            if (listing.ListingTypeID == 0)
            {
                var listingTypes = CacheHelper.ListingTypes.Where(x => x.CategoryListingTypes.Any(y => y.CategoryID == listing.CategoryID));

                if (listingTypes == null)
                {
                    TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                    TempData[TempDataKeys.UserMessage] = "[[[There are not listing types available yet.]]]";

                    return RedirectToAction("Listing", new { id = listing.ID });
                }

                listing.ListingTypeID = listingTypes.FirstOrDefault().ID;
            }

            if (listing.ID == 0)
            {
                listing.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                listing.IP = Request.GetVisitorIP();
                listing.Expiration = DateTime.MaxValue.AddDays(-1);
                listing.UserID = userIdCurrent;
                listing.Enabled = true;
                listing.Currency = CacheHelper.Settings.Currency;

                updateCount = true;
                _listingService.Insert(listing);
            }
            else
            {
                if (await NotMeListing(listing.ID))
                    return new HttpUnauthorizedResult();

                var listingExisting = await _listingService.FindAsync(listing.ID);

                listingExisting.Title = listing.Title;
                listingExisting.Description = listing.Description;
                listingExisting.Active = listing.Active;
                listingExisting.Price = listing.Price;

                listingExisting.ContactEmail = listing.ContactEmail;
                listingExisting.ContactName = listing.ContactName;
                listingExisting.ContactPhone = listing.ContactPhone;

                listingExisting.Latitude = listing.Latitude;
                listingExisting.Longitude = listing.Longitude;
                listingExisting.Location = listing.Location;

                listingExisting.ShowPhone = listing.ShowPhone;
                listingExisting.ShowEmail = listing.ShowEmail;

                listingExisting.CategoryID = listing.CategoryID;
                listingExisting.ListingTypeID = listing.ListingTypeID;

                listingExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _listingService.Update(listingExisting);
            }

            // Delete existing fields on item
            var customFieldItemQuery = await _customFieldListingService.Query(x => x.ListingID == listing.ID).SelectAsync();
            var customFieldIds = customFieldItemQuery.Select(x => x.ID).ToList();
            foreach (var customFieldId in customFieldIds)
            {
                await _customFieldListingService.DeleteAsync(customFieldId);
            }

            // Get custom fields
            var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == listing.CategoryID).Include(x => x.MetaField.ListingMetas).SelectAsync();
            var customFieldCategories = customFieldCategoryQuery.ToList();

            foreach (var metaCategory in customFieldCategories)
            {
                var field = metaCategory.MetaField;
                var controlType = (BeYourMarket.Model.Enum.Enum_MetaFieldControlType)field.ControlTypeID;

                string controlId = string.Format("customfield_{0}_{1}_{2}", metaCategory.ID, metaCategory.CategoryID, metaCategory.FieldID);

                var formValue = form[controlId];

                if (string.IsNullOrEmpty(formValue))
                    continue;

                formValue = formValue.ToString();

                var itemMeta = new ListingMeta()
                {
                    ListingID = listing.ID,
                    Value = formValue,
                    FieldID = field.ID,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                };

                _customFieldListingService.Insert(itemMeta);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            if (Request.Files.Count > 0)
            {
                var itemPictureQuery = _listingPictureservice.Queryable().Where(x => x.ListingID == listing.ID);
                if (itemPictureQuery.Count() > 0)
                    nextPictureOrderId = itemPictureQuery.Max(x => x.Ordering);
            }

            if (files != null && files.Count() > 0)
            {
                foreach (HttpPostedFileBase file in files)
                {
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        // Picture picture and get id
                        var picture = new Picture();
                        picture.MimeType = "image/jpeg";
                        _pictureService.Insert(picture);
                        await _unitOfWorkAsync.SaveChangesAsync();

                        // Format is automatically detected though can be changed.
                        ISupportedImageFormat format = new JpegFormat { Quality = 90 };
                        Size size = new Size(500, 0);

                        //https://naimhamadi.wordpress.com/2014/06/25/processing-images-in-c-easily-using-imageprocessor/
                        // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            var path = Path.Combine(Server.MapPath("~/images/listing"), string.Format("{0}.{1}", picture.ID.ToString("00000000"), "jpg"));

                            // Load, resize, set the format and quality and save an image.
                            imageFactory.Load(file.InputStream)
                                        .Resize(size)
                                        .Format(format)
                                        .Save(path);
                        }

                        var itemPicture = new ListingPicture();
                        itemPicture.ListingID = listing.ID;
                        itemPicture.PictureID = picture.ID;
                        itemPicture.Ordering = nextPictureOrderId;

                        _listingPictureservice.Insert(itemPicture);

                        nextPictureOrderId++;
                    }
                }
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            // Update statistics count
            if (updateCount)
            {
                _sqlDbService.UpdateCategoryItemCount(listing.CategoryID);
                _dataCacheService.RemoveCachedItem(CacheKeys.Statistics);
            }

            TempData[TempDataKeys.UserMessage] = "[[[Listing is updated!]]]";
            return RedirectToAction("Listing", new { id = listing.ID });
        }

        [HttpPost]
        public async Task<ActionResult> ListingDelete(int id)
        {
            var item = await _listingService.FindAsync(id);
            var orderQuery = await _orderService.Query(x => x.ListingID == id).SelectAsync();

            // Delete item if no orders associated with it
            if (item.Orders.Count > 0)
            {
                var resultFailed = new { Success = false, Message = "[[[You cannot delete item with orders! You can deactivate it instead.]]]" };
                return Json(resultFailed, JsonRequestBehavior.AllowGet);
            }

            await _listingService.DeleteAsync(id);

            await _unitOfWorkAsync.SaveChangesAsync();

            var result = new { Success = true, Message = "[[[Your listing has been deleted.]]]" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ListingPhotoDelete(int id)
        {
            try
            {
                await _pictureService.DeleteAsync(id);
                var itemPicture = _listingPictureservice.Query(x => x.PictureID == id).Select().FirstOrDefault();

                if (itemPicture != null)
                    await _listingPictureservice.DeleteAsync(itemPicture.ID);

                await _unitOfWorkAsync.SaveChangesAsync();

                var path = Path.Combine(Server.MapPath("~/images/listing"), string.Format("{0}.{1}", id.ToString("00000000"), "jpg"));

                System.IO.File.Delete(path);

                var result = new { Success = "true", Message = "" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = "false", Message = ex.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        public async new Task<ActionResult> Profile(string id)
        {
            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
                return new HttpNotFoundResult();

            var items = await _listingService.Query(x => x.UserID == id)
                .Include(x => x.ListingPictures)
                .Include(x => x.ListingType)
                .Include(x => x.AspNetUser)
                .Include(x => x.ListingReviews)
                .SelectAsync();

            var itemsModel = new List<ListingItemModel>();
            foreach (var item in items.OrderByDescending(x => x.Created))
            {
                itemsModel.Add(new ListingItemModel()
                {
                    ListingCurrent = item,
                    UrlPicture = item.ListingPictures.Count == 0 ? ImageHelper.GetListingImagePath(0) : ImageHelper.GetListingImagePath(item.ListingPictures.OrderBy(x => x.Ordering).FirstOrDefault().PictureID)
                });
            }

            // include reviews
            var reviews = await _listingReviewService
                .Query(x => x.UserTo == id)
                .Include(x => x.AspNetUserFrom)
                .SelectAsync();

            var model = new ProfileModel()
            {
                Listings = itemsModel,
                User = user,
                ListingReviews = reviews.ToList()
            };

            return View("~/Views/Listing/Profile.cshtml", model);
        }

        public async Task<ActionResult> ContactUser(ContactUserModel model)
        {
            var listing = await _listingService.FindAsync(model.ListingID);

            var userIdCurrent = User.Identity.GetUserId();
            var user = userIdCurrent.User();
            
            // Check if user send message to itself, which is not allowed
            if (listing.UserID == userIdCurrent)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You cannot send message to yourself!]]]";
                return RedirectToAction("Listing", "Listing", new { id = model.ListingID });
            }

            // Send message to user
            var message = new MessageSendModel()
            {
                UserFrom = userIdCurrent,
                UserTo = listing.UserID,
                Subject = listing.Title,
                Body = model.Message,
                ListingID = listing.ID
            };

            await MessageHelper.SendMessage(message);

            // Send email with notification
            var emailTemplateQuery = await _emailTemplateService.Query(x => x.Slug.ToLower() == "privatemessage").SelectAsync();
            var emailTemplate = emailTemplateQuery.Single();

            dynamic email = new Postal.Email("Email");
            email.To = user.Email;
            email.From = CacheHelper.Settings.EmailAddress;
            email.Subject = emailTemplate.Subject;
            email.Body = emailTemplate.Body;
            email.Message = model.Message;
            EmailHelper.SendEmail(email);

            TempData[TempDataKeys.UserMessage] = "[[[Message sent succesfully!]]]";

            return RedirectToAction("Listing", "Listing", new { id = model.ListingID });
        }

        public async Task<bool> NotMeListing(int id)
        {
            var userId = User.Identity.GetUserId();
            var item = await _listingService.FindAsync(id);
            return item.UserID != userId;
        }

        /// <summary>
        /// review for an order on a listing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Review(int id)
        {
            var currentUserId = User.Identity.GetUserId();

            // check if user has right to review the order            
            var reviewModel = new ListingReview();

            var orderQuery = await _orderService
                .Query(x => x.ID == id)
                .Include(x => x.Listing)
                .Include(x => x.Listing.AspNetUser)
                .Include(x => x.Listing.ListingType)
                .Include(x => x.Listing.ListingReviews)
                .Include(x => x.AspNetUserProvider)
                .Include(x => x.AspNetUserReceiver)
                .SelectAsync();

            var order = orderQuery.FirstOrDefault();

            reviewModel.Listing = order.Listing;
            reviewModel.OrderID = order.ID;
            reviewModel.ListingID = order.ListingID;

            // set user for review
            reviewModel.AspNetUserTo = currentUserId == order.UserProvider ? order.AspNetUserReceiver : order.AspNetUserProvider;

            return View(reviewModel);
        }

        /// <summary>
        /// Submit review
        /// </summary>
        /// <param name="listingReview"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Review(ListingReview listingReview)
        {
            var currentUserId = User.Identity.GetUserId();

            var orderQuery = await _orderService.Query(x => x.ID == listingReview.OrderID)
                .Include(x => x.Listing)
                .SelectAsync();

            var order = orderQuery.FirstOrDefault();

            var userTo = order.UserProvider == currentUserId ? order.UserReceiver : order.UserProvider;

            // User cannot comment himself
            if (currentUserId == userTo)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You cannot review yourself!]]]";
                return RedirectToAction("Orders", "Payment");
            }

            // check if user has right to review the order
            if (order == null || (order.UserProvider != currentUserId && order.UserReceiver != currentUserId))
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You cannot review the order!]]]";
                return RedirectToAction("Orders", "Payment");
            }

            // update review id on the order
            var review = new ListingReview()
            {
                UserFrom = currentUserId,
                UserTo = userTo,
                OrderID = listingReview.OrderID,
                Description = listingReview.Description,
                Rating = listingReview.Rating,
                Spam = false,
                Active = true,
                Enabled = true,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added,
                Created = DateTime.Now
            };

            // Set listing id if it's service receiver
            if (order.UserReceiver == currentUserId)
                review.ListingID = order.ListingID;

            _listingReviewService.Insert(review);

            await _unitOfWorkAsync.SaveChangesAsync();

            // update rating on the user            
            var listingReviewQuery = await _listingReviewService.Query(x => x.UserTo == userTo).SelectAsync();
            var rating = listingReviewQuery.Average(x => x.Rating);

            var user = await UserManager.FindByIdAsync(userTo);
            user.Rating = rating;
            await UserManager.UpdateAsync(user);

            // Notify the user with the rating and comment
            var message = new MessageSendModel()
            {
                UserFrom = review.UserFrom,
                UserTo = review.UserTo,
                Subject = review.Title,
                Body = string.Format("{0} <span class=\"score s{1} text-xs\"></span>", review.Description, review.RatingClass),
                ListingID = order.ListingID
            };

            await MessageHelper.SendMessage(message);

            TempData[TempDataKeys.UserMessage] = "[[[Thanks for your feedback!]]]";
            return RedirectToAction("Orders", "Payment");
        }

        /// <summary>
        /// Submit review by listing id
        /// </summary>
        /// <param name="listingReview"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ReviewListing(ListingReview listingReview)
        {
            var currentUserId = User.Identity.GetUserId();

            // Check if listing review is enabled
            if (!CacheHelper.Settings.ListingReviewEnabled)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[Listing review is not allowed!]]]";
                return RedirectToAction("Listing", "Listing", new { id = listingReview.ID });
            }

            // Check if users reach max review limit       
            var today = DateTime.Today.Date;
            var reviewQuery = await _listingReviewService.Query(x => x.UserFrom == currentUserId
            && System.Data.Entity.DbFunctions.TruncateTime(x.Created) == today).SelectAsync();
            var reviewCount = reviewQuery.Count();

            if (reviewCount >= CacheHelper.Settings.ListingReviewMaxPerDay)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You have reach the review limits today!]]]";
                return RedirectToAction("Listing", "Listing", new { id = listingReview.ID });
            }

            var listingQuery = await _listingService.Query(x => x.ID == listingReview.ID)
                .Include(x => x.AspNetUser)
                .SelectAsync();

            var listing = listingQuery.FirstOrDefault();

            // User cannot comment himself
            if (currentUserId == listing.UserID)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You cannot review yourself!]]]";
                return RedirectToAction("Listing", "Listing", new { id = listingReview.ID });
            }

            // update review id on the order
            var review = new ListingReview()
            {
                UserFrom = currentUserId,
                UserTo = listing.UserID,
                Description = listingReview.Description,
                Rating = listingReview.Rating,
                Spam = false,
                Active = true,
                Enabled = true,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added,
                Created = DateTime.Now
            };

            review.ListingID = listingReview.ID;

            _listingReviewService.Insert(review);

            await _unitOfWorkAsync.SaveChangesAsync();

            // update rating on the user            
            var listingReviewQuery = await _listingReviewService.Query(x => x.UserTo == listing.UserID).SelectAsync();
            var rating = listingReviewQuery.Average(x => x.Rating);

            var user = await UserManager.FindByIdAsync(listing.UserID);
            user.Rating = rating;
            await UserManager.UpdateAsync(user);

            // Notify the user with the rating and comment
            var message = new MessageSendModel()
            {
                UserFrom = review.UserFrom,
                UserTo = review.UserTo,
                Subject = review.Title,
                Body = string.Format("{0} <span class=\"score s{1} text-xs\"></span>", review.Description, review.RatingClass),
                ListingID = listingReview.ID
            };

            await MessageHelper.SendMessage(message);

            TempData[TempDataKeys.UserMessage] = "[[[Thanks for your feedback!]]]";
            return RedirectToAction("Listing", "Listing", new { id = listingReview.ID });
        }
        #endregion
    }
}