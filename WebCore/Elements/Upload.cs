﻿using OpenQA.Selenium;
using System.Collections.Generic;

namespace WebCore.Elements
{
    public class Upload : UIElement
    {
        public Upload(FindBy type, string locator) : base(type, locator)
        {
            try
            {
                Browser.Current.WaitForElementPresent(Locator);
                Browser.Current.ScrollToElement(Locator);
                Element = GetElement();
            }
            catch (NoSuchElementException) {}
        }

        public void UploadFile(FileUtils path)
        {
            Browser.Current.UploadFiles(
                path,
                Browser.FindElement(Locator)
            );
        }

        public void UploadFilesWithoutChecking(FileUtils path)
        {
            Browser.Current.UploadFilesWithoutChecking(
                path,
                Browser.FindElement(Locator)
            );
        }

        public bool IsFileUploaded()
        {
            var id = Element.GetCssValue("id");
            var file = By.XPath($"//div[@data-field-code='{id}']//li");
            Browser.Current.WaitForElementVisible(file, 3);
            var files = Browser.FindElements(file);

            return files.Count > 0;
        }

        public bool IsFileUploaded(string fileName)
        {
            var id = Element.GetAttribute("id");
            var file = new UIElement(FindBy.Xpath, $"//div[@data-field-code='{id}']//li//span[text()='{fileName}']");

            return file.Displayed;
        }
        
        public bool IsFilePresent(string fileName)
        {
            var id = Element.GetAttribute("id");
            var file = new UIElement(FindBy.Xpath, $"//div[@data-field-code='{id}']//li//span[text()='{fileName}']");

            return file.Presented;
        }

        public bool IsValidationMessagePresented(string text)
        {
            var id = Element.GetAttribute("id");
            var message = new UIElement(
                FindBy.Xpath,
                $"//div[@data-field-code='{id}']//span[@class='k-file-validation-message' and text()='{text}']");

            return message.Displayed;
        }

        public List<string> GetNamesOfUploadedReadOnlyFiles()
        {
            var uploadedFilesLocator =
                By.XPath(Locator.GetStringLocator() + "//ancestor::div[contains(@id, 'form-element-wrapper')]//a");
            Browser.Current.WaitForElementVisible(uploadedFilesLocator);
            var elements = Browser.FindElements(uploadedFilesLocator);

            List<string> names = new List<string>();
            foreach (var element in elements)
            {
                names.Add(element.Text);
            }

            return names;
        }

        public List<string> GetNamesOfUploadedFiles()
        {
            var uploadedFilesLocator =
                By.XPath(Locator.GetStringLocator() + "//ancestor::div[contains(@id, 'form-element-wrapper')]//span[@class='k-file-name']");
            Browser.Current.WaitForElementVisible(uploadedFilesLocator);
            var elements = Browser.FindElements(uploadedFilesLocator);

            List<string> names = new List<string>();
            foreach (var element in elements)
            {
                names.Add(element.Text);
            }

            return names;
        }

        public void DeleteFile(string fileName)
        {
            var id = Browser.FindElement(Locator).GetAttribute("id");
            var file = By.XPath($"//div[@data-field-code='{id}']//li[span//span[@title='{fileName}']]//button");
            Browser.Current.WaitForElementVisible(file, 3);
            var button = Browser.FindElement(file);
            button.Click();
        }
    }
}