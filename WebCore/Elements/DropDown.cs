﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace WebCore.Elements
{
    public class DropDown : UIElement
    {
        public DropDown(FindBy type, string locator) : base(type, locator)
        {
        }

    }
}