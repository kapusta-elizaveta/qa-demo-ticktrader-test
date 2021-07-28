﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace WebCore.Elements
{
    public class DropDown : UIElement
    {
        private readonly string m_selectTextboxLocator ="//following-sibling::span//input[@role='textbox']";
        private readonly string m_selectComboboxLocator = "//following-sibling::span//span[@role='combobox']";
        private string searchResult = ".select2-results__options[aria-hidden='false']";
        private string m_treeItemLocator = "//li[@role='treeitem' and contains(text(),'{0}')]";
        private string m_searchInput = "//span[contains(@class,'drop-down') or contains(@class,'dropdown')]//span//input[@role='textbox']";
        private string m_clearButton = "/following-sibling::span[@role='button']";
        private string m_placeholder = "//span[@class='select2-selection__placeholder']";
        private string m_noResults = "//span[contains(@class, 'drop-down')]//li[text()='No results found']";

        public DropDown(FindBy type, string locator) : base(type, locator)
        { }

        public bool IsNoResultsFoundAlertEnabled => new UIElement(FindBy.Xpath, m_noResults).Enabled;

        public void SelectValueFromDropDown(string value)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var select = Browser.FindElement(Locator);
            var selectElement = new SelectElement(select);
            Browser.Current.WaitForElementClickable(selectElement.WrappedElement);
            selectElement.SelectByText(value);
        }

        public void SelectValueFromWrappedDropDownWithoutArrow(string value)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var select = Browser.FindElement(Locator);

            if (Configurator.BrowserType.Equals("firefox"))
            {
                var inputSelect = Browser.Current.WrappedDriver.FindElement(By.XPath(Locator.GetStringLocator() + m_selectTextboxLocator));
                inputSelect.Clear();
                inputSelect.SendKeys(value);
                new UIElement(FindBy.Xpath, string.Format(m_treeItemLocator, value)).Click();
            }
            else
            {

                var selectElement = new SelectElement(select);
                Browser.Current.WaitForElementClickable(selectElement.WrappedElement);
                selectElement.SelectByText(value);
            }
        }

        public void SelectValueFromWrappedDropDownWithArrow(string value)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var select = Browser.FindElement(Locator);

            if (Configurator.BrowserType.Equals("firefox"))
            {
                new UIElement(FindBy.Xpath, Locator.GetStringLocator() + m_selectComboboxLocator).Click();
                new UIElement(FindBy.Xpath, string.Format(m_treeItemLocator, value)).Click();
            }
            else
            {
                var selectElement = new SelectElement(select);
                Browser.Current.WaitForElementClickable(selectElement.WrappedElement);
                selectElement.SelectByText(value);
            }
        }

        public void ClickArrowTypeTextSelectItem(string value)
        {
            new UIElement(FindBy.Xpath, Locator.GetStringLocator() + m_selectComboboxLocator).Click();
            Browser.Current.WaitForPageLoad();
            Browser.Current.WaitForElementClickable(By.XPath(m_searchInput));
            var searchInputs = Browser.FindElements(By.XPath(m_searchInput));
            foreach (var searchInput in searchInputs)
            {
                try
                {
                    searchInput.SendKeys(value);
                    break;
                }
                catch (ElementNotInteractableException e)
                {
                }
            }
            Browser.Current.WaitForPageLoad();
            new UIElement(FindBy.Xpath, string.Format(m_treeItemLocator, value)).Click();
            Browser.Current.WaitForPageLoad();
        }

        public void ClickArrowTypeText(string value)
        {
            new UIElement(FindBy.Xpath, Locator.GetStringLocator() + m_selectComboboxLocator).Click();
            new UIElement(FindBy.Xpath, m_searchInput).SendKeys(value);
        }

        public void SelectValueInNonPrePopulatedDropDown(string value)
        {
            new UIElement(FindBy.Xpath, Locator.GetStringLocator() + m_selectComboboxLocator).SendKeys(value);
            new UIElement(FindBy.Xpath, string.Format(m_treeItemLocator, value)).Click();
        }

        public string GetTextFromSelectedItemInDropDown()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var element = Browser.FindElement(Locator);
            var selectElement = new SelectElement(element);
            var selectedElement = selectElement.AllSelectedOptions.SingleOrDefault();
            var text = selectedElement?.Text;
            return text;
        }

        public string GetValueFromSelectedItemInDropDown()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var element = Browser.FindElement(Locator);
            var selectElement = new SelectElement(element);
            var selectedElement = selectElement.AllSelectedOptions.SingleOrDefault();
            var text = selectedElement?.GetAttribute("value");
            return text;
        }

        public bool IsOptionPresented(string option)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var element = Browser.FindElement(Locator);
            var selectElement = new SelectElement(element);
            var options = selectElement.Options;
            return options.Any(o => o.Text == option);
        }

        public List<string> GetAllValuesFromDropDown()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var element = Browser.FindElement(Locator);
            var selectElement = new SelectElement(element);

            return selectElement.Options.Select(x => x.Text).ToList();
        }
        
        public void SelectValue(string value)
        {
            var webElement = Browser.FindElement(Locator);
            webElement.SendKeys(value);

            Browser.Current.WaitForElementVisible(By.CssSelector(searchResult));
            var searchElement = Browser.FindElement(By.CssSelector(searchResult));

            Browser.Current.WaitForElementVisible(
                By.XPath($"//ul[@class = 'select2-results__options']/li[(contains(.,'{value}'))]"));
            
            searchElement
                .FindElement(By.XPath($"//ul[@class = 'select2-results__options']/li[(contains(.,'{value}'))]"))
                .Click();

            Browser.Current.WaitForPageLoad();
        }

        public void SelectByValue(string value)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var select = Browser.FindElement(Locator);
            var selectElement = new SelectElement(select);
            //Browser.Current.WaitForElementVisible(selectElement.WrappedElement);
            Browser.Current.WaitForElementClickable(selectElement.WrappedElement);
            selectElement.SelectByValue(value);
        }
        
        public void SelectByText(string value)
        {
            Browser.Current.WaitForElementVisible(Locator);
            var select = Browser.FindElement(Locator);
            var selectElement = new SelectElement(select);
            //Browser.Current.WaitForElementVisible(selectElement.WrappedElement);
            Browser.Current.WaitForElementClickable(selectElement.WrappedElement);
            selectElement.SelectByText(value);
        }

        public void ClearSelection()
        {
            new UIElement(FindBy.Xpath, Locator.GetStringLocator() + m_clearButton).Click();
        }

        public string GetPlaceHolderValue()
        {
            var placeholder = new UIElement(FindBy.Xpath, Locator.GetStringLocator() + m_placeholder);

            return placeholder.Text;
        }
    }
}