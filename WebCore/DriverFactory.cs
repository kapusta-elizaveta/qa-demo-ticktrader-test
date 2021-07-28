﻿using System.IO;
using System.Reflection;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using WebCore.Drivers;

namespace WebCore
{
    public class DriverFactory
    {
        private static readonly string s_executeAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public ChromeDriverEx GetChromeDriver()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--ignore-certifcate-errors");
            chromeOptions.AddArguments("--incognito");
            chromeOptions.AddArguments("--disable-dev-shm-usage");
            chromeOptions.AddArguments("no-sandbox");
            chromeOptions.AddArguments("-no-sandbox");
            chromeOptions.AddArguments("--no-sandbox");
            chromeOptions.AddArguments("--disable-extensions");
            chromeOptions.AddArguments("--disable-browser-side-navigation");
          //  chromeOptions.AddArguments("--disable-notifications");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--disable-web-security");
            chromeOptions.AddArguments("--window-size=1920,1080");
            chromeOptions.AddArguments("--no-proxy-server");
            chromeOptions.AddArguments("enable-automation");
           // chromeOptions.AddArguments("--disable-geolocation");
            // Test Option for Headless Chrome Option
            //chromeOptions.AddArguments("--headless");
            //
            chromeOptions.AddUserProfilePreference("clipboardRead", true);
            chromeOptions.AddUserProfilePreference("download.default_directory", Configurator.DownloadFolder);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("download.directory_upgrade", true);
            chromeOptions.AddUserProfilePreference("safebrowsing.enabled", false);
            chromeOptions.AddUserProfilePreference("safebrowsing.disable_download_protection", true);
            chromeOptions.AddUserProfilePreference("profile.default_content_settings.notifications", 2);
            chromeOptions.AddUserProfilePreference("profile.default_content_settings_values.notifications", 2);
            chromeOptions.AddUserProfilePreference("profile.default_content_settings.popups", 2);
            chromeOptions.AddUserProfilePreference("profile.default_content_settings_values.popups", 2);
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 2);
            chromeOptions.UnhandledPromptBehavior = UnhandledPromptBehavior.Default;
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
            chromeOptions.SetLoggingPreference(LogType.Driver, LogLevel.All);
            return new ChromeDriverEx(ChromeDriverService.CreateDefaultService(s_executeAssemblyPath), chromeOptions);
        }

        public IWebDriverEx GetFirefoxDriver()
        {
            var mimeTypes =
                "image/png,image/gif,image/jpeg,image/pjpeg,application/pdf,text/csv,application/vnd.ms-excel," +
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" +
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            var ffOptions = new FirefoxOptions();
            var profile = new FirefoxProfile();
            profile.SetPreference("security.sandbox.content.level", 4);
            profile.SetPreference("browser.download.dir", Configurator.DownloadFolder);
            profile.SetPreference("browser.download.folderList", 2);
            profile.SetPreference("browser.helperApps.alwaysAsk.force", false);
            profile.SetPreference("browser.download.manager.focusWhenStarting", false);
            profile.SetPreference("browser.download.manager.useWindow", false);
            profile.SetPreference("browser.download.manager.showAlertOnComplete", false);
            profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", mimeTypes);
            profile.SetPreference("browser.helperApps.neverAsk.openFile", mimeTypes);
            profile.SetPreference("clipboardRead", "true");
            ffOptions.Profile = profile;
            ffOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
            ffOptions.SetLoggingPreference(LogType.Driver, LogLevel.All);

            return new FirefoxDriverEx(FirefoxDriverService.CreateDefaultService(s_executeAssemblyPath), ffOptions);
        }

        public IWebDriverEx GetInternetExplorerDriver()
        {
            var ieOptions = new InternetExplorerOptions();
            ieOptions.AddAdditionalCapability("--no-sandbox", true);
            ieOptions.AddAdditionalCapability("security.sandbox.content.level", 5);
            ieOptions.AddAdditionalCapability("download.prompt_for_download", true);
            ieOptions.AddAdditionalCapability("download.default_directory", Configurator.DownloadFolder);
            ieOptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
            ieOptions.AddAdditionalCapability("clipboardRead", true);
            ieOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
            ieOptions.SetLoggingPreference(LogType.Driver, LogLevel.All);
            return new InternetExplorerDriverEx(InternetExplorerDriverService.CreateDefaultService(s_executeAssemblyPath), ieOptions);
        }
    }
}