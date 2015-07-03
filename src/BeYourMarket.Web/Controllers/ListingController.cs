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
        private readonly IItemService _itemService;
        private readonly IItemStatService _itemStatService;
        private readonly IItemPictureService _itemPictureService;
        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;
        private readonly ICustomFieldItemService _customFieldItemService;

        private readonly IEmailTemplateService _emailTemplateService;

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
           IItemService itemService,
           IPictureService pictureService,
           IItemPictureService itemPictureService,
           IOrderService orderService,
           ICustomFieldService customFieldService,
           ICustomFieldCategoryService customFieldCategoryService,
           ICustomFieldItemService customFieldItemService,
           ISettingDictionaryService settingDictionaryService,
           IItemStatService itemStatService,
           IEmailTemplateService emailTemplateService,
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
            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldItemService = customFieldItemService;
            _itemStatService = itemStatService;
            _emailTemplateService = emailTemplateService;
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
            var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == categoryID).Include(x => x.MetaField.ItemMetas).SelectAsync();
            var customFieldCategories = customFieldCategoryQuery.ToList();
            var customFieldModel = new CustomFieldItemModel()
            {
                MetaCategories = customFieldCategories
            };

            return PartialView("_CategoryCustomFields", customFieldModel);
        }

        public async Task<ActionResult> ListingUpdate(int? id)
        {
            if (CacheHelper.Categories.Count == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[There are not categories available yet.]]]";
            }   

            Item item;

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

                item = await _itemService.FindAsync(id);

                if (item == null)
                    return new HttpNotFoundResult();

                // Pictures
                var pictures = await _itemPictureService.Query(x => x.ItemID == id).SelectAsync();

                var picturesModel = pictures.Select(x =>
                    new PictureModel()
                    {
                        ID = x.PictureID,
                        Url = ImageHelper.GetItemImagePath(x.PictureID),
                        ItemID = x.ItemID,
                        Ordering = x.Ordering
                    }).OrderBy(x => x.Ordering).ToList();

                model.Pictures = picturesModel;
            }
            else
            {
                item = new Item()
                {
                    CategoryID = CacheHelper.Categories.Any() ? CacheHelper.Categories.FirstOrDefault().ID : 0,
                    Created = DateTime.Now.Date,
                    LastUpdated = DateTime.Now.Date,
                    Expiration = DateTime.MaxValue,
                    Enabled = true,
                    Active = true,
                    ContactEmail = user.Email,
                    ContactName = string.Format("{0} {1}", user.FirstName, user.LastName),
                    ContactPhone = user.PhoneNumber
                };
            }

            // Item
            model.ListingItem = item;

            // Custom fields
            var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == item.CategoryID).Include(x => x.MetaField.ItemMetas).SelectAsync();
            var customFieldCategories = customFieldCategoryQuery.ToList();
            var customFieldModel = new CustomFieldItemModel()
            {
                ItemID = item.ID,
                MetaCategories = customFieldCategories
            };

            model.CustomFields = customFieldModel;
            model.UserID = item.UserID;
            model.CategoryID = item.CategoryID;

            return View("~/Views/Listing/ListingUpdate.cshtml", model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Listing(int id)
        {
            var itemQuery = await _itemService.Query(x => x.ID == id)
                .Include(x => x.Category).Include(x => x.ItemMetas).Include(x => x.ItemMetas.Select(y => y.MetaField)).Include(x => x.ItemStats).SelectAsync();

            var item = itemQuery.FirstOrDefault();

            if (item == null)
                return new HttpNotFoundResult();

            var orders = _orderService.Queryable()
                .Where(x => x.ItemID == id
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

            var pictures = await _itemPictureService.Query(x => x.ItemID == id).SelectAsync();

            var picturesModel = pictures.Select(x =>
                new PictureModel()
                {
                    ID = x.PictureID,
                    Url = ImageHelper.GetItemImagePath(x.PictureID),
                    ItemID = x.ItemID,
                    Ordering = x.Ordering
                }).OrderBy(x => x.Ordering).ToList();

            var user = await UserManager.FindByIdAsync(item.UserID);

            var itemModel = new ItemModel()
            {
                ItemCurrent = item,
                Pictures = picturesModel,
                DatesBooked = datesBooked,
                User = user,
                // allow only booking if there is a price and payment methods
                BookingAllowed = CacheHelper.Settings.BookingEnabled && item.Price.HasValue && item.Active && item.Enabled
            };

            // Update stat count
            var itemStat = item.ItemStats.FirstOrDefault();
            if (itemStat == null)
            {
                _itemStatService.Insert(new ItemStat()
                {
                    ItemID = id,
                    CountView = 1,
                    Created = DateTime.Now,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });
            }
            else
            {
                itemStat.CountView++;
                itemStat.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
                _itemStatService.Update(itemStat);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            return View("~/Views/Listing/Listing.cshtml", itemModel);
        }

        [HttpPost]
        public async Task<ActionResult> ListingUpdate(Item item, FormCollection form, IEnumerable<HttpPostedFileBase> files)
        {
            if (CacheHelper.Categories.Count == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[There are not categories available yet.]]]";

                return RedirectToAction("Listing", new { id = item.ID });
            }

            bool updateCount = false;

            int nextPictureOrderId = 0;

            if (item.ID == 0)
            {
                item.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                item.IP = Request.GetVisitorIP();
                item.Expiration = DateTime.MaxValue.AddDays(-1);
                item.UserID = User.Identity.GetUserId();
                item.Enabled = true;

                updateCount = true;
                _itemService.Insert(item);
            }
            else
            {
                if (await NotMeListing(item.ID))
                    return new HttpUnauthorizedResult();

                var itemExisting = await _itemService.FindAsync(item.ID);

                itemExisting.Title = item.Title;
                itemExisting.Description = item.Description;
                itemExisting.Active = item.Active;

                itemExisting.ContactEmail = item.ContactEmail;
                itemExisting.ContactName = item.ContactName;
                itemExisting.ContactPhone = item.ContactPhone;

                itemExisting.Latitude = item.Latitude;
                itemExisting.Longitude = item.Longitude;
                itemExisting.Location = item.Location;

                itemExisting.ShowPhone = item.ShowPhone;
                itemExisting.ShowEmail = item.ShowEmail;

                itemExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _itemService.Update(itemExisting);
            }

            // Delete existing fields on item
            var customFieldItemQuery = await _customFieldItemService.Query(x => x.ItemID == item.ID).SelectAsync();
            var customFieldIds = customFieldItemQuery.Select(x => x.ID).ToList();
            foreach (var customFieldId in customFieldIds)
            {
                await _customFieldItemService.DeleteAsync(customFieldId);
            }

            // Get custom fields
            var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == item.CategoryID).Include(x => x.MetaField.ItemMetas).SelectAsync();
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

                var itemMeta = new ItemMeta()
                {
                    ItemID = item.ID,
                    Value = formValue,
                    FieldID = field.ID,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                };

                _customFieldItemService.Insert(itemMeta);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            if (Request.Files.Count > 0)
            {
                var itemPictureQuery = _itemPictureService.Queryable().Where(x => x.ItemID == item.ID);
                if (itemPictureQuery.Count() > 0)
                    nextPictureOrderId = itemPictureQuery.Max(x => x.Ordering);
            }

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
                        var path = Path.Combine(Server.MapPath("~/images/item"), string.Format("{0}.{1}", picture.ID.ToString("00000000"), "jpg"));

                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(file.InputStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Save(path);
                    }

                    var itemPicture = new ItemPicture();
                    itemPicture.ItemID = item.ID;
                    itemPicture.PictureID = picture.ID;
                    itemPicture.Ordering = nextPictureOrderId;

                    _itemPictureService.Insert(itemPicture);

                    nextPictureOrderId++;
                }
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            // Update statistics count
            if (updateCount)
            {
                _sqlDbService.UpdateCategoryItemCount(item.CategoryID);
                _dataCacheService.RemoveCachedItem(CacheKeys.Statistics);
            }

            return RedirectToAction("Listing", new { id = item.ID });
        }

        [HttpPost]
        public async Task<ActionResult> ListingDelete(int id)
        {
            var item = await _itemService.FindAsync(id);
            var orderQuery = await _orderService.Query(x => x.ItemID == id).SelectAsync();

            // Delete item if no orders associated with it
            if (item.Orders.Count > 0)
            {
                var resultFailed = new { Success = false, Message = "You cannot delete item with orders! You can deactivate it instead." };
                return Json(resultFailed, JsonRequestBehavior.AllowGet);
            }            

            // Delete pictures
            var pictureIds = _itemPictureService.Query(x => x.ItemID == id).Select(x => x.ItemID).ToList();
            foreach (var pictureId in pictureIds)
            {
                await _itemPictureService.DeleteAsync(pictureId);
            }                        

            await _itemService.DeleteAsync(id);

            await _unitOfWorkAsync.SaveChangesAsync();

            var result = new { Success = true, Message = "Your listing has been deleted." };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ListingPhotoDelete(int id)
        {
            try
            {
                await _pictureService.DeleteAsync(id);
                var itemPicture = _itemPictureService.Query(x => x.PictureID == id).Select().FirstOrDefault();

                if (itemPicture != null)
                    await _itemPictureService.DeleteAsync(itemPicture.ID);

                await _unitOfWorkAsync.SaveChangesAsync();

                var path = Path.Combine(Server.MapPath("~/images/item"), string.Format("{0}.{1}", id.ToString("00000000"), "jpg"));

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

            var items = await _itemService.Query(x => x.UserID == id).Include(x => x.ItemPictures).SelectAsync();

            var itemsModel = new List<ItemModel>();
            foreach (var item in items.OrderByDescending(x => x.Created))
            {
                itemsModel.Add(new ItemModel()
                {
                    ItemCurrent = item,
                    UrlPicture = item.ItemPictures.Count == 0 ? ImageHelper.GetItemImagePath(0) : ImageHelper.GetItemImagePath(item.ItemPictures.OrderBy(x => x.Ordering).FirstOrDefault().PictureID)
                });
            }

            var model = new ProfileModel()
            {
                Items = itemsModel,
                User = user
            };

            return View("~/Views/Listing/Profile.cshtml", model);
        }

        public async Task<ActionResult> ContactUser(ContactUserModel model)
        {
            var emailTemplateQuery = await _emailTemplateService.Query(x => x.Slug.ToLower() == "privatemessage").SelectAsync();
            var emailTemplate = emailTemplateQuery.Single();

            dynamic email = new Postal.Email("Email");
            email.To = CacheHelper.Settings.EmailContact;
            email.From = CacheHelper.Settings.EmailContact;
            email.Subject = emailTemplate.Subject;
            email.Body = emailTemplate.Body;
            email.Message = model.Message;
            EmailHelper.SendEmail(email);

            TempData[TempDataKeys.UserMessage] = "[[[Message sent succesfully!]]]";

            return RedirectToAction("Listing", "Listing", new { id = model.ItemID });
        }

        public async Task<bool> NotMeListing(int id)
        {
            var userId = User.Identity.GetUserId();
            var item = await _itemService.FindAsync(id);
            return item.UserID != userId;
        }
        #endregion
    }
}