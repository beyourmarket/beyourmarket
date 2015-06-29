using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Plugins
{
    public interface IDependencyRegister
    {
        void Register(IUnityContainer container);

        int Order { get; }
    }
}
