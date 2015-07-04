using BeYourMarket.Core.Plugins;
using BeYourMarket.Core.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket
{
    [TestFixture]
    public class BeYourMarketTest
    {
        Mock<IPluginFinder> _pluginFinder;
        Mock<IHookService> _hookService;
        Mock<IHookPlugin> _hookPlugin;

        Mock<PluginDescriptor> _pluginDescriptor;

        public BeYourMarketTest()
        {
            _hookPlugin = new Mock<IHookPlugin>();
            _pluginDescriptor = new Mock<PluginDescriptor>();

            _pluginFinder = new Mock<IPluginFinder>();
            _pluginFinder.Setup(x => x.GetPluginDescriptorBySystemName<IHookPlugin>(It.IsAny<string>(), It.IsAny<LoadPluginsMode>())).Returns(_pluginDescriptor.Object);

            _hookService = new Mock<IHookService>();
            _hookService.Setup(x => x.LoadActiveHooksByName(It.IsAny<string>()));

            _hookService = new Mock<IHookService>();
            _hookService.Setup(x => x.LoadHookBySystemName(It.IsAny<string>())).Returns(It.IsAny<IHookPlugin>());
        }

        [Test]
        public void TestHookService1()
        {
            var plugin = _hookService.Object.LoadActiveHooksByName("Plugin.Test");            
            
            _hookService.Verify(x => x.LoadActiveHooksByName("Plugin.Test"), Times.AtLeastOnce());
        }

        [Test]
        public void TestHookService2()
        {
            var plugin = _hookService.Object.LoadHookBySystemName("Plugin.Test");

            _hookService.Verify(x => x.LoadHookBySystemName("Plugin.Test"), Times.AtLeastOnce());

            //_pluginFinder.Verify(x => x.GetPluginDescriptorBySystemName<IHookPlugin>(It.IsAny<string>(), It.IsAny<LoadPluginsMode>()), Times.AtLeastOnce());

        }

    }
}
