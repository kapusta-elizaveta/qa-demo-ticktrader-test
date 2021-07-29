using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using WebCore.Elements;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace WebCore
{
    public class Browser : IDisposable
    {
        private const int s_waitElementTimeout = 10;
        private const int s_pageLoadDefaultTimeoutSeconds = 120;

        private static readonly ThreadLocal<Browser> s_instance = new ThreadLocal<Browser>(
            () => { return new Browser(); },
            true
        );

        private bool m_disposed;

        public Browser()
        {
            IWebDriverEx driver;

            switch (Configurator.BrowserType)
            {
                case "chrome":
                    driver = new DriverFactory().GetChromeDriver();
                    break;
                case "firefox":
                    driver = new DriverFactory().GetFirefoxDriver();
                    break;
                case "ie":
                    driver = new DriverFactory().GetInternetExplorerDriver();
                    break;
                default:
                    driver = new DriverFactory().GetChromeDriver();
                    break;
            }

            driver.Manage().Cookies.DeleteAllCookies();
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(s_pageLoadDefaultTimeoutSeconds);
            driver.Manage().Window.Maximize();
            WrappedDriver = driver;
            m_disposed = false;
        }

        public IWebDriverEx WrappedDriver { get; private set; }

        /// <summary>
        /// Returns existing browser for current thread, or starts a new one.
        /// </summary>
        public static Browser Current
        {
            get
            {
                var result = s_instance.Value;
                if (result.m_disposed)
                {
                    TryKillProcess();
                    result = new Browser();
                    s_instance.Value = result;
                }

                Thread.MemoryBarrier();
                return result;
            }
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                WrappedDriver?.Close();
                WrappedDriver?.Quit();
            }

            m_disposed = true;
        }

        private void AssertDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("Browser for " + Thread.CurrentThread.ManagedThreadId);
            }
        }

        /// <summary>
        /// Shuts down browser for current thread, next access to .Current will create a new one.
        /// </summary>
        public static void KillCurrent()
        {
            //Current.AcceptAlert();
            if (!s_instance.IsValueCreated)
            {
                return;
            }

            s_instance.Value.Dispose();
            TryKillProcess();
        }

        public void GoTo(string url)
        {
            //AssertDisposed();
            WrappedDriver.Navigate().GoToUrl(url);
        }

        public void Refresh()
        {
            WrappedDriver.Navigate().Refresh();
            WaitForPageLoad();
        }

        public static void TryKillProcess()
        {
            if (s_instance.Value != null)
            {
                try
                {
                    var process = Process.GetProcessById(s_instance.Value.WrappedDriver.ProcessId);
                    if (process != null)
                    {
                        process.Kill();
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine(e);
                }
            }
        }

        // Scrolling methods
        public void ScrollToElement(By locator)
        {
            AssertDisposed();
            WaitForElementPresent(locator);
            IWebElement element = WrappedDriver.FindElement(locator);
            WrappedDriver.ExecuteJavaScript<object>("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(100);
        }


        // Wait methods
        public IWebElement WaitForElementPresent(By locator,
            int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            try
            {
                return new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                    ExpectedConditions.ElementExists(locator));
            }
            catch
            {
                return null;
            }
        }

        public IWebElement WaitForElementPresent(UIElement uiElement,
            int seconds = s_waitElementTimeout)
        {
            return WaitForElementPresent(uiElement.GetLocator, seconds);
        }


        public bool WaitForElementInvisible(IWebElement webElement,
            int seconds = s_waitElementTimeout)
        {
            try
            {
                if (webElement.Displayed)
                {
                    var wait = new WebDriverWait(Current.WrappedDriver, TimeSpan.FromSeconds(seconds));
                    wait.Until(driver => !webElement.Displayed);
                }

                return true;
            }
            catch (Exception) { }

            return false;
        }


        public void WaitForElementClickable(By locator,
            int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                ExpectedConditions.ElementToBeClickable(locator));
        }

        public bool WaitForElementClickable(IWebElement webElement)
        {
            try
            {
                var wait = new WebDriverWait(Current.WrappedDriver, TimeSpan.FromSeconds(s_waitElementTimeout));
                wait.Until(driver => webElement.Enabled);

                return true;
            }
            catch (Exception) { }

            return false;
        }

        public IWebElement WaitForElementVisible(By locator, int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            try
            {
                return new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                    ExpectedConditions.ElementIsVisible(locator));
            }
            catch (Exception ex)
            {
                Console.Out.Write(ex.ToString());
            }

            return null;
        }

        public void WaitForPageLoad(IWebElement element = null, int seconds = s_waitElementTimeout)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            try
            {
                if ((bool) ((IJavaScriptExecutor) Current.WrappedDriver).ExecuteScript(
                    "return window.jQuery == undefined") && element == null)
                {
                    new WebDriverWait(Current.WrappedDriver, new TimeSpan(0, 0, Math.Max(seconds, 1))).Until(
                        d => ((IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete"));
                }
                else if ((bool) ((IJavaScriptExecutor) Current.WrappedDriver).ExecuteScript(
                    "return window.jQuery == undefined"))
                {
                    new WebDriverWait(Current.WrappedDriver, new TimeSpan(0, 0, Math.Max(seconds, 1))).Until(
                        d => (
                                 (IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete")
                             && element.Displayed
                             && element.Enabled
                    );
                }
                else if (element == null)
                {
                    new WebDriverWait(Current.WrappedDriver, new TimeSpan(0, 0, Math.Max(seconds, 1))).Until(
                        d => ((IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete") &&
                             (bool) ((IJavaScriptExecutor) d).ExecuteScript(
                                 "return window.jQuery != undefined && jQuery.active == 0"));
                }
                else
                {
                    new WebDriverWait(Current.WrappedDriver, new TimeSpan(0, 0, Math.Max(seconds, 1))).Until(
                        d => (
                                 (IJavaScriptExecutor) d).ExecuteScript("return document.readyState")
                             .Equals("complete") &&
                             (bool) ((IJavaScriptExecutor) d).ExecuteScript(
                                 "return window.jQuery != undefined && jQuery.active == 0") && element.Displayed
                             && element.Enabled);
                }
            }
            catch (WebDriverException ex)
            {
                Console.WriteLine(ex.ToString());
                var loadIcon = Current.WaitForElementVisible(By.Id("loadingPanel"), 1);
                //var loadIcon = FindElement(By.CssSelector("canvas#canvas"));
                if (loadIcon != null)
                {
                    Current.WaitForElementInvisible(loadIcon, s_pageLoadDefaultTimeoutSeconds);
                }
            }
            catch (NullReferenceException)
            {
                //Console.WriteLine(e);
            }
            
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
        }

        public void SwitchToLastWindow()
        {
            AssertDisposed();
            Current.WrappedDriver.SwitchTo().Window(WrappedDriver.WindowHandles.Last());
            Current.WrappedDriver.Manage().Window.Maximize();
        }

        // Element methods
        public static bool ClickElement(IWebElement webElement)
        {
            try
            {
                Current.WaitForElementClickable(webElement);
                webElement.Click();
                Thread.Sleep(100);
                return true;
            }
            catch (Exception)
            {
                try
                {
                    Thread.Sleep(250);
                    //MoveToElement(webElement);
                    webElement.Click();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return false;
        }

    
        public static IWebElement FindElement(By by)
        {
            try
            {
                return Current.WrappedDriver.FindElement(by);
            }
            catch (Exception)
            {
                
            }

            return null;
        }
        
    }
}