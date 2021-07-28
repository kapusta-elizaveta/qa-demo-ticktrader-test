using System;
using OpenQA.Selenium.Chrome;

namespace WebCore.Drivers
{
    public class ChromeDriverEx : ChromeDriver, IWebDriverEx
    {
        public ChromeDriverEx(ChromeDriverService service, ChromeOptions options): base(service, options, TimeSpan.FromMinutes(30))
        {
            ProcessId = service.ProcessId;
        }

        public int ProcessId
        {
            get;
        }
    }
}