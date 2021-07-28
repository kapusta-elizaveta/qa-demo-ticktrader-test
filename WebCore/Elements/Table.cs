﻿using OpenQA.Selenium;

namespace WebCore.Elements
{
    public class Table : UIElement
    {
        public Table(FindBy type, string locator) : base(type, locator)
        { }

        public int CountOfPopulatedCells()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var countOfElements = Browser.FindElement(Locator).FindElements(By.XPath("//*[contains(@name, 'element') and string-length(normalize-space(text())) > 0]")).Count;
            return countOfElements;
        }
    }
}