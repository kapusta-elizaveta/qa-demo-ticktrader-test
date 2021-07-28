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
        private const int s_implicitTimeout = 10;
        private const int s_logoutTimeout = 3000;
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
            Current.AcceptAlert();
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

        public void DeleteAllCookies()
        {
            WrappedDriver.Manage().Cookies.DeleteAllCookies();
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

        public void OpenNewTab()
        {
            ExecuteJS("window.open()");
        }

        public void CloseWindow()
        {
            WrappedDriver.Close();
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

        public void ScrollToElement(IWebElement element)
        {
            AssertDisposed();
            WrappedDriver.ExecuteJavaScript<object>("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(100);
        }

        public void ScrollToTop()
        {
            AssertDisposed();
            WrappedDriver.ExecuteJavaScript("window.scrollTo(0, -document.body.scrollHeight)");
            Thread.Sleep(100);
        }

        public int CountElements(By locator)
        {
            return WrappedDriver.FindElements(locator).Count;
        }

        public void JsMouseOver(IWebElement element)
        {
            WrappedDriver.ExecuteJavaScript<object>(
                "var evObj = document.createEvent('MouseEvents');" +
                "evObj.initMouseEvent(\"mouseover\",true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);"
                +
                "arguments[0].dispatchEvent(evObj);",
                element);
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

        public bool WaitForElementInvisible(By locator,
            int seconds = s_waitElementTimeout)
        {
            var bResult = false;
            try
            {
                // Current.WrappedDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                WaitForElementVisible(locator, 2);
                var webElement = FindElement(locator);
                //   Current.WrappedDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(s_waitElementTimeout);
                if (webElement != null && webElement.Displayed)
                {
                    new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                        ExpectedConditions.InvisibilityOfElementLocated(locator));
                }

                bResult = true;
            }
            catch (NoSuchElementException)
            {
                bResult = true;
            }
            catch (Exception)
            {
                // ignored
            }

            return bResult;
        }

        public bool IsVisible(UIElement uiElement,
            int seconds = s_waitElementTimeout)
        {
            var locator = uiElement.GetLocator;
            var bResult = false;
            try
            {
                var webElement = FindElement(locator);
                if (webElement != null && !webElement.Displayed)
                {
                    new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                        ExpectedConditions.ElementIsVisible(locator));
                }

                bResult = true;
            }
            catch (NoSuchElementException)
            {
                bResult = true;
            }
            catch (Exception)
            {
                // ignored
            }

            return bResult;
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

        public void WaitForTextToBePresentInElement(IWebElement WebElement,
            string Text,
            int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            try
            {
                new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds))
                    .Until(ExpectedConditions.TextToBePresentInElement(WebElement, Text));
            }
            catch { }
        }

        public void WaitForTextToBePresentInElementValue(IWebElement WebElement,
            string Text,
            int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            try
            {
                new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds))
                    .Until(ExpectedConditions.TextToBePresentInElementValue(WebElement, Text));
            }
            catch { }
        }

        public void WaitForElementClickable(By locator,
            int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                ExpectedConditions.ElementToBeClickable(locator));
        }

        public void WaitForElementClickable(UIElement uiElement,
            int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds))
                .Until(ExpectedConditions.ElementToBeClickable(uiElement.GetLocator));
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

        public IWebElement WaitForElementVisible(UIElement uiElement, int seconds = s_waitElementTimeout)
        {
            return WaitForElementVisible(uiElement.GetLocator, seconds);
        }

        public IWebElement WaitForElementVisible(IWebElement webElement, int seconds = s_waitElementTimeout)
        {
            try
            {
                AssertDisposed();
                return new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                    ElementIsVisible(webElement));
            }
            catch (Exception ex)
            {
                Console.Out.Write(ex.ToString());
            }

            return null;
        }

        public IWebElement WaitForElementVisible(IWebElement parentElement, By by, int seconds = s_waitElementTimeout)
        {
            AssertDisposed();
            try
            {
                return new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(seconds)).Until(
                    ElementIsVisible(parentElement.FindElement(by)));
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public IWebElement WaitForAnyElementVisible(ICollection<IWebElement> webElementList)
        {
            try
            {
                var wait = new WebDriverWait(Current.WrappedDriver, TimeSpan.FromSeconds(s_waitElementTimeout));
                return wait.Until(driver => ValidateCollection(webElementList));
            }
            catch (Exception) { }

            return null;
        }
        
        public IAlert WaitForAllertPresent()
        {
            try
            {
                var wait = new WebDriverWait(Current.WrappedDriver, TimeSpan.FromSeconds(s_waitElementTimeout));
                return wait.Until(ExpectedConditions.AlertIsPresent());
                //return wait.Until(driver => ValidateCollection(webElementList));
            }
            catch (Exception) { }

            return null;
        }

        private IWebElement ValidateCollection(ICollection<IWebElement> webElementList)
        {
            foreach (var webElement in webElementList)
            {
                if (webElement.Displayed)
                {
                    return webElement;
                }
            }

            return null;
        }

        private static Func<IWebDriver, IWebElement> ElementIsVisible(IWebElement webElement)
        {
            return (Func<IWebDriver, IWebElement>) (driver =>
            {
                try
                {
                    if (webElement != null && webElement.Displayed)
                        return webElement;
                    return (IWebElement) null;

                }
                catch (StaleElementReferenceException)
                {
                    return (IWebElement) null;
                }
            });
        }
        
        public void SetUpTimeoutForLogOut(int timeoutMilliSeconds = s_logoutTimeout)
        {
            WrappedDriver.ExecuteJavaScript(
                $"localStorage.setItem(SESSION_STORAGE_ID, new Date().getTime() + {timeoutMilliSeconds})");
        }

        public void WaitForModalWindowDisappear()
        {
            Current.WaitForElementInvisible(By.CssSelector(".modal[aria-hidden='false'] .modal-content"));
        }

        public void WaitForModalWindowOpened()
        {
            Current.WaitForElementVisible(By.CssSelector(".modal[aria-hidden='false'] .modal-content"));
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

        public void WaitForMetaQueryDashboardLoad()
        {
            var spinnerLocator = By.CssSelector(".dx-loadpanel-wrapper");
            try
            {
                new WebDriverWait(Current.WrappedDriver, TimeSpan.FromSeconds(s_waitElementTimeout)).Until(
                    ExpectedConditions.ElementIsVisible(spinnerLocator));
            }
            catch (WebDriverTimeoutException) { }

            WaitForElementInvisible(spinnerLocator);
        }

        public void WaitForFileDownload(string pathToDownload)
        {
            Stopwatch watcher = new Stopwatch();
            watcher.Start();
            while (watcher.Elapsed.Seconds < s_waitElementTimeout)
            {
                if (File.Exists(pathToDownload)) break;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            watcher.Stop();
        }

        // Tabs methods
        public int GetTabsCount()
        {
            return Current.WrappedDriver.WindowHandles.Count;
        }

        public string GetCurrentWindowName()
        {
            return WrappedDriver.CurrentWindowHandle;
        }

        public void SwitchToLastWindow()
        {
            AssertDisposed();
            Current.WrappedDriver.SwitchTo().Window(WrappedDriver.WindowHandles.Last());
            Current.WrappedDriver.Manage().Window.Maximize();
        }

        public void SwitchToFirstWindow()
        {
            AssertDisposed();
            Current.WrappedDriver.SwitchTo().Window(WrappedDriver.WindowHandles.First());
        }

        public void SwitchWindowByName(string windowName)
        {
            WrappedDriver.SwitchTo().Window(windowName);
        }

        public void CloseLastTabAndSwitchToFirst()
        {
            if (GetTabsCount() > 1)
            {
                SwitchToLastWindow();
                Current.WrappedDriver.Close();
                SwitchToFirstWindow();
            }
        }

        public void CloseCurrentTab()
        {
            Current.WrappedDriver.Close();
        }

        // IFrame methods
        public static void SwitchToIframe(UIElement frameElement)
        {
            Current.WrappedDriver.SwitchTo().Frame(frameElement.WebElement);
        }

        public static void SwitchToIframe(string frameName)
        {
            Current.WrappedDriver.SwitchTo().Frame(frameName);
        }

        public static void SwitchToParentFrame()
        {
            Current.WrappedDriver.SwitchTo().ParentFrame();
        }

        public static void SwitchToDefaultContent()
        {
            Current.WrappedDriver.SwitchTo().DefaultContent();
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
                    MoveToElement(webElement);
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

        public static bool SendText(IWebElement webElement,
            string value)
        {
            try
            {
                Current.WaitForElementVisible(webElement);
                webElement.SendKeys(value);
                return true;
            }
            catch (Exception)
            {
                try
                {
                    Thread.Sleep(500);
                    webElement.SendKeys(value);
                    return true;
                }
                catch (Exception) { }
            }

            return false;
        }

        public static bool ClearElement(IWebElement webElement)
        {
            try
            {
                Current.WaitForElementVisible(webElement);
                webElement.Clear();
                return webElement.Text.Equals("");
            }
            catch (Exception)
            {
                try
                {
                    Thread.Sleep(500);
                    webElement.Clear();
                    return webElement.Text.Equals("");
                }
                catch (Exception) { }
            }

            return false;
        }

        public static UIElement FindUIElement(params By[] by)
        {
            return (UIElement) FindElement(by);
        }

        public static IWebElement FindElement(params By[] by)
        {
            var result = Current.WrappedDriver.FindElement(by[0]);
            for (var i = 1; i < by.Length && result != null; i++)
            {
                result = result.FindElement(by[i]);
            }

            return result;
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

        public IWebElement GetVisibleElement(By by)
        {
            var list = FindElements(by);
            foreach (var webElement in list)
            {
                if (webElement.Enabled) return webElement;
            }

            return null;
        }

        public static ReadOnlyCollection<IWebElement> FindElements(By by) => Current.WrappedDriver.FindElements(by);

        public static void MoveToElement(IWebElement element)
        {
            new Actions(Current.WrappedDriver).MoveToElement(element).Perform();
        }

        public static void MoveToElement(UIElement element)
        {
            new Actions(Current.WrappedDriver).MoveToElement(FindElement(element.GetLocator)).Perform();
        }

        public void SingleClickEnterButton()
        {
            AssertDisposed();
            new Actions(WrappedDriver).SendKeys(Keys.Enter).Perform();
        }

        public void SingleClickEscapeButton()
        {
            AssertDisposed();
            new Actions(WrappedDriver).SendKeys(Keys.Escape).Perform();
        }

        public static void SendKeys(string keysToSend)
        {
            new Actions(Current.WrappedDriver).SendKeys(keysToSend).Perform();
        }

        public static void DragAndDropElement(IWebElement element,
            int offsetX,
            int offsetY)
        {
            Actions action = new Actions(Current.WrappedDriver);
            action.DragAndDropToOffset(element, offsetX, offsetY)
                .Build()
                .Perform();
        }

        public static void DragAndDropToElement(IWebElement fromElement,
            IWebElement toElement)
        {
            Actions action = new Actions(Current.WrappedDriver);
            action.DragAndDrop(fromElement, toElement)
                .Build()
                .Perform();
        }

        public static string GetPasteText()
        {
            Stopwatch watch = new Stopwatch();
            var copiedLink = "";
            watch.Start();
            while (watch.Elapsed.Seconds < s_waitElementTimeout && string.IsNullOrEmpty(copiedLink))
            {
                ((IJavaScriptExecutor) Current.WrappedDriver)
                    .ExecuteScript(
                        "document.addEventListener('paste', (e) => {" +
                        "const text =  (e.clipboardData || window.clipboardData).getData('text');" +
                        "console.log(text);});");
                new Actions(Current.WrappedDriver).KeyDown(Keys.Control).SendKeys("v").Perform();
                new Actions(Current.WrappedDriver).Release().Perform();
                foreach (var item in Current.WrappedDriver.Manage().Logs.GetLog(LogType.Browser))
                {
                    if (item.Message.Contains("console"))
                    {
                        copiedLink = item.Message.Split(' ').Last().Replace("\"", "");
                    }
                }
            }

            watch.Stop();
            return copiedLink;
        }

        public static void SetText(string text,
            IWebElement element)
        {
            Current.WrappedDriver.ExecuteJavaScript(
                string.Format("arguments[0].innerHTML = \"{0}\";", text),
                element);
        }

        public static IWebElement GetParentElement(IWebElement el)
        {
            try
            {
                IJavaScriptExecutor executor = (IJavaScriptExecutor) Current.WrappedDriver;
                return (IWebElement) executor.ExecuteScript("return arguments[0].parentNode;", el);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return null;
        }

        public static IWebElement GetParentElementByTagName(IWebElement el,
            string tagName)
        {
            for (int i = 0; i < 100; i++)
            {
                el = GetParentElement(el);
                var value = (el != null && el.TagName != null) ? el.TagName : "";
                if (value.Contains(tagName))
                {
                    return el;
                }
            }

            return null;
        }

        public static IWebElement GetParentElementByAttribute(IWebElement el,
            string attributeName,
            string attributeValue)
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    if (el == null)
                    {
                        break;
                    }

                    el = GetParentElement(el);
                    var value = (el != null && el.GetAttribute(attributeName) != null)
                        ? el.GetAttribute(attributeName)
                        : "";
                    if (value.Contains(attributeValue))
                    {
                        return el;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public static IWebElement GetChild(IWebElement element,
            By by)
        {
            if (element != null)
            {
                try
                {
                    return element.FindElement(by);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }

            return null;
        }

        public static IList<IWebElement> GetChildes(IWebElement element,
            By by)
        {
            if (element != null)
            {
                try
                {
                    return element.FindElements(by);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }

            return null;
        }

        // Upload methods
        public void UploadFiles(string filePath,
            IWebElement element)
        {
            Current.ExecuteJS(
                "arguments[0].style = \"\"; arguments[0].style.display = \"block\"; arguments[0].style.visibility = \"visible\";",
                element);
            element.SendKeys(filePath);
            WaitForPageLoad();
        }

        public void UploadFiles(FileUtils file,
            IWebElement element)
        {
            UploadFiles(file.FilePath, file.FileName, element);
        }

        public void UploadFilesWithoutChecking(FileUtils file,
            IWebElement element)
        {
            Current.ExecuteJS(
                "arguments[0].style = \"\"; arguments[0].style.display = \"block\"; arguments[0].style.visibility = \"visible\";",
                element);
            element.SendKeys(Path.Combine(file.FilePath, file.FileName));
        }

        public void UploadFiles(string filePath,
            string fileName,
            IWebElement element)
        {
            Current.ExecuteJS(
                "arguments[0].style = \"\"; arguments[0].style.display = \"block\"; arguments[0].style.visibility = \"visible\";",
                element);

            var parent = GetParentElementByAttribute(element, "class", "k-dropzone");

            element.SendKeys(Path.Combine(filePath, fileName));
            WaitForPageLoad();

            var child = Current.WaitForElementVisible(parent, By.ClassName("k-upload-status"));

            if (child != null)
            {
                Current.WaitForTextToBePresentInElement(child, "Done", s_pageLoadDefaultTimeoutSeconds);
            }
            else
            {
                Thread.Sleep(TimeSpan.FromSeconds(s_waitElementTimeout));
            }
        }

        const string JsDropFile =
            "for(var b=arguments[0],k=arguments[1],l=arguments[2],c=b.ownerDocument,m=0;;){var e=b.getBoundingClientRect(),"
            + "g=e.left+(k||e.width/2),h=e.top+(l||e.height/2),f=c.elementFromPoint(g,h);if(f&&b.contains(f))break;if(1<++m)throw "
            + "b=Error('Element not interractable'),b.code=15,b;b.scrollIntoView({behavior:'instant',block:'center',inline:'center'})}"
            + "var a=c.createElement('INPUT');a.setAttribute('type','file');a.setAttribute('style','position:fixed;z-index:2147483647;left:0;top:0;');"
            + "a.onchange=function(){var b={effectAllowed:'all',dropEffect:'none',types:['Files'],files:this.files,setData:function(){},"
            + "getData:function(){},clearData:function(){},setDragImage:function(){}};window.DataTransferItemList&&(b.items=Object"
            + ".setPrototypeOf([Object.setPrototypeOf({kind:'file',type:this.files[0].type,file:this.files[0],getAsFile:function()"
            + "{return this.file},getAsString:function(b){var a=new FileReader;a.onload=function(a){b(a.target.result)};"
            + "a.readAsText(this.file)}},DataTransferItem.prototype)],DataTransferItemList.prototype));"
            + "Object.setPrototypeOf(b,DataTransfer.prototype);['dragenter','dragover','drop'].forEach(function(a)"
            + "{var d=c.createEvent('DragEvent');d.initMouseEvent(a,!0,!0,c.defaultView,0,0,0,g,h,!1,!1,!1,!1,0,null);"
            + "Object.setPrototypeOf(d,null);d.dataTransfer=b;Object.setPrototypeOf(d,DragEvent.prototype);f.dispatchEvent(d)});"
            + "a.parentElement.removeChild(a)};c.documentElement.appendChild(a);a.getBoundingClientRect();return a;";

        public void DropFile(IWebElement target,
            string filePath,
            double offsetX = 0,
            double offsetY = 0)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            IWebDriver driver = Current.WrappedDriver;
            var jse = (IJavaScriptExecutor) driver;

            var input = (IWebElement) jse.ExecuteScript(JsDropFile, target, offsetX, offsetY);
            input.SendKeys(filePath);
        }

        // JS methods
        public void ExecuteJS(string script,
            IWebElement element)
        {
            WrappedDriver.ExecuteJavaScript(script, element);
        }

        public void ExecuteJS(string script)
        {
            WrappedDriver.ExecuteJavaScript(script);
        }

        // Screenshot methods
        public byte[] MakeScreenshot(string filePath)
        {
            var ss = ((ITakesScreenshot) Current.WrappedDriver).GetScreenshot();
            ss.SaveAsFile(filePath, ScreenshotImageFormat.Png);

            return ss.AsByteArray;
        }

        // Alert methods
        public IAlert IsAlertPresent()
        {
            return IsAlertPresent(s_waitElementTimeout);
        }

        public IAlert IsAlertPresent(int waitTime)
        {
            try
            {
                new WebDriverWait(WrappedDriver, TimeSpan.FromSeconds(waitTime)).Until(
                    ExpectedConditions.AlertIsPresent());
                var alert = Current.WrappedDriver.SwitchTo().Alert();
                return alert;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void AcceptAlert()
        {
            var alert = IsAlertPresent(s_waitElementTimeout);
            alert?.Accept();
        }

        public void DismissAlert()
        {
            var alert = IsAlertPresent();
            alert?.Dismiss();
        }

        public string GetAlertText()
        {
            var alert = IsAlertPresent();
            return alert?.Text;
        }

        public static string JavaScriptGetFieldValue(object element)
        {
            var js = (IJavaScriptExecutor) Current.WrappedDriver;
            var elementValue = (string) js.ExecuteScript(
                "return arguments[0].value",
                element);
            return elementValue;
        }

        public string JavaScriptGetValueInputElement(string elementName)
        {
            var js = (IJavaScriptExecutor) Current.WrappedDriver;
            var elementValue = (string) js.ExecuteScript(
                "return arguments[0].value",
                Current.WaitForElementPresent(By.CssSelector($"[id={elementName}]")));
            return elementValue;
        }
    }
}