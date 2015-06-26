using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Plugins
{
    /// <summary>
    /// Plugin files parser
    /// </summary>
    public static class PluginFileParser
    {
        public static PluginInstalled ParseInstalledPluginsFile(string filePath)
        {
            //read and parse the file
            if (!File.Exists(filePath))
                return new PluginInstalled();

            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return new PluginInstalled();

            return JsonConvert.DeserializeObject<PluginInstalled>(text);
        }

        public static void SaveInstalledPluginsFile(PluginInstalled pluginSystemNames, string filePath)
        {
            var result = JsonConvert.SerializeObject(pluginSystemNames);

            File.WriteAllText(filePath, result);
        }

        public static PluginDescriptor ParsePluginDescriptionFile(string filePath)
        {
            var descriptor = new PluginDescriptor();

            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return descriptor;

            descriptor = JsonConvert.DeserializeObject<PluginDescriptor>(text);

            return descriptor;
        }

        public static void SavePluginDescriptionFile(PluginDescriptor plugin)
        {
            if (plugin == null)
                throw new ArgumentException("plugin");

            //get the Description.txt file path
            if (plugin.OriginalAssemblyFile == null)
                throw new Exception(string.Format("Cannot load original assembly path for {0} plugin.", plugin.SystemName));
            var filePath = Path.Combine(plugin.OriginalAssemblyFile.Directory.FullName, PluginManager.ManifestJson);
            if (!File.Exists(filePath))
                throw new Exception(string.Format("Description file for {0} plugin does not exist. {1}", plugin.SystemName, filePath));

            string pluginJson = JsonConvert.SerializeObject(plugin);

            //save the file
            File.WriteAllText(filePath, pluginJson);
        }
    }
}
