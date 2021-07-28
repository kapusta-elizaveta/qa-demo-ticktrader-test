﻿using OpenQA.Selenium;

namespace WebCore.Elements
{
    public class KendoDropDown
    {
        public static bool SelectValue(IWebElement listElement, string value)
        {
            var bResult = false;

            var optionsList = listElement.FindElements(By.XPath($".//li[text() = '{value}']"));

            var option = Browser.Current.WaitForAnyElementVisible(optionsList);
            
            if (optionsList.Count > 1)
            {
                foreach (var webElement in optionsList)
                {
                    if (webElement.Displayed)
                    {
                        option = webElement;
                        break;
                    }
                }
            }

            if (option != null)
            {
                Browser.ClickElement(option);
                
                bResult = true;
                bResult &= Browser.Current.WaitForElementInvisible(option);
            }
            
            return bResult;
        }
        
        public static bool SelectValue(UIElement uiElement, string value)
        {
            var bResult = false;
            
            var optionsList = uiElement.WebElement.FindElements(By.XPath($".//li[text() = '{value}']"));
            var option = Browser.Current.WaitForAnyElementVisible(optionsList);
            
            if (optionsList.Count > 1)
            {
                foreach (var webElement in optionsList)
                {
                    if (webElement.Displayed)
                    {
                        option = webElement;
                        break;
                    }
                }
            }

            if (option != null)
            {
                Browser.ClickElement(option);
                
                bResult = true;
                bResult &= Browser.Current.WaitForElementInvisible(option);
            }
            
            return bResult;
        }

        public static bool ClickAndSelectValue(IWebElement dropDown, string value)
        {
            var bResult = false;

            if (!Browser.ClickElement(dropDown))
            {
                return bResult;
            }
            
            var visibleList = GetVisibleList();

            if (visibleList != null)
            {
                bResult = SelectValue(visibleList, value);
            }

            return bResult;
        }

        private static IWebElement GetVisibleList()
        {
            return Browser.Current.WaitForElementPresent(By.CssSelector(".k-list.k-reset[aria-hidden='false']"), 1)
                       ?? Browser.Current.WaitForElementPresent(By.CssSelector(".select2-results__options[aria-expanded='true']"), 1);
        }
    }
}