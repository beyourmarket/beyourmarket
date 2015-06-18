using BeYourMarket.Model.StoredProcedures;
using Repository.Pattern.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
    public class SqlDbService
    {
        private readonly IStoredProcedures _storedProcedures;

        public SqlDbService(IStoredProcedures storedProcedures)
        {
            _storedProcedures = storedProcedures;
        }

        public int UpdateCategoryItemCount(int categoryID)
        {
            return _storedProcedures.UpdateCategoryItemsCount(categoryID);
        }
    }
}
