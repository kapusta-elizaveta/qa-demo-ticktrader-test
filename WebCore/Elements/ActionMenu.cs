using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace WebCore.Elements
{
    public class ActionMenu
    {
        private readonly string m_mainLocator = "//span[@role='application']";

        public void ClickOnMenu(string menuName)
        {
            var actionMenuLocator = m_mainLocator + $"//span[normalize-space()='{menuName}']";
            Browser.Current.WaitForElementClickable(By.XPath(actionMenuLocator));

            new Actions(Browser.Current.WrappedDriver)
                .MoveToElement(new UIElement(FindBy.Xpath, actionMenuLocator).WebElement).Click().Build().Perform();
            
            Browser.Current.WaitForPageLoad();
        }

        public bool IsMenuPresented(string menuName)
        {
            var actionMenuLocator = m_mainLocator + $"//span[contains(text(), '{menuName}')]";
            Browser.Current.WaitForElementPresent(By.XPath(actionMenuLocator));
            return new UIElement(FindBy.Xpath, actionMenuLocator).Presented;
        }

        public bool IsMenuHyperlink(string menuName)
        {
            Browser.Current.WaitForElementVisible(By.XPath(m_mainLocator + $"//span[normalize-space()='{menuName}']"));
            var actionMenuLocator = m_mainLocator + $"//span[contains(text(), '{menuName}')]/ancestor::a";
            var isLinkPresented = Browser.FindElements(By.XPath(actionMenuLocator)).Count > 0;
            if (isLinkPresented)
                return Browser.FindElement(By.XPath(actionMenuLocator)).GetAttribute("href")
                    .Contains("/portal/showworkflow/");
            return false;
        }
    }
}