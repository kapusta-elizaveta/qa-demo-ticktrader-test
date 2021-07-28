﻿using OpenQA.Selenium;

namespace WebCore.Elements
{
    public class SpreadSheet : UIElement
    {
        private readonly string m_formulaInput ="//div[@class='k-spreadsheet-formula-input']";

        public SpreadSheet(FindBy type, string locator) : base(type, locator)
        { }

        public bool IsValuePresented()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var countOfElements = Browser.FindElement(Locator).FindElements(By.CssSelector(".k-spreadsheet-data .k-spreadsheet-cell")).Count;
            return countOfElements > 0;
        }

        public void TypeFormula(string formula)
        {
            var formulaLocator = By.XPath(Locator.GetStringLocator() + m_formulaInput);
            Browser.Current.WaitForElementVisible(formulaLocator);
            var element = Browser.FindElement(formulaLocator);
            element.Clear();
            element.SendKeys(formula);
        }
    }
}