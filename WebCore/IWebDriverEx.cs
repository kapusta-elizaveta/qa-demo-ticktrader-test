using OpenQA.Selenium;

namespace WebCore
{
    public interface IWebDriverEx : IWebDriver
    {
        int ProcessId { get; }
    }
}