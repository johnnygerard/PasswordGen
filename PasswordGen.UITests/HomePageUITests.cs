namespace PasswordGen.UITests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenQA.Selenium.Appium.Windows;
    using OpenQA.Selenium.Interactions;

    using System.Diagnostics;
    using System.Threading;
    using System.Windows;

    using static Session;

    [TestClass]
    public class HomePageUITests
    {
        [ClassInitialize] // Navigate to home page
        public static void InitClass(TestContext context)
        {
            _client.FindElementByAccessibilityId("HomePage").Click();
            Thread.Sleep(500); // Ensure animation is over
        }

        [TestMethod]
        public void TestCopyButton()
        {
            // Arrange
            WindowsElement copyButton = _client.FindElementByAccessibilityId("CopyButton");
            WindowsElement passwordTextBlock = _client.FindElementByAccessibilityId("PasswordTextBlock");

            // Act
            copyButton.Click();

            // Assert
            Assert.AreEqual(passwordTextBlock.Text, Clipboard.GetText());
        }

        [TestMethod]
        public void TestLengthSlider()
        {
            // Arrange
            WindowsElement passwordLengthSlider = _client.FindElementByAccessibilityId("PasswordLengthSlider");
            WindowsElement passwordTextBlock = _client.FindElementByAccessibilityId("PasswordTextBlock");
            var actions = new Actions(_client);

            // Act
            actions.MoveToElement(passwordLengthSlider, _rng.Next(passwordLengthSlider.Size.Width) + 1, 40);
            actions.Click();
            actions.Perform();

            // Assert
            Assert.AreEqual(passwordTextBlock.Text.Length, int.Parse(passwordLengthSlider.Text));
        }
    }
}
