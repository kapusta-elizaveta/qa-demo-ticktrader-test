﻿using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using OpenQA.Selenium.Support.UI;

namespace WebCore.Elements
{
    public class UIElement : IWebElement
    {
        protected IWebElement Element;
        protected By Locator;
        protected string Name;
        private const int m_delay = 100;
        
        public UIElement(FindBy type, string locator)
        {
            switch (type)
            {
                case FindBy.Xpath:
                    Locator = By.XPath(locator);
                    break;
                case FindBy.Name:
                    Locator = By.Name(locator);
                    break;
                case FindBy.Css:
                    Locator = By.CssSelector(locator);
                    break;
                case FindBy.Id:
                    Locator = By.Id(locator);
                    break;
                case FindBy.ClassName:
                    Locator = By.ClassName(locator);
                    break;
                case FindBy.PartialLinkText:
                    Locator = By.PartialLinkText(locator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public By GetLocator => Locator;

        public bool Presented
        {
            get
            {
                try
                {
                    return Browser.Current.WrappedDriver.FindElements(Locator).Count >= 1;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            }
        }

        public string InnerText
        {
            get
            {
                Browser.Current.WaitForElementPresent(Locator);
                Browser.Current.ScrollToElement(Locator);
                Element = GetElement();
                return Element.GetAttribute("innerText");
            }
        }

        public string Value
        {
            get
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.Current.WaitForElementPresent(Locator);
                Element = GetElement();
                return Element.GetAttribute("value");
            }
        }

        public bool Hidden
        {
            get
            {
                try
                {
                    Browser.Current.ScrollToElement(Locator);
                    Browser.Current.WaitForElementPresent(Locator);
                    if (Browser.FindElement(Locator).Displayed)
                    {
                        return FindParentWithAttribute("class", "form-item-offscreen") != null;
                    }
                }
                catch (NoSuchElementException)
                {

                }
                return true;
            }
        }

        public string Text
        {
            get
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.Current.WaitForElementVisible(Locator);
                Element = GetElement();
                return Element.Text;
            }
        }

        public bool Enabled
        {
            get
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.Current.WaitForElementPresent(Locator);
                return Browser.FindElement(Locator).Enabled;
            }
        }

        public bool Selected
        {
            get
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.Current.WaitForElementPresent(Locator);
                return Browser.FindElement(Locator).Selected;
            }
        }

        public Point Location { get; }
        
        public Size Size { get; }
        
        public string TagName { get; }

        public bool Displayed
        {
            get
            {
                try
                {
                    if (Presented)
                    {
                        Browser.Current.ScrollToElement(Locator);
                        Browser.Current.WaitForElementPresent(Locator, 3);
                        var element = Browser.FindElement(Locator);

                        return element.Displayed;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return false;
            }
        }

        public int Width
        {
            get
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.Current.WaitForElementVisible(Locator);
                Element = GetElement();
                return Element.Size.Width;
            }
        }

        public IWebElement WebElement
        {
            get
            {
                return GetElement();
            }
        }

        public IWebElement GetParent
        {
            get
            {
                return Browser.FindElement(Locator).FindElement(By.XPath(".."));
            }
        }

        public void SendKeys(string text)
        {
            try
            {
                var webElement = Browser.Current.WaitForElementVisible(Locator);
                webElement.SendKeys(text);
                //Console.WriteLine(text + " entered in the " + elementName + " field.");
            }
            catch (Exception e)
            {
                ScrollToElement();
                Browser.Current.WaitForElementVisible(Locator);
                Browser.FindElement(Locator).SendKeys(text);
            }
            
        }
        
        public void SendKeysWithoutScroll(string text)
        {
            Browser.Current.WaitForElementVisible(Locator);
            Browser.FindElement(Locator).SendKeys(text);
        }
        
        public void SendKeysWithDelay(string text, int delay = m_delay)
        {
            ScrollToElement();
            Browser.Current.WaitForElementVisible(Locator);
            var textChars = text.Split();
            Browser.FindElement(Locator).Click();
            foreach (var symbol in textChars)
            {
                Thread.Sleep(delay);
                Browser.FindElement(Locator).SendKeys(symbol);
            }
        }

        public void SendKeysWithJs(string text)
        {
            ScrollToElement();
            Browser.Current.WaitForElementVisible(Locator);
            var input = Browser.FindElement(Locator);
            Browser.Current.ExecuteJS($"arguments[0].value='{text}';", input);
        }

        public void Submit()
        {
            Browser.Current.ScrollToElement(Locator);
            Browser.Current.WaitForElementClickable(Locator);
            Browser.FindElement(Locator).Submit();
            Browser.Current.WaitForPageLoad();
        }

        public void Set(bool flag)
        {
            var webElement = Browser.Current.WaitForElementPresent(Locator); 
            if ( webElement != null)
            {
                if (!webElement.Displayed)
                {
                    ScrollToElement();                    
                }
                
                if (webElement.Selected != flag)
                {
                    webElement.Click();
                }
            }
        }

        public void SetDate(string date)
        {
            ScrollToElement();
            Browser.Current.WaitForElementVisible(Locator);
            Element = Browser.FindElement(Locator);
            Element.Click();
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                for (var i = 1; i < 15; i++)
                {
                    Element.SendKeys(Keys.Backspace);    
                }
            }
            else
            {
                Element.SendKeys(Keys.Control + "a");
            }
            Element.SendKeys(date + Keys.Tab);
        }
        
        public void Click()
        {
            try
            {
                FindElement(Locator).Click();
               // Console.WriteLine("Clicked on " + elementName);
            }
            catch (Exception)
            {
                ScrollToElement();
                Browser.Current.WaitForElementVisible(Locator);
                Browser.Current.WaitForElementClickable(Locator);
                Browser.ClickElement(Browser.FindElement(Locator));
            }
            
            Browser.Current.WaitForPageLoad();
        }

        public void ClickWithoutScroll()
        {
            Browser.Current.WaitForElementVisible(Locator);
            Browser.FindElement(Locator).Click();
        } 
        public void ClickAndSendKeys(string text)
        {
            try
            {
                Click();
                Browser.FindElement(Locator).SendKeys(text);
            }
            catch (NoSuchElementException)
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.ClickElement(Browser.FindElement(Locator));
                Browser.FindElement(Locator).SendKeys(text);
            }
            catch (ElementNotVisibleException)
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.ClickElement(Browser.FindElement(Locator));
                Browser.FindElement(Locator).SendKeys(text);
            }
            Browser.Current.WaitForPageLoad();
        }

        public string GetAttribute(string attributeName)
        {
            Browser.Current.WaitForElementPresent(Locator);
            Browser.Current.ScrollToElement(Locator);
            return Browser.FindElement(Locator).GetAttribute(attributeName);
        }
        
        public void SetAttribute(string attributeName, string value)
        {
            Browser.Current.WaitForElementPresent(Locator);
            Browser.Current.ScrollToElement(Locator);
            var executor = (IJavaScriptExecutor)Browser.Current.WrappedDriver;
            executor.ExecuteScript($"arguments[0].setAttribute('{attributeName}', '{value}');", Element = GetElement());
        }

        public string GetCssValue(string propertyName)
        {
            Browser.Current.WaitForElementVisible(Locator);
            return Browser.FindElement(Locator).GetCssValue(propertyName);
        }

        public string GetProperty(string propertyName)
        {
            Browser.Current.WaitForElementVisible(Locator);
            return Browser.FindElement(Locator).GetProperty(propertyName);
        }

        public void Clear()
        {
            Browser.Current.ScrollToElement(Locator);
            Browser.Current.WaitForElementVisible(Locator);
            Browser.FindElement(Locator).Clear();
        }

        public void ClearAndSendKeys(string text)
        {
            ScrollToElement();
            Browser.Current.WaitForElementVisible(Locator);
            var webElement = Browser.FindElement(Locator);
            webElement.Clear();
            webElement.SendKeys(text);
        }

        public string GetText()
        {
            Browser.Current.ScrollToElement(Locator);
            Browser.Current.WaitForElementVisible(Locator);
            Element = GetElement();
            return Element.Text;
        }

        public string GetValueWithJs()
        {
            Browser.Current.WaitForElementPresent(Locator);
            Browser.Current.ScrollToElement(Locator);
            var executor = (IJavaScriptExecutor)Browser.Current.WrappedDriver;
            var elementValue = (string)executor.ExecuteScript(
                "return arguments[0].value",
                GetElement());
            return elementValue;
        }

        /*
        public string GetValueWithJs()
        {
            var js = (IJavaScriptExecutor)Browser.Current.WrappedDriver;
            var elementValue = (string)js.ExecuteScript(
                "return arguments[0].value",
                Element = GetElement());
            return elementValue;
        }
        */

        public void DoubleClick()
        {
            Browser.Current.WaitForElementClickable(Locator);
            Browser.Current.ScrollToElement(Locator);
            new Actions(Browser.Current.WrappedDriver).DoubleClick(Browser.FindElement(Locator)).Perform();
            Browser.Current.WaitForPageLoad();
        }

        public void JsClick()
        {
            Browser.Current.WaitForElementPresent(Locator);
            Browser.Current.ScrollToElement(Locator);
            var executor = (IJavaScriptExecutor)Browser.Current.WrappedDriver;
            executor.ExecuteScript("arguments[0].click();", GetElement());
            Browser.Current.WaitForPageLoad();
        }
        
        public void JsClickWithoutScroll()
        {
            Browser.Current.WaitForElementPresent(Locator);
            var executor = (IJavaScriptExecutor)Browser.Current.WrappedDriver;
            executor.ExecuteScript("arguments[0].click();", GetElement());
            Browser.Current.WaitForPageLoad();
        }

        public void JsHighLighter()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var executor = (IJavaScriptExecutor)Browser.Current.WrappedDriver;
            executor.ExecuteScript(@"arguments[0].style.cssText = ""border-width: 2px; border-style: solid; border-color: red"";", GetElement());
        }

        public void ContextActClick()
        {
            var act = new Actions(Browser.Current.WrappedDriver);
            Browser.Current.WaitForElementVisible(Locator);
            act.ContextClick(FindElement(Locator)).Build().Perform();
        }

        public void MoveToElement()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var action = new Actions(Browser.Current.WrappedDriver);
            action.MoveToElement(Browser.FindElement(Locator)).Build().Perform();
        }

        public void MouseDown()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var action = new Actions(Browser.Current.WrappedDriver);
            action.MoveToElement(Browser.FindElement(Locator)).Click().Build().Perform();
        }

        public void DragAndDrop(int offsetX, int offsetY)
        {
            Browser.Current.WaitForElementPresent(Locator);
            Actions action = new Actions(Browser.Current.WrappedDriver);
            action.DragAndDropToOffset(Browser.FindElement(Locator), offsetX, offsetY)
                .Build()
                .Perform();
        }

        public void DragAndDrop(UIElement element, int offsetX, int offsetY)
        {
            var targetElement = Browser.FindElement(element.GetLocator);
            Browser.Current.WaitForElementPresent(Locator);
            Actions action = new Actions(Browser.Current.WrappedDriver);
            action.DragAndDrop(Browser.FindElement(Locator), targetElement).DragAndDropToOffset(Browser.FindElement(Locator), offsetX, offsetY)
                .Build()
                .Perform();
        }

        public void DragAndDrop(UIElement element)
        {
            Browser.Current.WaitForElementPresent(Locator);
            var action = new Actions(Browser.Current.WrappedDriver);
            action.DragAndDrop(Browser.FindElement(Locator), Browser.FindElement(element.GetLocator))
                .Build()
                .Perform();
        }

        public void ScrollToElement()
        {
            try
            {
                var element = Browser.Current.WaitForElementPresent(Locator);
                Browser.Current.WrappedDriver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", element);
            }
            catch (Exception)
            {
            }
        }

        protected IWebElement GetElement()
        {
            return Browser.FindElement(Locator);
        }

        public IWebElement GetChild(By by)
        {
            try
            {
                Browser.Current.ScrollToElement(Locator);
                Browser.Current.WaitForElementVisible(Locator);
                Element = GetElement();

                return Element.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public List<IWebElement> FindElements()
        {
            try
            {
                Browser.Current.WaitForElementPresent(Locator, 5);
            }
            catch { }
            return Browser.Current.WrappedDriver.FindElements(Locator).ToList();
        }

        public IWebElement FindElement(By by)
        {
            return Browser.Current.WaitForElementPresent(Locator, 5);
            //return Browser.FindElement(Locator);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            Browser.Current.WaitForElementPresent(Locator);
            return Browser.Current.WrappedDriver.FindElements(Locator);
        }

        public IWebElement FindParentWithAttribute(string attribute, string value)
        {
            try
            {
                return Browser.FindElement(Locator).FindElement(By.XPath($"./ancestor::*[contains(@{attribute}, '{value}')]"));
            }
            catch
            {
                return null;
            }
        }

        public void WaitForElementHasValue(int waitSeconds = 10)
        {
            new WebDriverWait(Browser.Current.WrappedDriver, TimeSpan.FromSeconds(waitSeconds)).Until(condition =>
            {
                return GetValueWithJs() != string.Empty;
            });
        }
    }
}