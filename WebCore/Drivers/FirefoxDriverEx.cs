﻿using OpenQA.Selenium.Firefox;

namespace WebCore.Drivers
{
    class FirefoxDriverEx : FirefoxDriver, IWebDriverEx
    {
        public FirefoxDriverEx(FirefoxDriverService service, FirefoxOptions options): base(service, options)
        {
            ProcessId = service.ProcessId;
        }

        public int ProcessId
        {
            get;
        }
    }
}
