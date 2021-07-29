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
        

        public void Submit()
        {
            Browser.Current.ScrollToElement(Locator);
            Browser.Current.WaitForElementClickable(Locator);
            Browser.FindElement(Locator).Submit();
            Browser.Current.WaitForPageLoad();
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

       
        public string GetAttribute(string attributeName)
        {
            Browser.Current.WaitForElementPresent(Locator);
            Browser.Current.ScrollToElement(Locator);
            return Browser.FindElement(Locator).GetAttribute(attributeName);
        }

        public string GetProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        public string GetCssValue(string propertyName)
        {
            throw new NotImplementedException();
        }

        public string TagName { get; }

        public void Clear()
        {
            Browser.Current.ScrollToElement(Locator);
            Browser.Current.WaitForElementVisible(Locator);
            Browser.FindElement(Locator).Clear();
        }
        
        public void JsClick()
        {
            Browser.Current.WaitForElementPresent(Locator);
            Browser.Current.ScrollToElement(Locator);
            var executor = (IJavaScriptExecutor)Browser.Current.WrappedDriver;
            executor.ExecuteScript("arguments[0].click();", GetElement());
            Browser.Current.WaitForPageLoad();
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

        public IWebElement FindElement(By by)
        {
            return Browser.Current.WaitForElementPresent(Locator, 5);
            //return Browser.FindElement(Locator);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By @by)
        {
            throw new NotImplementedException();
        }
    }
}