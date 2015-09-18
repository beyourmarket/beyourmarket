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
using BeYourMarket.Core.Web;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ListingController : Controller
    {
        #region Fields
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        private readonly ISettingService _settingService;
        private readonly ISettingDictionaryService _settingDictionaryService;

        private readonly ICategoryService _categoryService;
        private readonly ICategoryListingTypeService _categoryListingTypeService;

        private readonly IListingService _listingService;
        private readonly IListingTypeService _listingTypeService;
        private readonly IListingReviewService _listingReviewService;

        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldCategoryService _customFieldCategoryService;
        private readonly ICustomFieldListingService _customFieldListingService;

        private readonly IContentPageService _contentPageService;

        private readonly IOrderService _orderService;

        private readonly IListingPictureService _listingPictureservice;
        private readonly IPictureService _pictureService;

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
        public ListingController(
            IUnitOfWorkAsync unitOfWorkAsync,
            ISettingService settingService,
            ICategoryService categoryService,
            ICategoryListingTypeService categoryListingTypeService,
            IListingService listingService,
            IListingTypeService listingTypeService,
            ICustomFieldService customFieldService,
            ICustomFieldCategoryService customFieldCategoryService,
            ICustomFieldListingService customFieldListingService,
            IContentPageService contentPageService,
            IOrderService orderService,
            ISettingDictionaryService settingDictionaryService,
            IEmailTemplateService emailTemplateService,
            IPictureService pictureService,
            IListingPictureService listingPictureservice,
            IListingReviewService listingReviewService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _categoryListingTypeService = categoryListingTypeService;

            _listingService = listingService;
            _listingTypeService = listingTypeService;

            _pictureService = pictureService;
            
            _listingPictureservice = listingPictureservice;
            _listingReviewService = listingReviewService;

            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldListingService = customFieldListingService;

            _orderService = orderService;

            _emailTemplateService = emailTemplateService;
            _contentPageService = contentPageService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;
        }
        #endregion

        #region Methods
        public ActionResult Categories()
        {
            var categories = _categoryService.Queryable().OrderBy(x => x.Parent).ThenBy(x => x.Ordering).ToList().GenerateTree(x => x.ID, x => x.Parent);

            return View(categories);
        }


        public class CategoryJson
        {
            public int id { get; set; }

            public CategoryJson[] children { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> CategoriesUpdate(string JsonCategories)
        {
            var result = JsonConvert.DeserializeObject<CategoryJson[]>(JsonCategories);

            await SetCategoryOrdering(0, result);

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.RemoveCachedItem(CacheKeys.ListingTypes);
            _dataCacheService.RemoveCachedItem(CacheKeys.Categories);

            return RedirectToAction("Categories");
        }

        /// <summary>
        /// Recursive method to set category ordering
        /// </summary>
        /// <param name="json"></param>
        public async Task SetCategoryOrdering(int parent, CategoryJson[] json)
        {
            int sortOrder = 0;
            foreach (var item in json)
            {
                var category = await _categoryService.FindAsync(item.id);
                category.Parent = parent;
                category.Ordering = sortOrder;
                category.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _categoryService.Update(category);

                sortOrder++;

                if (item.children != null)
                {
                    await SetCategoryOrdering(item.id, item.children);
                }
            }
        }

        public async Task<ActionResult> CategoryUpdate(int? id)
        {
            Category category;

            var categoryModel = new CategoryModel();

            var ListingTypes = await _listingTypeService.Query().SelectAsync();
            categoryModel.ListingTypes = ListingTypes.ToList();

            if (id.HasValue)
            {
                category = await _categoryService.FindAsync(id);
                var categoryListingTypes = await _categoryListingTypeService.Query(x => x.CategoryID == id.Value).SelectAsync();
                categoryModel.CategoryListingTypeID = categoryListingTypes.Select(x => x.ListingTypeID).ToList();
            }
            else
                category = new Category() { Enabled = true };

            categoryModel.Category = category;

            return View(categoryModel);
        }

        [HttpPost]
        public async Task<ActionResult> CategoryUpdate(CategoryModel model)
        {
            if (model.Category.ID == 0)
            {
                model.Category.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                model.Category.Parent = 0;

                _categoryService.Insert(model.Category);
            }
            else
            {
                var categoryExisting = await _categoryService.FindAsync(model.Category.ID);

                categoryExisting.Name = model.Category.Name;
                categoryExisting.Description = model.Category.Description;
                categoryExisting.Enabled = model.Category.Enabled;

                categoryExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _categoryService.Update(categoryExisting);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            // Delete existing category item types
            var ListingTypes = _categoryListingTypeService.Queryable().Where(x => x.CategoryID == model.Category.ID).Select(x => x.ID).ToList();
            foreach (var ListingTypeID in ListingTypes)
            {
                await _categoryListingTypeService.DeleteAsync(ListingTypeID);
            }

            // Insert category item types
            foreach (var categoryListingTypeID in model.CategoryListingTypeID)
            {
                _categoryListingTypeService.Insert(new CategoryListingType()
                {
                    CategoryID = model.Category.ID,
                    ListingTypeID = categoryListingTypeID,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.RemoveCachedItem(CacheKeys.ListingTypes);
            _dataCacheService.RemoveCachedItem(CacheKeys.Categories);

            return RedirectToAction("Categories");
        }

        public ActionResult CustomFields()
        {
            var grid = new CustomFieldsGrid(_customFieldService.Query().Include(x => x.MetaCategories.Select(s => s.Category)).Select().OrderByDescending(x => x.ID).AsQueryable());

            return View(grid);
        }

        public async Task<ActionResult> CustomFieldUpdate(int? id)
        {
            MetaField field;

            if (id.HasValue)
            {
                var fieldQuery = await _customFieldService.Query(x => x.ID == id).Include(x => x.MetaCategories).SelectAsync();
                field = fieldQuery.FirstOrDefault();
            }
            else
                field = new MetaField()
                {
                    Required = false,
                    Searchable = false
                };

            var categories = await _categoryService.Query().SelectAsync();

            var model = new MetaFieldModel()
            {
                MetaField = field,
                Categories = categories.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CustomFieldUpdate(MetaField metaField, List<int> Categories)
        {
            if (metaField.ID == 0)
            {
                metaField.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;

                _customFieldService.Insert(metaField);
            }
            else
            {
                var metaFieldExistingQuery = await _customFieldService.Query(x => x.ID == metaField.ID).Include(x => x.MetaCategories).SelectAsync();
                var metaFieldExisting = metaFieldExistingQuery.FirstOrDefault();

                metaFieldExisting.Name = metaField.Name;
                metaFieldExisting.ControlTypeID = metaField.ControlTypeID;
                metaFieldExisting.Options = metaField.Options;
                metaFieldExisting.Required = metaField.Required;
                metaFieldExisting.Searchable = metaField.Searchable;

                metaFieldExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _customFieldService.Update(metaFieldExisting);

                // Delete existing
                foreach (var category in metaFieldExisting.MetaCategories)
                {
                    await _customFieldCategoryService.DeleteAsync(category.ID);
                }
            }

            if (Categories != null)
            {
                // Insert meta categories
                var metaCategories = Categories.Select(x => new MetaCategory()
                {
                    CategoryID = x,
                    FieldID = metaField.ID,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                }).ToList();

                _customFieldCategoryService.InsertRange(metaCategories);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            return RedirectToAction("CustomFields");
        }

        public ActionResult Listings()
        {
            var grid = new ListingsGrid(_listingService.Query().Include(x => x.ListingType).Select().OrderByDescending(x => x.Created).AsQueryable());
            var categories = CacheHelper.Categories;

            var model = new ListingModel()
            {
                Categories = categories,
                Grid = grid
            };

            return View(model);
        }

        public async Task<ActionResult> ListingUpdate(int? id)
        {
            if (CacheHelper.Categories.Count == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[There are not categories available yet.]]]";
            }

            Listing item;

            var model = new ListingUpdateModel()
            {
                Categories = CacheHelper.Categories
            };

            if (id.HasValue)
            {
                item = await _listingService.FindAsync(id);

                if (item == null)
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
                item = new Listing()
                {
                    CategoryID = CacheHelper.Categories.Any() ? CacheHelper.Categories.FirstOrDefault().ID : 0,
                    Created = DateTime.Now.Date,
                    LastUpdated = DateTime.Now.Date,
                    Expiration = DateTime.Now.AddDays(30),
                    Enabled = true,
                    Active = true,
                };

            // Item
            model.ListingItem = item;

            // Custom fields
            var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == item.CategoryID).Include(x => x.MetaField.ListingMetas).SelectAsync();
            var customFieldCategories = customFieldCategoryQuery.ToList();
            var customFieldModel = new CustomFieldListingModel()
            {
                ListingID = item.ID,
                MetaCategories = customFieldCategories
            };

            model.CustomFields = customFieldModel;
            model.Users = UserManager.Users.ToList();
            model.UserID = item.UserID;
            model.CategoryID = item.CategoryID;
            model.ListingTypeID = item.ListingTypeID;

            // Listing types
            model.ListingTypes = CacheHelper.ListingTypes.Where(x => x.CategoryListingTypes.Any(y => y.CategoryID == model.CategoryID)).ToList();

            return View(model);
        }

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

        [HttpPost]
        public async Task<ActionResult> ListingUpdate(Listing listing, FormCollection form, IEnumerable<HttpPostedFileBase> files)
        {
            if (CacheHelper.Categories.Count == 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[There are not categories available yet.]]]";

                return RedirectToAction("Listing", new { id = listing.ID });
            }

            bool updateCount = false;

            int nextPictureOrderId = 0;

            // Add new listing
            if (listing.ID == 0)
            {
                listing.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                listing.IP = Request.GetVisitorIP();
                listing.Expiration = DateTime.MaxValue.AddDays(-1);
                listing.UserID = User.Identity.GetUserId();

                updateCount = true;
                _listingService.Insert(listing);
            }
            else
            {
                // Update listing
                var listingExisting = await _listingService.FindAsync(listing.ID);

                listingExisting.Title = listing.Title;
                listingExisting.Description = listing.Description;
                listingExisting.CategoryID = listing.CategoryID;

                listingExisting.Enabled = listing.Enabled;
                listingExisting.Active = listing.Active;
                listingExisting.Premium = listing.Premium;

                listingExisting.ContactEmail = listing.ContactEmail;
                listingExisting.ContactName = listing.ContactName;
                listingExisting.ContactPhone = listing.ContactPhone;

                listingExisting.Latitude = listing.Latitude;
                listingExisting.Longitude = listing.Longitude;
                listingExisting.Location = listing.Location;

                listingExisting.ShowPhone = listing.ShowPhone;
                listingExisting.ShowEmail = listing.ShowEmail;

                listingExisting.UserID = listing.UserID;

                listingExisting.Price = listing.Price;
                listingExisting.Currency = listing.Currency;

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

            // Update custom fields
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

            // Update photos
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

            return RedirectToAction("Listings");
        }

        [HttpPost]
        public async Task<ActionResult> ListingDelete(int id)
        {
            var item = await _listingService.FindAsync(id);
            var orderQuery = await _orderService.Query(x => x.ListingID == id).SelectAsync();

            // Delete item if no orders associated with it
            if (item.Orders.Count > 0)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You cannot delete item with orders! You can deactivate it instead.]]]";

                return RedirectToAction("Listings");
            }

            await _listingService.DeleteAsync(id);

            await _unitOfWorkAsync.SaveChangesAsync();

            TempData[TempDataKeys.UserMessage] = "[[[The listing has been deleted.]]]";

            return RedirectToAction("Listings");
        }

        [HttpPost]
        public async Task<ActionResult> ListingTypeDelete(int id)
        {
            if (CacheHelper.ListingTypes.Count <= 1)
            {
                TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
                TempData[TempDataKeys.UserMessage] = "[[[You cannot the listing type. There should be at least one listing type.]]]";

                return RedirectToAction("ListingTypes");
            }

            var item = await _listingTypeService.FindAsync(id);                       

            await _listingTypeService.DeleteAsync(id);

            await _unitOfWorkAsync.SaveChangesAsync();

            TempData[TempDataKeys.UserMessage] = "[[[The listing type has been deleted.]]]";

            _dataCacheService.RemoveCachedItem(CacheKeys.ListingTypes);

            return RedirectToAction("ListingTypes");
        }

        [HttpPost]
        public async Task<ActionResult> CategoryDelete(int id)
        {
            var item = await _categoryService.FindAsync(id);

            await _categoryService.DeleteAsync(id);

            await _unitOfWorkAsync.SaveChangesAsync();

            TempData[TempDataKeys.UserMessage] = "[[[The category has been deleted.]]]";

            _dataCacheService.RemoveCachedItem(CacheKeys.Categories);

            return RedirectToAction("Categories");
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

        public ActionResult ListingTypes()
        {
            var grid = new ListingTypesGrid(_listingTypeService.Queryable().OrderBy(x => x.ID));


            var model = new ListingTypeModel()
            {
                Grid = grid
            };

            return View(model);
        }

        public async Task<ActionResult> ListingTypeUpdate(int? id)
        {
            ListingType ListingType = new ListingType();

            if (id.HasValue)
            {
                ListingType = await _listingTypeService.FindAsync(id);

                if (ListingType == null)
                    return new HttpNotFoundResult();
            }

            return View(ListingType);
        }

        [HttpPost]
        public async Task<ActionResult> ListingTypeUpdate(ListingType model)
        {
            var isNew = false;

            if (model.ID == 0)
            {
                model.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;

                isNew = true;
                _listingTypeService.Insert(model);
            }
            else
            {
                var listingTypeExisting = await _listingTypeService.FindAsync(model.ID);

                listingTypeExisting.Name = model.Name;
                listingTypeExisting.ButtonLabel = model.ButtonLabel;
                listingTypeExisting.PriceUnitLabel = model.PriceUnitLabel;
                listingTypeExisting.ShippingEnabled = model.ShippingEnabled;

                listingTypeExisting.PriceEnabled = model.PriceEnabled;
                listingTypeExisting.PaymentEnabled = model.PaymentEnabled;
                if (model.PaymentEnabled)
                {
                    listingTypeExisting.OrderTypeID = model.OrderTypeID;
                    listingTypeExisting.OrderTypeLabel = model.OrderTypeLabel;
                }

                listingTypeExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _listingTypeService.Update(listingTypeExisting);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

            // Add item type to each category
            if (isNew)
            {
                foreach (var category in CacheHelper.Categories)
                {
                    _categoryListingTypeService.Insert(new CategoryListingType()
                    {
                        CategoryID = category.ID,
                        ListingTypeID = model.ID,
                        ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                    });
                }
                await _unitOfWorkAsync.SaveChangesAsync();
            }

            _dataCacheService.RemoveCachedItem(CacheKeys.ListingTypes);

            return RedirectToAction("ListingTypes");
        }
        #endregion
    }
}