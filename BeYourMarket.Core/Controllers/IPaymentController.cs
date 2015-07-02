using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Controllers
{
    public interface IPaymentController
    {
        bool OrderAction(int id, int status, out string message);

        bool HasPaymentMethod(string userId);

        int GetTransactionCount();
    }
}
