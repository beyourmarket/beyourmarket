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
    public interface IItemService : IService<Item>
    {
        Dictionary<DateTime, int> GetItemsCount(DateTime datetime);

        Dictionary<Category, int> GetCategoryCount();
    }

    public class ItemService : Service<Item>, IItemService
    {
        public ItemService(IRepositoryAsync<Item> repository)
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
            
            var itemsCountQuery = Queryable().Where(x => x.Created >= fromDate).GroupBy(x => EntityFunctions.TruncateTime(x.Created)).Select(x => new { i = x.Key.Value, j = x.Count() }).ToDictionary(x => x.i, x => x.j);
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
