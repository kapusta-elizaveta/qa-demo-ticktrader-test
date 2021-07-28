﻿using OpenQA.Selenium;

namespace WebCore
{
    public static class ByExtension
    {
        public static string GetStringLocator(this By by)
        {
            int indexOfBackspace = by.ToString().IndexOf(' ');
            string locator = by.ToString().Substring(indexOfBackspace + 1);
            return locator;
        }
    }
}