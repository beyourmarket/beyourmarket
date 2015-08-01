using BeYourMarket.Core.Controllers;
using BeYourMarket.Model.Models;
using BeYourMarket.Service.Models;
using Microsoft.Practices.Unity;
using Repository.Pattern.Repositories;
using Repository.Pattern.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
    public class DataCacheService
    {
        public MemoryCache MainCache { get; private set; }

        private ISettingService SettingService
        {
            get { return _container.Resolve<ISettingService>(); }
        }

        private ISettingDictionaryService SettingDictionaryService
        {
            get { return _container.Resolve<ISettingDictionaryService>(); }
        }

        private ICategoryService CategoryService
        {
            get { return _container.Resolve<ICategoryService>(); }
        }

        private IListingTypeService ListingTypeService
        {
            get { return _container.Resolve<IListingTypeService>(); }
        }

        private IContentPageService ContentPageService
        {
            get { return _container.Resolve<IContentPageService>(); }
        }

        private IEmailTemplateService EmailTemplateService
        {
            get { return _container.Resolve<IEmailTemplateService>(); }
        }

        private ICategoryStatService CategoryStatService
        {
            get { return _container.Resolve<ICategoryStatService>(); }
        }

        private IListingService ListingService
        {
            get { return _container.Resolve<IListingService>(); }
        }

        private IListingStatService ListingStatservice
        {
            get { return _container.Resolve<IListingStatService>(); }
        }

        private IOrderService OrderService
        {
            get { return _container.Resolve<IOrderService>(); }
        }

        private IAspNetUserService AspNetUserService
        {
            get { return _container.Resolve<IAspNetUserService>(); }
        }

        private IUnityContainer _container;

        private object _lock = new object();

        public DataCacheService(IUnityContainer container)
        {
            _container = container;

            MainCache = new MemoryCache("MainCache");

            GetCachedItem(CacheKeys.Settings);
            GetCachedItem(CacheKeys.SettingDictionary);
            GetCachedItem(CacheKeys.Categories);
            GetCachedItem(CacheKeys.ContentPages);
            GetCachedItem(CacheKeys.EmailTemplates);
            GetCachedItem(CacheKeys.Statistics);
        }

        public void UpdateCache(CacheKeys CacheKeyName, object CacheItem, int priority = (int)CacheItemPriority.NotRemovable)
        {
            lock (_lock)
            {
                var policy = new CacheItemPolicy();
                policy.Priority = (CacheItemPriority)priority;
                //policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10.00);

                // Add inside cache 
                MainCache.Set(CacheKeyName.ToString(), CacheItem, policy);
            }
        }

        public object GetCachedItem(CacheKeys CacheKeyName)
        {
            lock (_lock)
            {
                if (!MainCache.Contains(CacheKeyName.ToString()))
                {
                    switch (CacheKeyName)
                    {
                        case CacheKeys.Settings:
                            var setting = SettingService.Queryable().FirstOrDefault();
                            UpdateCache(CacheKeys.Settings, setting);
                            break;
                        case CacheKeys.SettingDictionary:
                            var settingDictionary = SettingDictionaryService.Queryable().ToList();
                            UpdateCache(CacheKeys.SettingDictionary, settingDictionary);
                            break;
                        case CacheKeys.Categories:
                            var categories = CategoryService.Queryable().Where(x => x.Enabled).OrderBy(x => x.Ordering).ToList();
                            UpdateCache(CacheKeys.Categories, categories);
                            break;
                        case CacheKeys.ListingTypes:
                            var ListingTypes = ListingTypeService.Query().Include(x => x.CategoryListingTypes).Select().ToList();
                            UpdateCache(CacheKeys.ListingTypes, ListingTypes);
                            break;
                        case CacheKeys.ContentPages:
                            var contentPages = ContentPageService.Queryable().Where(x => x.Published).OrderBy(x => x.Ordering).ToList();
                            UpdateCache(CacheKeys.ContentPages, contentPages);
                            break;
                        case CacheKeys.EmailTemplates:
                            var emailTemplates = EmailTemplateService.Queryable().ToList();
                            UpdateCache(CacheKeys.EmailTemplates, emailTemplates);
                            break;
                        case CacheKeys.Statistics:
                            SaveCategoryStats();

                            var statistics = new Statistics();
                            statistics.CategoryStats = CategoryStatService.Query().Include(x => x.Category).Select().ToList();

                            statistics.ListingCount = ListingService.Queryable().Count();
                            statistics.UserCount = AspNetUserService.Queryable().Count();
                            statistics.OrderCount = OrderService.Queryable().Count();
                            statistics.TransactionCount = 0;

                            statistics.ItemsCountDictionary = ListingService.GetItemsCount(DateTime.Now.AddDays(-10));

                            UpdateCache(CacheKeys.Statistics, statistics);
                            break;
                        default:
                            break;
                    }
                };

                return MainCache[CacheKeyName.ToString()] as Object;
            }
        }

        // Update categories stats
        private void SaveCategoryStats()
        {
            var unitOfWorkAsync = _container.Resolve<IUnitOfWorkAsync>();

            var categoryCountDctionary = ListingService.GetCategoryCount();

            foreach (var item in categoryCountDctionary)
            {
                var categoryStatQuery = CategoryStatService.Query(x => x.CategoryID == item.Key.ID).Select();

                var categoryStat = categoryStatQuery.FirstOrDefault();

                if (categoryStat != null)
                {
                    categoryStat.Count = item.Value;
                    categoryStat.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
                }
                else
                {
                    CategoryStatService.Insert(new CategoryStat()
                    {
                        CategoryID = item.Key.ID,
                        Count = item.Value,
                        ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
                    });
                }
            }

            unitOfWorkAsync.SaveChanges();
        }

        public void RemoveCachedItem(CacheKeys CacheKeyName)
        {
            if (MainCache.Contains(CacheKeyName.ToString()))
            {
                MainCache.Remove(CacheKeyName.ToString());
            }
        }

    }
}
