namespace PasswordGen.UITests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenQA.Selenium.Appium.Windows;
    using OpenQA.Selenium.Interactions;

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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

        private string GetRandomString()
        {
            int randomLength = _rng.Next(1, 96);
            var randomString = new StringBuilder(randomLength);

            for (int i = 0; i < randomLength; i++)
            {
                char randomChar;

                // Control characters are avoided to simplify the test
                do randomChar = (char) _rng.Next(256);
                while (char.IsControl(randomChar));
                randomString.Append(randomChar);
            }
            return randomString.ToString();
        }

        [TestMethod]
        public void TestIncludeTextBox()
        {
            // Arrange
            string oldPassword = GetPassword();
            string randomString = GetRandomString();
            Clipboard.SetText(randomString, TextDataFormat.UnicodeText);
            _includeTextBox.Clear();

            // Act
            _includeTextBox.SendKeys("\uE009v"); // Paste (Ctrl-V)

            // Assert
            Assert.AreEqual(string.Join(null, randomString.Intersect(_ascii).Except(_excludeTextBox.Text)), _includeTextBox.Text);
            ValidatePassword(oldPassword);
        }

        [TestMethod]
        public void TestExcludeTextBox()
        {
            // Arrange
            string oldPassword = GetPassword();
            string randomString = GetRandomString();
            Clipboard.SetText(randomString, TextDataFormat.UnicodeText);
            _excludeTextBox.Clear();

            // Act
            _excludeTextBox.SendKeys("\uE009v"); // Paste (Ctrl-V)

            // Assert
            Assert.AreEqual(string.Join(null, randomString.Intersect(_ascii).Except(_includeTextBox.Text)), _excludeTextBox.Text);
            ValidatePassword(oldPassword);
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
            if (oldPassword == string.Empty || oldPassword == NULL_CHARSET_MESSAGE)
            {
                Assert.IsFalse(copyButton.Enabled);
                TestLengthSlider(); // Get non zero length
                _charsetSwitches[_rng.Next(_charsetSwitches.Count)].Click(); // Get non empty charset
                TestCopyButton(); // Retest
                return;
            }
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
            if (int.Parse(_passwordLengthSlider.Text) == 0)
            {
                Assert.AreEqual(_passwordTextBlock.Text, string.Empty);
                TestLengthSlider();
                return;
            }
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
            if (oldPassword == string.Empty || oldPassword == NULL_CHARSET_MESSAGE)
            {
                Assert.IsFalse(refreshButton.Enabled);
                TestLengthSlider(); // Get non zero length
                _charsetSwitches[_rng.Next(_charsetSwitches.Count)].Click(); // Get non empty charset
                TestRefreshButton(); // Retest
                return;
            }
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
                randomCharsetSwitch.Click();

                // Assert
                ValidatePassword(oldPassword);
            }
        }

        [TestMethod]
        public void TestMinNumberBoxes()
        {
            for (int i = 0; i < 16; i++)
            {
                // Arrange
                string oldPassword = GetPassword();
                WindowsElement randomCharsetMin = _charsets[_rng.Next(_charsets.Length)].Item2;
                if (!randomCharsetMin.Enabled) continue;

                // Act
                randomCharsetMin.SendKeys($"{_rng.Next(-20, 100)}\uE007"); // \uE007 is the Enter key 

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
            Thread.Sleep(10); // Wait for the password to refresh
            string password = GetPassword();
            bool charsetIsEmpty = true;

            // Check empty charset
            foreach (var (charsetSwitch, _, fullCharset) in _charsets)
            {
                IEnumerable<char> charset = charsetSwitch.Selected
                    ? fullCharset.Except(_excludeTextBox.Text)
                    : fullCharset.Intersect(_includeTextBox.Text);

                if (charset.Any())
                {
                    charsetIsEmpty = false;
                    break;
                }
            }
            if (charsetIsEmpty)
            {
                Assert.AreEqual(password, NULL_CHARSET_MESSAGE);
                return;
            }

            // Validate password character composition
            foreach (var (charsetSwitch, charsetMin, fullCharset) in _charsets)
            {
                IEnumerable<char> charset = charsetSwitch.Selected
                    ? fullCharset.Except(_excludeTextBox.Text)
                    : fullCharset.Intersect(_includeTextBox.Text);

                if (charset.Any())
                    Assert.IsTrue(password.Count(passwordChar => charset.Contains(passwordChar)) >= int.Parse(charsetMin.Text));
                Assert.IsFalse(password.Intersect(fullCharset.Except(charset)).Any());
            }

            // Validate password length
            Assert.AreEqual(int.Parse(_passwordLengthSlider.Text), password.Length);
        }

        /// <summary>
        /// Returns the password with all ZWSP characters removed.
        /// </summary>
        private string GetPassword() => _passwordTextBlock.Text.Replace(ZWSP.ToString(), string.Empty);
    }
}
