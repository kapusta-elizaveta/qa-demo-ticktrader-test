using System.Linq;

namespace WebCore
{

    public class ApplicationUrls
    {
        public string BaseUrl { get; }


        private ApplicationUrls(string baseUrl, string tenant)
        {
            BaseUrl = baseUrl.Replace("{tenant}", tenant);
        }
    }
}