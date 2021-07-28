﻿namespace WebCore.Elements
{
    public class Container
    {
        private string m_containerName;

        public Container(string containerName)
        {
            m_containerName = containerName;
        }

        public void ClickAddClickToEditItemTemplateButton()
        {
            var locator = $"div[data-name='{m_containerName}'] button.add-item-template";
            new UIElement(FindBy.Css, locator).Click();
        }
    }
}