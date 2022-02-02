namespace PasswordGen.UITests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenQA.Selenium.Appium.Windows;
    using OpenQA.Selenium.Interactions;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using static Charsets;
    using static Session;

    [TestClass]
    public class HomePageUITests
    {
        private static WindowsElement _passwordTextBlock;
        private static WindowsElement _passwordLengthSlider;
        private static (WindowsElement, string)[] _charsets;
        private static List<WindowsElement> _toggleSwitches;
        private static int _toggleSwitchTestIterations = 16;

        [ClassInitialize]
        public static void InitClass(TestContext context)
        {
            WindowsElement homePage = _client.FindElementByAccessibilityId("HomePage");

            if (!homePage.Selected)
            {
                // Navigate to home page
                homePage.Click();
                Thread.Sleep(500); // Ensure animation is over
            }

            _passwordTextBlock = _client.FindElementByAccessibilityId("PasswordTextBlock");
            _passwordLengthSlider = _client.FindElementByAccessibilityId("PasswordLengthSlider");
            _charsets = new (WindowsElement, string)[]
            {
                (_client.FindElementByAccessibilityId("DigitSwitch"), _digits),
                (_client.FindElementByAccessibilityId("SymbolSwitch"), _symbols),
                (_client.FindElementByAccessibilityId("LowercaseSwitch"), _lowercase),
                (_client.FindElementByAccessibilityId("UppercaseSwitch"), _uppercase),
            };
            _toggleSwitches = new List<WindowsElement>(_charsets.Select(charset => charset.Item1));
        }

        [TestMethod]
        public void TestCopyButton()
        {
            // Arrange
            WindowsElement copyButton = _client.FindElementByAccessibilityId("CopyButton");
            string oldPassword = _passwordTextBlock.Text;

            // Act
            copyButton.Click();

            // Assert
            Assert.AreEqual(oldPassword, _passwordTextBlock.Text);
            Assert.AreEqual(oldPassword, Clipboard.GetText());
        }

        [TestMethod]
        public void TestLengthSlider()
        {
            // Arrange
            string oldPassword = _passwordTextBlock.Text;
            int oldLength = int.Parse(_passwordLengthSlider.Text);
            var actions = new Actions(_client);

            // Act
            actions.MoveToElement(_passwordLengthSlider, _rng.Next(_passwordLengthSlider.Size.Width) + 1, 40);
            actions.Click();
            actions.Perform();
            if (oldLength == int.Parse(_passwordLengthSlider.Text))
            {
                Assert.AreEqual(oldPassword, _passwordTextBlock.Text);
                TestLengthSlider();
                return;
            }

            // Assert
            ValidatePassword(oldPassword);
        }

        [TestMethod]
        public void TestRefreshButton()
        {
            // Arrange
            WindowsElement refreshButton = _client.FindElementByAccessibilityId("RefreshButton");
            string oldPassword = _passwordTextBlock.Text;

            // Act
            refreshButton.Click();

            // Assert
            ValidatePassword(oldPassword);
        }

        [TestMethod]
        public void TestToggleSwitches()
        {
            for (int i = 0; i < _toggleSwitchTestIterations; i++)
            {
                // Arrange
                WindowsElement randomToggleSwitch = _toggleSwitches[_rng.Next(_toggleSwitches.Count)];
                string oldPassword = _passwordTextBlock.Text;

                // Act
                if (randomToggleSwitch.Enabled)
                    randomToggleSwitch.Click();
                else continue;

                // Assert
                ValidatePassword(oldPassword);
            }
        }

        [TestMethod]
        public void TestResetButton()
        {
            #region Arrange
            WindowsElement resetButton = _client.FindElementByAccessibilityId("ResetButton");
            WindowsElement saveButton = _client.FindElementByAccessibilityId("SaveButton");
            string oldPassword = _passwordTextBlock.Text;
            _toggleSwitchTestIterations = 4;

            // Save password settings
            saveButton.Click();
            string savedLength = _passwordLengthSlider.Text;
            bool[] savedToggleSwitchesOn = _toggleSwitches.Select(toggleSwitch => toggleSwitch.Selected).ToArray();

            // Make some random changes
            TestLengthSlider();
            TestToggleSwitches();
            #endregion

            // Act
            resetButton.Click();

            #region Assert
            bool[] toggleSwitchesOn = _toggleSwitches.Select(toggleSwitch => toggleSwitch.Selected).ToArray();

            // Assert that settings are restored
            for (int i = 0; i < toggleSwitchesOn.Length; i++)
                Assert.AreEqual(savedToggleSwitchesOn[i], toggleSwitchesOn[i]);
            Assert.AreEqual(savedLength, _passwordLengthSlider.Text);

            ValidatePassword(oldPassword);
            #endregion

            _toggleSwitchTestIterations = 16;
        }

        private void ValidatePassword(string oldPassword)
        {
            string newPassword = _passwordTextBlock.Text;
            int charsetsOnCount = 0;
            foreach (WindowsElement toggleSwitch in _toggleSwitches)
                charsetsOnCount += toggleSwitch.Selected ? 1 : 0;

            // Validate state of each ToggleSwitch
            Assert.IsTrue(charsetsOnCount > 0);
            if (charsetsOnCount == 1)
                foreach (WindowsElement toggleSwitch in _toggleSwitches)
                    Assert.AreNotEqual(toggleSwitch.Selected, toggleSwitch.Enabled);
            else
                foreach (WindowsElement toggleSwitch in _toggleSwitches)
                    Assert.IsTrue(toggleSwitch.Enabled);

            // Validate new password
            Assert.AreNotEqual(oldPassword, newPassword);

            // Validate password length
            Assert.AreEqual(int.Parse(_passwordLengthSlider.Text), newPassword.Length);

            // Validate password character composition
            foreach ((WindowsElement toggleSwitch, string charset) in _charsets)
                Assert.AreEqual(toggleSwitch.Selected, newPassword.Intersect(charset).Any());
        }
    }
}
