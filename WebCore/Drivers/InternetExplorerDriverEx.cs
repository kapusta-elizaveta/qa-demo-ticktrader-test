﻿using OpenQA.Selenium.IE;

namespace WebCore.Drivers
{
    class InternetExplorerDriverEx : InternetExplorerDriver, IWebDriverEx
    {
        public InternetExplorerDriverEx(InternetExplorerDriverService service, InternetExplorerOptions options): base(service, options)
        {
            ProcessId = service.ProcessId;
        }

        public int ProcessId
        {
            get;
        }
    }
}
