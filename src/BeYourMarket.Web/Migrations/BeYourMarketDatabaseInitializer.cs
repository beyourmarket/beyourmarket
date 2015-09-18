using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BeYourMarket.Web.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using BeYourMarket.Web.Areas.Admin.Models;
using BeYourMarket.Web.Models;
using BeYourMarket.Model.Enum;
using System.IO;
using BeYourMarket.Core.Migrations;
using System.Globalization;

namespace BeYourMarket.Web.Migrations
{
    public class BeYourMarketDatabaseInitializer : CreateAndMigrateDatabaseInitializer<BeYourMarket.Model.Models.BeYourMarketContext, ConfigurationInstall<BeYourMarket.Model.Models.BeYourMarketContext>>
    {
        #region Fields and properties
        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            }
        }

        private InstallModel _installModel;
        #endregion

        #region Constructor
        // pass user model, and database info
        public BeYourMarketDatabaseInitializer(InstallModel installModel)
            : base()
        {
            _installModel = installModel;
            InitializeDatabase(new Model.Models.BeYourMarketContext());
        }
        #endregion

        #region Methods
        protected override void Seed(BeYourMarket.Model.Models.BeYourMarketContext context)
        {
            InstallSettings(context);
            InstallEmailTemplates(context);
            InstallListingTypes(context);

            var user = CreateUser();

            if (_installModel.InstallSampleData)
            {                
                InstallCategories(context);
                InstallCategoryTypes(context);
                InstallSampleData(context, user);
                InstallPictures(context);
                InstallStripe(context);
                InstallDisqus(context);
            }
        }

        private ApplicationUser CreateUser()
        {
            var user = new ApplicationUser
            {
                UserName = _installModel.Email,
                FirstName = "Administrator",                
                Email = _installModel.Email,
                RegisterDate = DateTime.Now,
                RegisterIP = HttpContext.Current.Request.GetVisitorIP(),
                LastAccessDate = DateTime.Now,
                LastAccessIP = HttpContext.Current.Request.GetVisitorIP(),
                Rating = 4
            };

            using (var context = new ApplicationDbContext())
            {
                context.Database.Initialize(true);
                context.SaveChanges();

                var userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(context));

                // Create role/user and redirect
                var userRole = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole(Enum_UserType.Administrator.ToString());
                var roleResult = RoleManager.Create(userRole);
                var result = userManager.Create(user, _installModel.Password);
                var roleAdded = userManager.AddToRole(user.Id, Enum_UserType.Administrator.ToString());
            }

            // Copy profile image
            var pathFrom = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/images/sample/profile"), "admin.jpg");
            var pathTo = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/images/profile"), string.Format("{0}.{1}", user.Id, "jpg"));
            File.Copy(pathFrom, pathTo, true);

            return user;
        }

        private void InstallSettings(BeYourMarket.Model.Models.BeYourMarketContext context)
        {
            if (_installModel.InstallSampleData)
            {
                context.Settings.Add(new Model.Models.Setting()
                {
                    ID = 1,
                    Name = "BeYourMarket",
                    Description = "Find the beauty and spa service providers in your neighborhood!",
                    Slogan = "BeYourMarket - Spa Demo",
                    SearchPlaceHolder = "Search your Spa...",
                    EmailContact = "hello@beyourmarket.com",
                    Version = "1.0",
                    Currency = "DKK",
                    TransactionFeePercent = 1,
                    TransactionMinimumSize = 10,
                    TransactionMinimumFee = 10,
                    EmailConfirmedRequired = false,
                    Theme = "Default",
                    DateFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern,
                    TimeFormat = DateTimeFormatInfo.CurrentInfo.ShortTimePattern,
                    ListingReviewEnabled = true,
                    ListingReviewMaxPerDay = 5,
                    Created = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });

                // Copy files
                var files = new string[] { "cover.jpg", "favicon.jpg", "logo.png" };
                foreach (var file in files)
                {
                    var pathFrom = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/images/sample/community"), file);
                    var pathTo = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/images/community"), file);
                    File.Copy(pathFrom, pathTo, true);
                }
            }
            else
            {
                context.Settings.Add(new Model.Models.Setting()
                {
                    ID = 1,
                    Name = "BeYourMarket",
                    Description = "Create your own peer to peer market place in 5 minutes!",
                    Slogan = "Slogan...",
                    SearchPlaceHolder = "Search...",
                    EmailContact = "hello@beyourmarket.com",
                    Version = "1.0",
                    Currency = "DKK",
                    TransactionFeePercent = 1,
                    TransactionMinimumSize = 10,
                    TransactionMinimumFee = 10,
                    EmailConfirmedRequired = false,
                    Theme = "Default",
                    DateFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern,
                    TimeFormat = DateTimeFormatInfo.CurrentInfo.ShortTimePattern,
                    ListingReviewEnabled = true,
                    ListingReviewMaxPerDay = 5,
                    Created = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });
            }
        }

        private void InstallEmailTemplates(BeYourMarket.Model.Models.BeYourMarketContext context)
        {
            context.EmailTemplates.Add(new Model.Models.EmailTemplate()
            {
                Slug = "signup",
                Subject = "Sign up",
                Body = @"<p>Hi there,</p>
                        <h1>Welcome to {SiteName}</h1>
                        <p>Thanks for your sign up.</p>
                        <table>
	                        <tbody>
		                        <tr>
			                        <td class=""padding"">
			                        <p><a class=""btn-primary"" href=""{CallbackUrl}"">Please confirm your email by clicking this link</a></p>
			                        </td>
		                        </tr>
	                        </tbody>
                        </table>",
                SendCopy = true,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.EmailTemplates.Add(new Model.Models.EmailTemplate()
            {
                Slug = "forgotpassword",
                Subject = "Forgot Password",
                Body = @"<p>Hi there,</p>
                        <p>You can use the link below to reset your password.</p>
                        <table>
	                        <tbody>
		                        <tr>
			                        <td class=""padding"">
			                        <p><a class=""btn-primary"" href=""{CallbackUrl}"">Please reset your password by clicking this link</a></p>
			                        </td>
		                        </tr>
	                        </tbody>
                        </table>",
                SendCopy = true,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.EmailTemplates.Add(new Model.Models.EmailTemplate()
            {
                Slug = "privatemessage",
                Subject = "Private Message",
                Body = @"<p>Hi there,</p>
			            <p>You got a new message as below.</p>
			            <table>
				            <tbody>
					            <tr>
						            <td class=""padding"">
						            <h4>{Message}</h4>
						            </td>
					            </tr>
				            </tbody>
			            </table>",
                SendCopy = true,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });
        }

        private void InstallSampleData(BeYourMarket.Model.Models.BeYourMarketContext context, ApplicationUser user)
        {
            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "Preganancy Massage",
                Description = @"During an hour waiting women be allowed to experience total relaxation and relief of aches. The therapist works gently with the pregnant body to loosen up tight muscles, give peace to the nervous system, increase blood circulation and reduce pain in the body.",
                CategoryID = 1,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 100,
                Currency = "DKK",
                ContactName = "Celia",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Marievej 1, 2900 Hellerup, Danmark",
                Latitude = 55.730344,
                Longitude = 12.5767257,
                Expiration = DateTime.MaxValue.Date,
                Active = true,
                Enabled = true,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "Facial Treatment",
                Description = @"Classic 45 min Facial treat: Cleaning, skin analysis, AHA-PHA peeling, light deep cleanse...",
                CategoryID = 2,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 249,
                Currency = "DKK",
                ContactName = "The Facial Lounge",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Sankt Jørgens Allé 5, København V, Danmark",
                Latitude = 55.6735479,
                Longitude = 12.559128399999963,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "60min Moisturizing face treatment",
                Description = @"During an hour waiting women be allowed to experience total relaxation and relief of aches. The therapist works gently with the pregnant body to loosen up tight muscles, give peace to the nervous system, increase blood circulation and reduce pain in the body.",
                CategoryID = 2,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 219,
                Currency = "DKK",
                ContactName = "Clinique Margarita",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Studiestræde 18 København K",
                Latitude = 55.6786854,
                Longitude = 12.5694609,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "Eyelash extensions",
                Description = @"Give the lashes fullness and length with eyelash extensions. 50-100 fiber hair attached individually at one's own lashes for a natural look. The treatment takes about 90 minutes.",
                CategoryID = 2,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 375,
                Currency = "DKK",
                ContactName = "Beauty And Accessories",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Henrik Steffens Vej 6, 1866 Frederiksberg C, Danmark",
                Latitude = 55.6779527,
                Longitude = 12.538388800000007,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "60min Massage for 2 persons - 3 types to choose",
                Description = @"Take your partner by the arm and enjoy an hour of relaxing parma sage. Choose freely between wellness, Aromatherapy- and hotstone massage. By wellness massage using long, smooth movements of the upper layers of the muscles of mental and physical relaxation. Fragrant oils from flowers and herbs used in Aroma Therapy massage for relaxation and enjoyment. By hotstone massage used heated lava rocks to smoothen the muscles, then loosen tensions and aches.",
                CategoryID = 1,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 549,
                Currency = "DKK",
                ContactName = "Healingstedet",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Liflandsgade 8 København 2300",
                Latitude = 55.6608952,
                Longitude = 12.6031471,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "1 hour shiatsu massage",
                Description = @"Let the body come into focus with an hour shiatsu massage that combines deep pressure and long runs. The treatment allows the body to sink into a relaxed state, and it is therefore suitable for stress-related genes and other long-term imbalances. The massage takes place on a mattress on the floor, and it is important to be dressed in comfortable clothes, so the body can completely relax.",
                CategoryID = 1,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 249,
                Currency = "DKK",
                ContactName = "Zen-Shiatsu",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Havnegade 43, st. th. København K 1058",
                Latitude = 55.677783,
                Longitude = 12.591222,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "Hot Stone Massage",
                Description = @"With heated lava stones are the body's tense muscles supple and ready for a deep massage. The massage works with aches and tension, and brings blood to the muscles for a pain-relieving effect and increased flexibility.",
                CategoryID = 1,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 249,
                Currency = "DKK",
                ContactName = "Healingstedet",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Liflandsgade 8, 2300 København",
                Latitude = 55.660869,
                Longitude = 12.603241,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "Facial massage",
                Description = @"30 mins facial massage",
                CategoryID = 1,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 199,
                Currency = "DKK",
                ContactName = "Natasha's Wellness",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Blegdamsvej 112A 2100 København",
                Latitude = 55.697579,
                Longitude = 12.574109,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Listings.Add(new Model.Models.Listing()
            {
                Title = "Waxing",
                Description = @"Choose one of below for waxing: arms / thighs / Lower armpit / Upper lip / Upper lip and chin",
                CategoryID = 3,
                ListingTypeID = 1,
                UserID = user.Id,
                Price = 249,
                Currency = "DKK",
                ContactName = "Mind Body and Soul",
                ContactEmail = "demo@beyourmarket.com",
                IP = HttpContext.Current.Request.GetVisitorIP(),
                Location = "Blegdamsvej 84, St. tv , København Ø 2100",
                Latitude = 55.696199,
                Longitude = 12.571483,
                Active = true,
                Enabled = true,
                Expiration = DateTime.MaxValue.Date,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });            
        }

        private void InstallListingTypes(Model.Models.BeYourMarketContext context)
        {
            context.ListingTypes.Add(new BeYourMarket.Model.Models.ListingType()
            {
                Name = "Offer",
                ButtonLabel = "Book",
                OrderTypeID = (int)Enum_ListingOrderType.DateRange,
                OrderTypeLabel = "Number of days",
                PriceUnitLabel = "Per day",
                PaymentEnabled = true,
                PriceEnabled = true,
                ShippingEnabled = false,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.SaveChanges();
        }

        private void InstallCategories(Model.Models.BeYourMarketContext context)
        {
            context.Categories.Add(new Model.Models.Category()
            {
                Name = "Massage",
                Description = "Massage",
                Parent = 0,
                Enabled = true,
                Ordering = 0,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Categories.Add(new Model.Models.Category()
            {
                Name = "Facial",
                Description = "Facial Care",
                Parent = 0,
                Enabled = true,
                Ordering = 1,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.Categories.Add(new Model.Models.Category()
            {
                Name = "Skin Care",
                Description = "Skin Care",
                Parent = 0,
                Enabled = true,
                Ordering = 2,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });
        }

        private void InstallCategoryTypes(Model.Models.BeYourMarketContext context)
        {
            context.CategoryListingTypes.Add(new BeYourMarket.Model.Models.CategoryListingType()
            {
                CategoryID = 1,
                ListingTypeID = 1,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.CategoryListingTypes.Add(new BeYourMarket.Model.Models.CategoryListingType()
            {
                CategoryID = 2,
                ListingTypeID = 1,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.CategoryListingTypes.Add(new BeYourMarket.Model.Models.CategoryListingType()
            {
                CategoryID = 3,
                ListingTypeID = 1,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });
        }

        private void InstallPictures(Model.Models.BeYourMarketContext context)
        {
            for (int i = 1; i <= 9; i++)
            {
                context.Pictures.Add(new Model.Models.Picture()
                {
                    MimeType = "image/jpeg",
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });

                context.SaveChanges();

                context.ListingPictures.Add(new Model.Models.ListingPicture()
                {
                    ListingID = i,
                    PictureID = i,
                    ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                });

                // Copy files
                var pathFrom = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/images/sample/listing"), string.Format("{0}.{1}", i.ToString("00000000"), "jpg"));
                var pathTo = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/images/listing"), string.Format("{0}.{1}", i.ToString("00000000"), "jpg"));
                File.Copy(pathFrom, pathTo, true);
            }
        }

        private void InstallStripe(Model.Models.BeYourMarketContext context)
        {
            context.SettingDictionaries.Add(new Model.Models.SettingDictionary()
            {
                SettingID = 1,
                Name = "StripeApiKey",
                Value = "sk_test_kUNQFEh3YLbEFEa38tbeMJLV",
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.SettingDictionaries.Add(new Model.Models.SettingDictionary()
            {
                SettingID = 1,
                Name = "StripePublishableKey",
                Value = "pk_test_EfbP8SfcALEJ8Jk2JxtSxmqe",
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });

            context.SettingDictionaries.Add(new Model.Models.SettingDictionary()
            {
                SettingID = 1,
                Name = "StripeClientID",
                Value = "ca_6Rh18px61rjCEZIav5ItunZ1mKD8YjvU",
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });
        }

        private void InstallDisqus(Model.Models.BeYourMarketContext context)
        {
            context.SettingDictionaries.Add(new Model.Models.SettingDictionary()
            {
                SettingID = 1,
                Name = "Disqus_ShortName",
                Value = "beyourmarket",
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
            });
        }

        #endregion
    }
}