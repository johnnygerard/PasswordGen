namespace PasswordGen.UITests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenQA.Selenium.Appium;
    using OpenQA.Selenium.Appium.Windows;

    using System;
    using System.Diagnostics;

    [TestClass]
    public class Session
    {
        private const string APP_ID = "50409JohnnyGrard.PasswordGen_pfjsvnw5qhb7g!App";
        private const string WIN_APP_DRIVER_URL = "http://127.0.0.1:4723";
        private const string WIN_APP_DRIVER_PATH = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";

        internal static WindowsDriver<WindowsElement> _client;
        internal static readonly Random _rng = new Random();
        private static Process _winAppDriver;

        [AssemblyInitialize]
        public static void Setup(TestContext context)
        {
            var options = new AppiumOptions();

            _winAppDriver = Process.Start(new ProcessStartInfo(WIN_APP_DRIVER_PATH)
            {
                UseShellExecute = false,
            });
            options.AddAdditionalCapability("app", APP_ID);
            _client = new WindowsDriver<WindowsElement>(new Uri(WIN_APP_DRIVER_URL), options);
            _client.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);
        }

        [AssemblyCleanup]
        public static void TearDown()
        {
            _client.Quit();
            using (_winAppDriver)
            {
                _winAppDriver.Kill();
                _winAppDriver.WaitForExit();
            }
        }
    }
}
