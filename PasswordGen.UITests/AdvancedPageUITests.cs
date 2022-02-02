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
    public class AdvancedPageUITests
    {
        private const char ZWSP = '\u200B'; // ZERO WIDTH SPACE
        private const string NULL_CHARSET_MESSAGE = "CHARACTER SET EMPTY";

        private static WindowsElement _passwordTextBlock;
        private static WindowsElement _passwordLengthSlider;
        private static WindowsElement _includeTextBox;
        private static WindowsElement _excludeTextBox;
        private static (WindowsElement, WindowsElement, string)[] _charsets;
        private static List<WindowsElement> _charsetSwitches;
        private static int _charsetSwitchTestIterations = 16;

        [ClassInitialize]
        public static void InitClass(TestContext context)
        {
            WindowsElement advancedPage = _client.FindElementByAccessibilityId("AdvancedPage");

            if (!advancedPage.Selected)
            {
                // Navigate to home page
                advancedPage.Click();
                Thread.Sleep(500); // Ensure animation is over
            }

            _passwordTextBlock = _client.FindElementByAccessibilityId("PasswordTextBlock");
            _passwordLengthSlider = _client.FindElementByAccessibilityId("PasswordLengthSlider");
            _includeTextBox = _client.FindElementByAccessibilityId("IncludeTextBox");
            _excludeTextBox = _client.FindElementByAccessibilityId("ExcludeTextBox");
            _charsets = new (WindowsElement, WindowsElement, string)[]
            {
                (_client.FindElementByAccessibilityId("DigitSwitch"), _client.FindElementByAccessibilityId("DigitMin"), _digits),
                (_client.FindElementByAccessibilityId("SymbolSwitch"), _client.FindElementByAccessibilityId("SymbolMin"), _symbols),
                (_client.FindElementByAccessibilityId("LowercaseSwitch"), _client.FindElementByAccessibilityId("LowercaseMin"), _lowercase),
                (_client.FindElementByAccessibilityId("UppercaseSwitch"), _client.FindElementByAccessibilityId("UppercaseMin"), _uppercase),
            };
            _charsetSwitches = new List<WindowsElement>(_charsets.Select(charset => charset.Item1));
        }

        [TestMethod]
        public void TestCopyButton()
        {
            // Arrange
            WindowsElement copyButton = _client.FindElementByAccessibilityId("CopyButton");
            string oldPassword = GetPassword();

            // Act
            copyButton.Click();

            // Assert
            Assert.AreEqual(oldPassword, GetPassword());
            Assert.AreEqual(oldPassword, Clipboard.GetText());
        }

        [TestMethod]
        public void TestLengthSlider()
        {
            // Arrange
            string oldPassword = GetPassword();
            int oldLength = int.Parse(_passwordLengthSlider.Text);
            var actions = new Actions(_client);

            // Act
            actions.MoveToElement(_passwordLengthSlider, _rng.Next(_passwordLengthSlider.Size.Width) + 1, 40);
            actions.Click();
            actions.Perform();
            if (oldLength == int.Parse(_passwordLengthSlider.Text))
            {
                Assert.AreEqual(oldPassword, GetPassword());
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
            string oldPassword = GetPassword();

            // Act
            refreshButton.Click();

            // Assert
            ValidatePassword(oldPassword);
        }

        [TestMethod]
        public void TestCharsetSwitches()
        {
            for (int i = 0; i < _charsetSwitchTestIterations; i++)
            {
                // Arrange
                WindowsElement randomCharsetSwitch = _charsetSwitches[_rng.Next(_charsetSwitches.Count)];
                string oldPassword = GetPassword();

                // Act
                if (randomCharsetSwitch.Enabled)
                    randomCharsetSwitch.Click();
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
            string oldPassword = GetPassword();
            _charsetSwitchTestIterations = 4;

            // Save password settings
            saveButton.Click();
            string savedLength = _passwordLengthSlider.Text;
            bool[] savedCharsetSwitchesOn = _charsetSwitches.Select(charsetSwitch => charsetSwitch.Selected).ToArray();

            // Make some random changes
            TestLengthSlider();
            TestCharsetSwitches();
            #endregion

            // Act
            resetButton.Click();

            #region Assert
            bool[] charsetSwitchesOn = _charsetSwitches.Select(charsetSwitch => charsetSwitch.Selected).ToArray();

            // Assert that settings are restored
            for (int i = 0; i < charsetSwitchesOn.Length; i++)
                Assert.AreEqual(savedCharsetSwitchesOn[i], charsetSwitchesOn[i]);
            Assert.AreEqual(savedLength, _passwordLengthSlider.Text);

            ValidatePassword(oldPassword);
            #endregion

            _charsetSwitchTestIterations = 16;
        }

        private void ValidatePassword(string oldPassword)
        {
            string password = GetPassword();

            // Validate new password
            Assert.AreNotEqual(oldPassword, password);

            // Validate password length
            Assert.AreEqual(int.Parse(_passwordLengthSlider.Text), password.Length);

            // Validate password character composition
            foreach ((WindowsElement charsetSwitch, WindowsElement charsetMin, string fullCharset) in _charsets)
            {
                IEnumerable<char> charset = charsetSwitch.Selected
                    ? fullCharset.Except(_excludeTextBox.Text)
                    : fullCharset.Intersect(_includeTextBox.Text);

                if (charset.Any())
                    Assert.IsTrue(password.Count(passwordChar => charset.Contains(passwordChar)) >= int.Parse(charsetMin.Text));
                Assert.IsFalse(password.Intersect(fullCharset.Except(charset)).Any());
            }
        }

        /// <summary>
        /// Returns the password with all ZWSP characters removed.
        /// </summary>
        private string GetPassword() => _passwordTextBlock.Text.Replace(ZWSP.ToString(), string.Empty);
    }
}
