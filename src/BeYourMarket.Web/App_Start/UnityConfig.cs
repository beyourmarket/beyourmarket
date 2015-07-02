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
                .RegisterType<IRepositoryAsync<Item>, Repository<Item>>()
                .RegisterType<IRepositoryAsync<ItemPicture>, Repository<ItemPicture>>()
                .RegisterType<IRepositoryAsync<Picture>, Repository<Picture>>()
                .RegisterType<IRepositoryAsync<Order>, Repository<Order>>()
                .RegisterType<IRepositoryAsync<StripeConnect>, Repository<StripeConnect>>()
                .RegisterType<IRepositoryAsync<MetaField>, Repository<MetaField>>()
                .RegisterType<IRepositoryAsync<MetaCategory>, Repository<MetaCategory>>()
                .RegisterType<IRepositoryAsync<ItemMeta>, Repository<ItemMeta>>()
                .RegisterType<IRepositoryAsync<ContentPage>, Repository<ContentPage>>()
                .RegisterType<IRepositoryAsync<SettingDictionary>, Repository<SettingDictionary>>()
                .RegisterType<IRepositoryAsync<ItemStat>, Repository<ItemStat>>()
                .RegisterType<IRepositoryAsync<EmailTemplate>, Repository<EmailTemplate>>()
                .RegisterType<IRepositoryAsync<CategoryStat>, Repository<CategoryStat>>()
                .RegisterType<IRepositoryAsync<AspNetUser>, Repository<AspNetUser>>()
                .RegisterType<IRepositoryAsync<AspNetRole>, Repository<AspNetRole>>()

                .RegisterType<ISettingService, SettingService>()
                .RegisterType<ICategoryService, CategoryService>()
                .RegisterType<ICategoryStatService, CategoryStatService>()
                .RegisterType<IItemService, ItemService>()
                .RegisterType<IItemPictureService, ItemPictureService>()
                .RegisterType<IPictureService, PictureService>()
                .RegisterType<IOrderService, OrderService>()
                .RegisterType<ICustomFieldService, CustomFieldService>()
                .RegisterType<ICustomFieldCategoryService, CustomFieldCategoryService>()
                .RegisterType<ICustomFieldItemService, CustomFieldItemService>()
                .RegisterType<IContentPageService, ContentPageService>()
                .RegisterType<ISettingDictionaryService, SettingDictionaryService>()
                .RegisterType<IItemStatService, ItemStatService>()
                .RegisterType<IEmailTemplateService, EmailTemplateService>()
                .RegisterType<IAspNetUserService, AspNetUserService>()
                .RegisterType<IAspNetRoleService, AspNetRoleService>()
                .RegisterType<IStoredProcedures, BeYourMarketContext>(new PerRequestLifetimeManager())
                .RegisterType<SqlDbService, SqlDbService>() 
                .RegisterType<DataCacheService, DataCacheService>(new ContainerControlledLifetimeManager());

            container
                .RegisterType<IHookService, HookService>()
                .RegisterType<IPluginFinder, PluginFinder>();


        }
    }
}
