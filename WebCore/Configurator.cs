using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace WebCore
{
    public static class Configurator
    {
        private static readonly Lazy<IConfiguration> s_configuration;

        public static IConfiguration Configuration => s_configuration.Value;
        public static string BaseUrl => Configuration[nameof(BaseUrl)];
        private static readonly string s_executeAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string s_downloadFolder = Path.Combine(s_executeAssemblyPath, "download");
        private static readonly string m_downloadFolder = Directory.CreateDirectory(s_downloadFolder).FullName;
        
        public static string BrowserType => Configuration[nameof(BrowserType)];
        public static string DownloadFolder => m_downloadFolder;

        static Configurator()
        {
            s_configuration = new Lazy<IConfiguration>(BuildConfiguration);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json");

            var appSettingFiles = Directory.EnumerateFiles(basePath, "appsettings.*.json");

            foreach (var appSettingFile in appSettingFiles)
            {
                builder.AddJsonFile(appSettingFile);
            }

            return builder.Build();
        }
    }
}