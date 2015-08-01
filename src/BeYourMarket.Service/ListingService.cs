using BeYourMarket.Model.Models;
using Repository.Pattern.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
    public interface IListingService : IService<Listing>
    {
        Dictionary<DateTime, int> GetItemsCount(DateTime datetime);

        Dictionary<Category, int> GetCategoryCount();
    }

    public class ListingService : Service<Listing>, IListingService
    {
        public ListingService(IRepositoryAsync<Listing> repository)
            : base(repository)
        {
        }

        public Dictionary<DateTime, int> GetItemsCount(DateTime fromDate)
        {
            var itemsCountDictionary = new Dictionary<DateTime, int>();
            for (DateTime i = fromDate; i <= DateTime.Now.Date; i = i.AddDays(1))
            {
                itemsCountDictionary.Add(i, 0);
            }
            
            var itemsCountQuery = Queryable().Where(x => x.Created >= fromDate).GroupBy(x => System.Data.Entity.DbFunctions.TruncateTime(x.Created)).Select(x => new { i = x.Key.Value, j = x.Count() }).ToDictionary(x => x.i, x => x.j);
            foreach (var item in itemsCountQuery)
            {
                itemsCountDictionary[item.Key] = item.Value;
            }

            return itemsCountDictionary;
        }


        public Dictionary<Category, int> GetCategoryCount()
        {
            return Queryable().GroupBy(x => x.Category).Select(x => new { i = x.Key, j = x.Count() }).ToDictionary(x => x.i, x => x.j);
        }
    }
}
