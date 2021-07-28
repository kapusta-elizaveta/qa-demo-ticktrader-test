﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace WebCore.Elements
{
    public class TableElement
    {
        private readonly string m_id;
        private readonly Dictionary<int, List<string>> m_rowValues = new Dictionary<int, List<string>>();
        private readonly int m_columnValueCount;

        public TableElement(string id,
            int columnValueCount)
        {
            m_id = id;
            m_columnValueCount = columnValueCount;
        }

        public List<IWebElement> GetAllRows()
        {
            return new UIElement(
                FindBy.Xpath,
                $"//div[@id='{m_id}']//tbody//tr").FindElements();
        }


        public List<IWebElement> GetColumnValues(int columnIndex)
        {
            return new UIElement(
                FindBy.Xpath,
                $"//div[@id='{m_id}']//tbody//tr//td[not(contains(@style,'none'))][{columnIndex}]").FindElements();
        }

        public UIElement GetLastColumnValue(int columnIndex)
        {
            return new UIElement(
                FindBy.Xpath,
                $"//div[@id='{m_id}']//tbody//tr[1]//td[not(contains(@style,'none'))][{columnIndex}]");
        }

        public Dictionary<int, List<string>> GetRowValues()
        {
            var allRows = GetAllRows();
            for (var i = 1; i < allRows.Count + 1; i++)
            {
                var listAllValues = new List<string>();
                for (var j = 1; j < m_columnValueCount + 1; j++)
                {
                    var value = new UIElement(
                        FindBy.Xpath,
                        $"//div[@id='{m_id}']//tbody//tr[{i}]//td[not(contains(@style,'none'))][{j}]").Text;

                    listAllValues.Add(value);
                }

                m_rowValues.Add(i, listAllValues);
            }

            return m_rowValues;
        }

        public UIElement GetRow(List<string> rowValues)
        {
            var sb = new StringBuilder();
            string row;
            if (rowValues == null || rowValues.Count == 0)
            {
                throw new ArgumentNullException(nameof(rowValues));
            }

            if (rowValues.Count == 1)
            {
                row = $"//div[@id='{m_id}']//tbody//tr[td='{rowValues[0]}']";
            }
            else
            {
                foreach (var t in rowValues.Skip(1))
                {
                    sb.Append($"and td='{t}'");
                }

                row = $"//div[@id='{m_id}']//tbody//tr[td='{rowValues[1]}'{sb}]";
            }

            return new UIElement(FindBy.Xpath, row);
        }

        public string GetLastValue(int columnIndex) => new UIElement(
            FindBy.Xpath,
            $"//div[@id='{m_id}']//tbody//tr//td[not(contains(@style,'none'))][{columnIndex}]").Text;

        public UIElement GetRowByCell(int columnIndex,
            string cellValue) => new UIElement(
            FindBy.Xpath,
            ""
            + $"//div[@id='{m_id}']//tbody//tr//td[text()='{cellValue}'][{columnIndex}]");

        public bool IsRowExists(List<string> rowValues)
        {
            var row = GetRow(rowValues);
            return Browser.Current.WaitForElementPresent(row) == null;
        }

        public void RemoveItem(List<string> rowValues)
        {
            var sb = new StringBuilder();

            foreach (var t in rowValues.Skip(1))
            {
                sb.Append($"and td='{t}'");
            }

            new UIElement(
                FindBy.Xpath,
                $"//div[@id='{m_id}']//tbody//tr[td='{rowValues[1]}'{sb}]//div[@title='Remove']").Click();
            Browser.Current.WaitForPageLoad();
        }
    }
}