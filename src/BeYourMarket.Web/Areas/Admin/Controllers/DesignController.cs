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

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DesignController : Controller
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
        public DesignController(
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
            SqlDbService sqlDbService)
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
        }
        #endregion

        #region Methods
        public ActionResult CodeStyle()
        {
            var path = Server.MapPath("~/content/custom.css");

            var code = System.IO.File.ReadAllText(path);

            var model = new TextFileModel()
            {
                Text = code
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult CodeStyleUpdate(TextFileModel model)
        {
            var path = Server.MapPath("~/content/custom.css");

            var text = string.IsNullOrEmpty(model.Text) ? string.Empty : model.Text.Trim();
            System.IO.File.WriteAllText(path, text);

            return RedirectToAction("CodeStyle");
        }

        public ActionResult CodeScript()
        {
            var path = Server.MapPath("~/scripts/custom.js");

            var code = System.IO.File.ReadAllText(path);

            var model = new TextFileModel()
            {
                Text = code
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult CodeScriptUpdate(TextFileModel model)
        {
            var path = Server.MapPath("~/scripts/custom.js");

            var text = string.IsNullOrEmpty(model.Text) ? string.Empty : model.Text.Trim();
            System.IO.File.WriteAllText(path, text);

            return RedirectToAction("CodeScript");
        }

        public ActionResult Appearance()
        {
            var setting = _settingService.Queryable().FirstOrDefault();

            var model = new AppearanceModel()
            {
                ID = setting.ID,
                CoverPhotoUrl = ImageHelper.GetCommunityImagePath("cover"),
                FaviconUrl = ImageHelper.GetCommunityImagePath("favicon"),
                LogoUrl = ImageHelper.GetCommunityImagePath("logo", "png")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AppearanceUpdate(AppearanceModel model)
        {
            var settingExisting = _settingService.Query(x => x.ID == model.ID).Select().FirstOrDefault();

            settingExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;

            _settingService.Update(settingExisting);

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.UpdateCache(CacheKeys.Settings, settingExisting);

            SavePicture(model.Favicon, "favicon", new Size(32, 32));
            SavePicture(model.CoverPhoto, "cover", new Size(1048, 0));
            SavePicture(model.Logo, "logo", new Size(200, 30), "png");

            return RedirectToAction("Appearance");
        }

        public void SavePicture(HttpPostedFileBase file, string name, Size size, string formatFile = "jpg")
        {
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                // Format is automatically detected though can be changed.
                ISupportedImageFormat format = new JpegFormat { Quality = 90 };

                if (formatFile == "png")
                    format = new PngFormat() { Quality = 90 };

                //https://naimhamadi.wordpress.com/2014/06/25/processing-images-in-c-easily-using-imageprocessor/
                // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                {
                    var path = Path.Combine(Server.MapPath("~/images/community"), string.Format("{0}.{1}", name, formatFile));

                    // Load, resize, set the format and quality and save an image.
                    imageFactory.Load(file.InputStream)
                                .Resize(size)
                                .Format(format)
                                .Save(path);
                }
            }
        }

        public async Task<ActionResult> PictureDelete(string key, string formatFile = "jpg")
        {
            try
            {
                var path = Path.Combine(Server.MapPath("~/images/community"), string.Format("{0}.{1}", Path.GetFileName(key), formatFile));

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