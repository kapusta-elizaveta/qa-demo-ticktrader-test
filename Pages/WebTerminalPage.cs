using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCore;
using WebCore.Elements;

namespace Pages
{
    public class WebTerminalPage
    {
        public static UIElement SignInIcon => new UIElement(FindBy.PartialLinkText, "Sign");
        public static UIElement ControlLanguageIcon => new UIElement(FindBy.Xpath, "//span[@id='list-language']/span");
        public static DropDown ListLanguage => new DropDown(FindBy.Xpath, "//select[@id='list-language']");
        public static UIElement CurrentLanguage => new UIElement(FindBy.Xpath, "//div[@class='header__language']//span[@class='k-input _text-over-hidden']");
        public static UIElement LanguagesOfDropDown(string lanName, string lanCode) => new UIElement(FindBy.Xpath,
            $"//option[@value='{lanCode}'][text()='{lanName}']");
        public static UIElement ExchangeTab => new UIElement(FindBy.Id, "exchange");
        public static UIElement TutorialTab => new UIElement(FindBy.Id, "tutorial-steps");
        public static UIElement MarketWatchTab(string name) => new UIElement(FindBy.Xpath, $"//span[text()='{name}']");
        public static UIElement PlaceLimitTab => new UIElement(FindBy.Id, "advanced-limit-trade");
        public static UIElement ListSymbolsDropDown => new UIElement(FindBy.Xpath, "//span[@id='list-symbols']");
        public static UIElement SymbolsDropDown(string symbol) => new UIElement(FindBy.Xpath, $"//span[@data-n='{symbol}']");
        public static UIElement TradingTab => new UIElement(FindBy.Xpath, "//ul[@id='menu']//a[@href='/trading']");
        public static UIElement OpenningListLanguage => new UIElement(FindBy.Id, "list-language_listbox");
        public static UIElement OpeningLanguageDropDown(String lanCode) => new UIElement(FindBy.Xpath, $"//a[@data-code='{lanCode}']");
        public static UIElement TradingViewFrame => new UIElement(FindBy.Xpath, "//div[@id='trading-chart']/iframe");
    }
}
