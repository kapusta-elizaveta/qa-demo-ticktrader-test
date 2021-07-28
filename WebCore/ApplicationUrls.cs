using System.Linq;

namespace WebCore
{
    /*public static class ApplicationUrlExtensions
    {
        public static string CombineWith(this string baseUrl, params string[] segments)
        {
            char[] pathSeparators = { '/', '\\' };

            var trimmedSegments = segments.Select(s => s.Trim(pathSeparators));

            return $"{baseUrl.TrimEnd(pathSeparators)}/{string.Join('/', trimmedSegments)}";
        }
    }*/

    public class ApplicationUrls
    {
        /// <summary>
        /// By default this instance should be configured to automation tenant but can be overridden in configuration
        /// </summary>
        public static readonly ApplicationUrls Automation = new ApplicationUrls(
            Configurator.BaseUrl,
            Configurator.Tenant
        );

        /// <summary>
        /// By default this instance should be configured to automation-ds tenant but can be overridden in configuration
        /// </summary>
        public static readonly ApplicationUrls DsAutomation = new ApplicationUrls(
            Configurator.BaseUrl,
            Configurator.DsTenant
        );

        public string BaseUrl { get; }

        public string Portal { get; }

        public string Api { get; }

        public string Auth { get; }


        private ApplicationUrls(string baseUrl, string tenant)
        {
            BaseUrl = baseUrl.Replace("{tenant}", tenant);
            //Portal = BaseUrl.CombineWith("portal");
            //Api = BaseUrl.CombineWith("api");
            //Auth = BaseUrl.CombineWith("auth");
        }
    }
}