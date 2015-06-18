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
    public class ListingController : Controller
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
        private readonly ICustomFieldItemService _customFieldItemService;

        private readonly IContentPageService _contentPageService;

        private readonly IOrderService _orderService;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IStripeConnectService _stripConnectService;

        private readonly IItemPictureService _itemPictureService;
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
            IItemService itemService,
            ICustomFieldService customFieldService,
            ICustomFieldCategoryService customFieldCategoryService,
            ICustomFieldItemService customFieldItemService,
            IContentPageService contentPageService,
            IOrderService orderService,
            IOrderTransactionService orderTransationService,
            IStripeConnectService stripConnectService,
            ISettingDictionaryService settingDictionaryService,
            IEmailTemplateService emailTemplateService,
            IPictureService pictureService,
           IItemPictureService itemPictureService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _itemService = itemService;
            _pictureService = pictureService;
            _itemPictureService = itemPictureService;

            _customFieldService = customFieldService;
            _customFieldCategoryService = customFieldCategoryService;
            _customFieldItemService = customFieldItemService;

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

            if (id.HasValue)
                category = await _categoryService.FindAsync(id);
            else
                category = new Category() { Enabled = true };

            return View(category);
        }

        [HttpPost]
        public async Task<ActionResult> CategoryUpdate(Category category)
        {
            if (category.ID == 0)
            {
                category.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                category.Parent = 0;

                _categoryService.Insert(category);
            }
            else
            {
                var categoryExisting = await _categoryService.FindAsync(category.ID);

                categoryExisting.Name = category.Name;
                categoryExisting.Description = category.Description;
                categoryExisting.Enabled = category.Enabled;

                categoryExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

                _categoryService.Update(categoryExisting);
            }

            await _unitOfWorkAsync.SaveChangesAsync();

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
            var grid = new ListingsGrid(_itemService.Queryable().OrderByDescending(x => x.Created));
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
            Item item;

            var model = new ListingUpdateModel()
            {
                Categories = CacheHelper.Categories
            };

            if (id.HasValue)
            {
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
                item = new Item()
                {
                    Created = DateTime.Now.Date,
                    LastUpdated = DateTime.Now.Date,
                    Expiration = DateTime.Now.AddDays(30),
                    Enabled = true,                    
                };

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
            model.Users = UserManager.Users.ToList();
            model.UserID = item.UserID;
            model.CategoryID = item.CategoryID;

            return View(model);
        }

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

        [HttpPost]
        public async Task<ActionResult> ListingUpdate(Item item, FormCollection form, IEnumerable<HttpPostedFileBase> files)
        {
            bool updateCount = false;

            int nextPictureOrderId = 0;

            if (item.ID == 0)
            {
                item.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                item.IP = Request.GetVisitorIP();
                item.Expiration = DateTime.MaxValue.AddDays(-1);
                item.UserID = User.Identity.GetUserId();

                updateCount = true;
                _itemService.Insert(item);
            }
            else
            {
                var itemExisting = await _itemService.FindAsync(item.ID);

                itemExisting.Title = item.Title;
                itemExisting.Description = item.Description;
                itemExisting.CategoryID = item.CategoryID;

                itemExisting.Enabled = item.Enabled;
                itemExisting.Active = item.Active;
                itemExisting.Premium = item.Premium;

                itemExisting.ContactEmail = item.ContactEmail;
                itemExisting.ContactName = item.ContactName;
                itemExisting.ContactPhone = item.ContactPhone;

                itemExisting.Latitude = item.Latitude;
                itemExisting.Longitude = item.Longitude;
                itemExisting.Location = item.Location;

                itemExisting.ShowPhone = item.ShowPhone;
                itemExisting.ShowEmail = item.ShowEmail;

                itemExisting.UserID = item.UserID;

                itemExisting.Price = item.Price;
                itemExisting.Currency = item.Currency;

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

            return RedirectToAction("Listings");
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
        #endregion
    }
}