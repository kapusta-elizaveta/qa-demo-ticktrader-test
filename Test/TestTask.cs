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
        public void DropDownLang_Test()
        {
            Browser.Current.WaitForElementPresent(WebTerminalPage.SignInIcon, 3);
            WebTerminalPage.ControlLanguageIcon.JsClick();

            Assert.Multiple(() =>
            {
                Assert.True(WebTerminalPage.OpenningListLanguage.Displayed, "Language dropdown isn't opened");
                Assert.AreEqual($"\r\n        {engCode}\r\n        {engLan}\r\n    ", WebTerminalPage.OpeningLanguageDropDown(engCode).InnerText, "Field doesn't contains English code and name");
                Assert.AreEqual($"\r\n        {rusCode}\r\n        {rusLan}\r\n    ", WebTerminalPage.OpeningLanguageDropDown(rusCode).InnerText, "Field doesn't contains Russian code and name");

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
                    WebTerminalPage.ControlLanguageIcon.JsClick();
                    WebTerminalPage.OpeningLanguageDropDown(rusCode).Click();
                    CheckingLanguauge(rusLan);
                    break;
                case "Русский":
                    WebTerminalPage.ControlLanguageIcon.JsClick();
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
            Browser.Current.Refresh();
            
            var srcTradingViewFrame = WebTerminalPage.TradingViewFrame.GetAttribute("src");
            var updateSymbol =srcTradingViewFrame.Substring(102, 6);

            Assert.AreEqual(symbol, updateSymbol, "LTCUSD isn't updated");
        }

        [Test]
        [Order(5)]
        public void ChangeTradingTab_Test()
        {
            WebTerminalPage.TradingTab.Click();
            Browser.Current.SwitchToLastWindow();

            var currentWindow = Browser.Current.WrappedDriver.Url;

            Assert.AreEqual("https://demo-ticktrader.free2ex.com/trading", currentWindow, "It isn't trading page");
        }

        public void CheckingLanguauge(string currentLanguage)
        {
            switch (currentLanguage)
            {
                case "English":
                    Assert.Multiple(() =>
                    {
                        Assert.True(WebTerminalPage.MarketWatchTab("Market Watch").Displayed, "Field doesn't contains Market Watch name");
                        Assert.AreEqual("PLACE LIMIT ", WebTerminalPage.PlaceLimitTab.Text, "Field doesn't contains PLACE LIMIT name");
                        Assert.AreEqual("EXCHANGE", WebTerminalPage.ExchangeTab.Text, "Field doesn't contains EXCHANGE name");
                        Assert.AreEqual("Tutorial", WebTerminalPage.TutorialTab.Text, "Field doesn't contains Tutorial name");

                    });
                    break;
                case "Русский":
                    Assert.Multiple(() =>
                    {
                        Assert.True(WebTerminalPage.MarketWatchTab("Обзор рынка").Displayed, "Field doesn't contains Обзор рынка name");
                        Assert.AreEqual("РАЗМЕСТИТЬ LIMIT ", WebTerminalPage.PlaceLimitTab.Text, "Field doesn't contains РАЗМЕСТИТЬ LIMIT name");
                        Assert.AreEqual("ОБМЕНЯТЬ", WebTerminalPage.ExchangeTab.Text, "Field doesn't contains ОБМЕНЯТЬ name");
                        Assert.AreEqual("Обучение", WebTerminalPage.TutorialTab.Text, "Field doesn't contains Обучение name");

                    });
                    break;
            }
        }
    }
}
