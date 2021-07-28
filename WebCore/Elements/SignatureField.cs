﻿using OpenQA.Selenium;

namespace WebCore.Elements
{
    public class SignatureField : UIElement
    {
        public SignatureField(FindBy type, string locator) : base(type, locator)
        { }

        public string GetTextOfSignature()
        {
            Browser.Current.WaitForElementVisible(Locator);
            var text = Browser.FindElement(Locator).FindElement(By.CssSelector("p.typed")).Text;
            return text;
        }
    }
}