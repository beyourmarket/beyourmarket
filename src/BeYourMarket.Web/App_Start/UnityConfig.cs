using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Repository.Pattern.UnitOfWork;
using Repository.Pattern.DataContext;
using BeYourMarket.Model.Models;
using Repository.Pattern.Ef6;
using Repository.Pattern.Repositories;
using BeYourMarket.Service;
using BeYourMarket.Model.StoredProcedures;
using BeYourMarket.Core.Services;
using BeYourMarket.Core.Plugins;

namespace BeYourMarket.Web.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            container
                .RegisterType<IDataContextAsync, BeYourMarketContext>(new PerRequestLifetimeManager())
                .RegisterType<IUnitOfWorkAsync, UnitOfWork>(new PerRequestLifetimeManager())

                .RegisterType<IRepositoryAsync<Setting>, Repository<Setting>>()
                .RegisterType<IRepositoryAsync<Category>, Repository<Category>>()
                .RegisterType<IRepositoryAsync<Listing>, Repository<Listing>>()
                .RegisterType<IRepositoryAsync<ListingPicture>, Repository<ListingPicture>>()
                .RegisterType<IRepositoryAsync<Picture>, Repository<Picture>>()
                .RegisterType<IRepositoryAsync<Order>, Repository<Order>>()
                .RegisterType<IRepositoryAsync<StripeConnect>, Repository<StripeConnect>>()
                .RegisterType<IRepositoryAsync<MetaField>, Repository<MetaField>>()
                .RegisterType<IRepositoryAsync<MetaCategory>, Repository<MetaCategory>>()
                .RegisterType<IRepositoryAsync<ListingMeta>, Repository<ListingMeta>>()
                .RegisterType<IRepositoryAsync<ContentPage>, Repository<ContentPage>>()
                .RegisterType<IRepositoryAsync<SettingDictionary>, Repository<SettingDictionary>>()
                .RegisterType<IRepositoryAsync<ListingStat>, Repository<ListingStat>>()
                .RegisterType<IRepositoryAsync<ListingReview>, Repository<ListingReview>>()
                .RegisterType<IRepositoryAsync<EmailTemplate>, Repository<EmailTemplate>>()
                .RegisterType<IRepositoryAsync<CategoryStat>, Repository<CategoryStat>>()
                .RegisterType<IRepositoryAsync<AspNetUser>, Repository<AspNetUser>>()
                .RegisterType<IRepositoryAsync<AspNetRole>, Repository<AspNetRole>>()
                .RegisterType<IRepositoryAsync<ListingType>, Repository<ListingType>>()
                .RegisterType<IRepositoryAsync<CategoryListingType>, Repository<CategoryListingType>>()
                .RegisterType<IRepositoryAsync<Message>, Repository<Message>>()
                .RegisterType<IRepositoryAsync<MessageParticipant>, Repository<MessageParticipant>>()
                .RegisterType<IRepositoryAsync<MessageReadState>, Repository<MessageReadState>>()
                .RegisterType<IRepositoryAsync<MessageThread>, Repository<MessageThread>>()

                .RegisterType<ISettingService, SettingService>()
                .RegisterType<ICategoryService, CategoryService>()
                .RegisterType<ICategoryStatService, CategoryStatService>()
                .RegisterType<IListingService, ListingService>()
                .RegisterType<IListingPictureService, ListingPictureService>()
                .RegisterType<IPictureService, PictureService>()
                .RegisterType<IOrderService, OrderService>()
                .RegisterType<ICustomFieldService, CustomFieldService>()
                .RegisterType<ICustomFieldCategoryService, CustomFieldCategoryService>()
                .RegisterType<ICustomFieldListingService, CustomFieldListingService>()
                .RegisterType<IContentPageService, ContentPageService>()
                .RegisterType<ISettingDictionaryService, SettingDictionaryService>()
                .RegisterType<IListingStatService, ListingStatService>()
                .RegisterType<IEmailTemplateService, EmailTemplateService>()
                .RegisterType<IAspNetUserService, AspNetUserService>()
                .RegisterType<IAspNetRoleService, AspNetRoleService>()
                .RegisterType<IListingTypeService, ListingTypeService>()
                .RegisterType<IListingReviewService, ListingReviewService>()
                .RegisterType<ICategoryListingTypeService, CategoryListingTypeService>()
                .RegisterType<IMessageService, MessageService>()
                .RegisterType<IMessageParticipantService, MessageParticipantService>()
                .RegisterType<IMessageReadStateService, MessageReadStateService>()
                .RegisterType<IMessageThreadService, MessageThreadService>()                
                .RegisterType<IStoredProcedures, BeYourMarketContext>(new PerRequestLifetimeManager())
                .RegisterType<SqlDbService, SqlDbService>() 
                .RegisterType<DataCacheService, DataCacheService>(new ContainerControlledLifetimeManager());

            container
                .RegisterType<IHookService, HookService>()
                .RegisterType<IPluginFinder, PluginFinder>();


        }
    }
}
