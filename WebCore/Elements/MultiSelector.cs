﻿using System.Collections.Generic;
using OpenQA.Selenium;

namespace WebCore.Elements
{
    public class MultiSelector : UIElement
    {
        private string searchResult = ".select2-results__options[aria-hidden='false']";
        private string m_noResults = "//span[contains(@class, 'multi-select')]//li[text()='No results found']";

        public MultiSelector(FindBy type,
            string locator) : base(type, locator) { }

        public UIElement Link_Import => new UIElement(FindBy.ClassName, "import-multi-users-select");
        public bool IsNoResultsFoundAlertEnabled => new UIElement(FindBy.Xpath, m_noResults).Enabled;

        public void DeleteCurrentValue(string value)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var webElementList = Browser.FindElements(Locator);
            if (webElementList.Count > 0)
            {
                Browser.FindElement(Locator).FindElement(By.XPath($"//li[@title='{value}']//span")).Click();
            }
        }

        public void DeleteCurrentValues()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var currentValues = GetSelectedValues();

            if (currentValues.Count == 0)
                return;
            foreach (var value in currentValues)
            {
                Browser.FindElement(Locator).FindElement(By.XPath($"//li[@title='{value}']//span")).Click();
                Browser.FindElement(Locator).FindElement(By.XPath("following-sibling::span//input[@type='search']"))
                    .Click();
            }
        }

        public bool IsValuePresented(string value)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var result = Browser.FindElement(Locator).FindElements(By.XPath($"//li[@title='{value}']//span")).Count > 0;
            return result;
        }

        public bool IsValuePresented()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var countOfElements = Browser.FindElement(Locator)
                .FindElements(By.XPath($"//li[@class='select2-selection__choice']//span")).Count;
            return countOfElements > 0;
        }

        public List<string> GetSelectedValues()
        {
            var foundSelectedOptions = new List<string>();
            var locator = By.XPath(Locator.GetStringLocator() + "/..//li[@class='select2-selection__choice']");
            
            if (Browser.FindElements(locator).Count > 0)
            {
                var selectedElements = Browser.FindElements(locator);

                foreach (var element in selectedElements)
                {
                    foundSelectedOptions.Add(element.GetAttribute("title"));
                }
            }

            return foundSelectedOptions;
        }

        public void SelectValue(string value)
        {
            Browser.FindElement(Locator).Click();
            IWebElement webElement = Browser.FindElement(Locator);
            webElement.SendKeys(value);

            Browser.Current.WaitForElementVisible(By.CssSelector(searchResult));
            IWebElement searchElement = Browser.FindElement(By.CssSelector(searchResult));

            Browser.Current.WaitForElementVisible(
                By.XPath($"//ul[@class = 'select2-results__options']/li[(contains(.,'{value}'))]"));

            searchElement
                .FindElement(By.XPath($"//ul[@class = 'select2-results__options']/li[(contains(.,'{value}'))]"))
                .Click();

            Browser.Current.WaitForPageLoad();
        }

        public void SelectNotPrepopulatedValue(string value)
        {
            Browser.FindElement(By.XPath(Locator.GetStringLocator() + "//input")).SendKeys(value);
            var option = By.XPath($"//ul[@class = 'select2-results__options']/li[(contains(.,'{value}'))]");
            Browser.Current.WaitForElementClickable(option);
            Browser.FindElement(option).Click();
        }

        public void TypeValue(string value)
        {
            Browser.FindElement(Locator).Click();
            IWebElement webElement = Browser.FindElement(Locator);
            webElement.SendKeys(value);
        }

        private IWebElement GetExpandedElement()
        {
            var list = Browser.FindElements(Locator);
            foreach (var webElement in list)
            {
                if (webElement.GetAttribute("aria-expanded").ToLower().Equals("true"))
                {
                    return webElement;
                }
            }

            return null;
        }
    }
}