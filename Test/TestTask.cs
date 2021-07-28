using NUnit.Framework;
using Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using WebCore;

namespace Test
{
    [TestFixture]
    class TestTask
    {
        private string engCode = "en";
        private string rusCode = "ru";
        private string engLan = "English";
        private string rusLan = "Русский";

        [OneTimeSetUp]
        public void SetUp()
        {
            Browser.Current.GoTo(Configurator.BaseUrl);
        }

        [Test]
        [Order(1)]
        public void DropDownLang()
        {
            Browser.Current.WaitForElementPresent(WebTerminalPage.SignInIcon, 3);
            WebTerminalPage.ControlLanguageIcon.JsClick();

            Assert.Multiple(() =>
            {
                Assert.True(WebTerminalPage.OpenningListLanguage.Enabled, "");
                Assert.AreEqual("\r\n        "+ engCode + "ru\r\n        " + engLan + "\r\n    ", WebTerminalPage.OpeningLanguageDropDown(engCode).InnerText, "");
                Assert.AreEqual($"\r\n        " + rusCode + "\r\n        " + rusLan+ "\r\n    ", WebTerminalPage.OpeningLanguageDropDown(rusCode).InnerText, "");

            });
        }

        [Test]
        [Order(2)]
        public void Language_Test()
        {
            var currentLanguage = WebTerminalPage.CurrentLanguage.GetAttribute("title");

            CheckingLanguauge(currentLanguage); 
        }

        [Test]
        [Order(3)]
        public void ChangeLanguage_Test()
        {
            var currentLanguage = WebTerminalPage.CurrentLanguage.GetAttribute("title");

            switch (currentLanguage)
            {
                case "English":
                    // WebTerminalPage.ControlLanguageIcon.JsClick();
                    WebTerminalPage.OpeningLanguageDropDown(rusCode).Click();
                    CheckingLanguauge(rusLan);
                    break;
                case "Русский":
                    // WebTerminalPage.ControlLanguageIcon.JsClick();
                    WebTerminalPage.OpeningLanguageDropDown(engCode).Click();
                    CheckingLanguauge(engLan);
                    break;
            }
        }

        [Test]
        [Order(4)]
        public void ChangeSymbol_Test()
        {
            var symbol = "LTCUSD";
            Browser.Current.WaitForPageLoad();
            WebTerminalPage.ListSymbolsDropDown.JsClick();
            WebTerminalPage.SymbolsDropDown(symbol).Click();
            Browser.Current.WaitForPageLoad();
            Browser.Current.WaitForElementVisible(WebTerminalPage.TradingViewFrame);
            Browser.Current.WrappedDriver.SwitchTo().Frame("tradingview_ea6bf");

            var srcTradingViewFrame = WebTerminalPage.TradingViewFrame.GetAttribute("src");
            var result = srcTradingViewFrame.Split('=').Last().Split('&')[0];
            
            Assert.AreEqual(symbol, result, "");
        }

        [Test]
        [Order(5)]
        public void ChangeTradingTab_Test()
        {
            WebTerminalPage.TradingTab.Click();
            Browser.Current.SwitchToLastWindow();

            var currentWindow = Browser.Current.WrappedDriver.Url;

            Assert.AreEqual("https://demo-ticktrader.free2ex.com/trading", currentWindow, "");
        }

        public void CheckingLanguauge(string currentLanguage)
        {
            switch (currentLanguage)
            {
                case "English":
                    Assert.Multiple(() =>
                    {
                        Assert.True(WebTerminalPage.MarketWatchTab("Market Watch").Displayed, "");
                        Assert.AreEqual("PLACE LIMIT ", WebTerminalPage.PlaceLimitTab.Text, "");
                        Assert.AreEqual("EXCHANGE", WebTerminalPage.ExchangeTab.Text, "");
                        Assert.AreEqual("Tutorial", WebTerminalPage.TutorialTab.Text, "");

                    });
                    break;
                case "Русский":
                    Assert.Multiple(() =>
                    {
                        Assert.True(WebTerminalPage.MarketWatchTab("Обзор рынка").Displayed, "");
                        Assert.AreEqual("РАЗМЕСТИТЬ LIMIT ", WebTerminalPage.PlaceLimitTab.Text, "");
                        Assert.AreEqual("ОБМЕНЯТЬ", WebTerminalPage.ExchangeTab.Text, "");
                        Assert.AreEqual("Обучение", WebTerminalPage.TutorialTab.Text, "");

                    });
                    break;
            }
        }
    }
}
