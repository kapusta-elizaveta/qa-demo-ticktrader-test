using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using WebCore;

namespace Test
{
    [TestFixture]
    class BaseTest
    {
        public Browser _browser;

        [OneTimeTearDown]
        public void TearDown()
        {
            Browser.Current.Dispose();
            Browser.Current.WrappedDriver.Quit();
            Browser.KillCurrent();
        }
    }
}
