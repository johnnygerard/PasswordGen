namespace PasswordGen.Pages
{
    using Microsoft.UI.Xaml.Controls;

    using PasswordGen.Utilities;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Windows.ApplicationModel.DataTransfer;
    using Windows.Globalization.NumberFormatting;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    using static SettingsPage;
    using static Utilities.Charsets;
    using static Utilities.PasswordBuilder;

    public sealed partial class AdvancedPage : Page
    {
        // Throttle delay of 4 ms to match a monitor refresh rate of 240 Hz.
        private const int THROTTLE_DELAY = 4;
        private const string DEBUG = nameof(DEBUG);
        private const string TEST = nameof(TEST);
        private const char ZWSP = '\u200B'; // ZERO WIDTH SPACE
        private const string CHARSET_EMPTY_MESSAGE = "CHARACTER SET EMPTY";

        private readonly ReadOnlyCollection<ToggleButton> _toggleButtons;
        private readonly ReadOnlyDictionary<string, NumberBox> _charsetMins;
        private readonly Dictionary<string, PasswordDataEntry> _passwordData;
        private readonly HashSet<char> _mainCharset;
        private string _password;

        public AdvancedPage()
        {
            var numberFormatter = new DecimalFormatter
            {
                NumberRounder = new IncrementNumberRounder { RoundingAlgorithm = RoundingAlgorithm.RoundTowardsZero },
                FractionDigits = 0,
            };

            InitializeComponent();
            _toggleButtons = new ReadOnlyCollection<ToggleButton>(new ToggleButton[]
            {
                DigitSwitch,
                SymbolSwitch,
                LowercaseSwitch,
                UppercaseSwitch,
            });
            _charsetMins = new ReadOnlyDictionary<string, NumberBox>(new Dictionary<string, NumberBox>
            {
                { DIGITS, DigitMin },
                { SYMBOLS, SymbolMin },
                { LOWERCASE, LowercaseMin },
                { UPPERCASE, UppercaseMin },
            });
            _passwordData = GetInitialPasswordData(out _mainCharset);

            // Set NumberBox integer format.
            foreach (NumberBox charsetMin in _charsetMins.Values)
                charsetMin.NumberFormatter = numberFormatter;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach event handlers
            foreach (var toggleButton in _toggleButtons)
            {
                toggleButton.Checked += ToggleButton_Checked;
                toggleButton.Unchecked += ToggleButton_Unchecked;
            }
            foreach (NumberBox charsetMin in _charsetMins.Values)
                charsetMin.ValueChanged += CharsetMinNumberBox_ValueChanged;
            IncludeTextBox.TextChanging += IncludeTextBox_TextChanging;
            ExcludeTextBox.TextChanging += ExcludeTextBox_TextChanging;
            PasswordLengthSlider.ValueChanged += PasswordLengthSlider_ValueChanged;

            ApplyUserSettings();
            RefreshPassword();
            SimulateUserInput();

            Loaded -= Page_Loaded; // Execute once
        }

        [Conditional(TEST)]
        private async void SimulateUserInput()
        {
            var rng = new Random();
            var stopwatch = Stopwatch.StartNew();
            int userActionsCount = 0;

            while (stopwatch.ElapsedMilliseconds < 5000)
            {
                switch (rng.Next(9))
                {
                    case 0:
                        PasswordLengthSlider.Value = rng.Next((int) PasswordLengthSlider.Minimum, (int) PasswordLengthSlider.Maximum + 1);
                        break;
                    case 1:
                        var toggleButton = _toggleButtons[rng.Next(_toggleButtons.Count)];

                        if (toggleButton.IsEnabled)
                            toggleButton.IsChecked = !toggleButton.IsChecked;
                        break;
                    case 2:
                        if (CopyButton.IsEnabled)
                            CopyButton_Click(null, null);
                        break;
                    case 3:
                        if (RefreshButton.IsEnabled)
                            RefreshButton_Click(null, null);
                        break;
                    case 4:
                        ResetButton_Click(null, null);
                        break;
                    case 5:
                        SaveSettings_Click(null, null);
                        break;
                    case 6:
                        IncludeTextBox.Text = GetRandomString(rng.Next(1, 95));
                        break;
                    case 7:
                        ExcludeTextBox.Text = GetRandomString(rng.Next(1, 95));
                        break;
                    case 8:
                        NumberBox randomCharsetMin = _charsetMins[_charsetKeys[rng.Next(4)]];

                        if (randomCharsetMin.IsEnabled)
                            randomCharsetMin.Value = rng.Next((int) randomCharsetMin.Maximum + 1);
                        break;
                }
                userActionsCount++;
                await Task.Delay(1);
            }
            stopwatch.Stop();
            Debug.WriteLine($"userActionsCount: {userActionsCount}");

            string GetRandomString(int length)
            {
                var randomString = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                {
                    randomString.Append((char) rng.Next(128));
                }
                return randomString.ToString();
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e) => ToggleButton_Toggled(sender, true);
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e) => ToggleButton_Toggled(sender, false);

        private void ToggleButton_Toggled(object sender, bool isChecked)
        {
            var toggleButton = (ToggleButton) sender;
            var charsetKey = (string) toggleButton.Tag;
            NumberBox charsetMin = _charsetMins[charsetKey];

            if (isChecked)
            {
                UpdatePasswordDataCharset(ExcludeTextBox.Text, charsetKey, false);
                if (!charsetMin.IsEnabled)
                    TurnOn(charsetMin);
            }
            else
            {
                UpdatePasswordDataCharset(IncludeTextBox.Text, charsetKey, true);
                if (!_passwordData[charsetKey].Charset.Any())
                    TurnOff(charsetMin);
            }
            RefreshPassword();
        }

        private void TurnOn(NumberBox charsetMin)
        {
            charsetMin.IsEnabled = true;
            charsetMin.Value = 0; // event handler suppressed becaused OldValue is NaN

            // invariant #2
            charsetMin.Maximum = PasswordLengthSlider.Value - PasswordLengthSlider.Minimum;
        }

        private void TurnOff(NumberBox charsetMin)
        {
            charsetMin.Value = 0;
            charsetMin.IsEnabled = false;

            // User input validation will be skipped because charsetMin is disabled
            charsetMin.Value = double.NaN;
        }

        private void CharsetMinNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (double.IsNaN(args.NewValue))
            {
                // Cancel NaN value set from user input.
                if (sender.IsEnabled)
                    sender.Value = args.OldValue;
                return;
            }
            if (double.IsNaN(args.OldValue))
                return;

            sender.Value = (int) args.NewValue; // Remove any possible fractional part from user input.
            MaintainInvariants(sender, (int) args.NewValue - (int) args.OldValue);
            RefreshPassword();
        }

        /*  There are 2 invariants to maintain:
            1.  minimum password length EQUALS sum of enabled charset minimums
            2.  FOREACH enabled charset:
                    password length - password minimum EQUALS charsetMin maximum - charsetMin value     */
        private void MaintainInvariants(NumberBox sender, int delta)
        {
            // invariant #1
            PasswordLengthSlider.Minimum += delta;

            // invariant #2
            foreach (NumberBox charsetMin in _charsetMins.Values.Where(charsetMin => charsetMin.IsEnabled && charsetMin != sender))
                charsetMin.Maximum -= delta;

            // Update _passwordData
            _passwordData[(string) sender.Tag].Length += delta;
            _passwordData[MAIN_CHARSET].Length -= delta;
        }

        private void UpdatePasswordDataCharset(IEnumerable<char> charset, string charsetKey, bool included)
        {
            PasswordDataEntry passwordDataEntry = _passwordData[charsetKey];
            string fullCharset = _fullCharsets[charsetKey];

            passwordDataEntry.Charset = included ? fullCharset.Intersect(charset) : fullCharset.Except(charset);
            _mainCharset.ExceptWith(fullCharset);
            _mainCharset.UnionWith(passwordDataEntry.Charset);
        }

        private void IncludeTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            IEnumerable<char> includedCharset = sender.Text.Intersect(_ascii).Except(ExcludeTextBox.Text);

            sender.Text = string.Join(null, includedCharset);
            foreach (var toggleButton in _toggleButtons.Where(toggleButton => !(bool) toggleButton.IsChecked))
            {
                var charsetKey = (string) toggleButton.Tag;
                NumberBox charsetMin = _charsetMins[charsetKey];

                UpdatePasswordDataCharset(sender.Text, charsetKey, true);
                if (_passwordData[charsetKey].Charset.Any())
                {
                    if (!charsetMin.IsEnabled)
                        TurnOn(charsetMin);
                }
                else if (charsetMin.IsEnabled)
                    TurnOff(charsetMin);
            }
            RefreshPassword();
        }

        private string _oldExcludedCharset = string.Empty;
        private void ExcludeTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            IEnumerable<char> excludedCharset = sender.Text.Intersect(_ascii).Except(IncludeTextBox.Text);

            // Avoid excluding entire charsets.
            foreach (string charsetKey in _charsetKeys)
            {
                if (_fullCharsets[charsetKey].All(character => excludedCharset.Contains(character)))
                {
                    sender.Text = _oldExcludedCharset;
                    return;
                }
            }

            sender.Text = string.Join(null, excludedCharset);
            _oldExcludedCharset = sender.Text;
            foreach (var toggleButton in _toggleButtons.Where(toggleButton => (bool) toggleButton.IsChecked))
                UpdatePasswordDataCharset(sender.Text, (string) toggleButton.Tag, false);
            RefreshPassword();
        }

        private bool _open = true;

        /// <summary>
        /// Get a new password and refresh text on screen after some delay.
        /// </summary>
        private async void RefreshPassword()
        {
            if (_open)
            {
                _open = false;
                await Task.Delay(THROTTLE_DELAY);
                _password = BuildPassword(_passwordData.Values, (int) PasswordLengthSlider.Value);
                CopyButton.IsEnabled = RefreshButton.IsEnabled = _password != string.Empty;
                PasswordTextBlock.Text = _mainCharset.Any() ? InsertZWSP(_password) : CHARSET_EMPTY_MESSAGE;
                TestProgram();
                _open = true;
            }

            // Insert ZERO WIDTH SPACE (U+200B) characters to enable character wrapping.
            string InsertZWSP(string password)
            {
                var wrappedPassword = new StringBuilder(password.Length * 2);

                foreach (char character in password)
                    wrappedPassword.Append($"{character}{ZWSP}");
                return wrappedPassword.ToString();
            }
        }

        [Conditional(DEBUG)]
        private void TestProgram()
        {
            var mainCharset = new HashSet<char>(_ascii.Length);
            string includedCharset = IncludeTextBox.Text;
            string excludedCharset = ExcludeTextBox.Text;
            const int BYTE_RANGE = 256;
            double enabledCharsetMinSum = 0;

            #region TextBox input validation
            // No duplicate characters
            Debug.Assert(includedCharset.Distinct().Count() == includedCharset.Length);
            Debug.Assert(excludedCharset.Distinct().Count() == excludedCharset.Length);

            // Only ascii characters
            Debug.Assert(!includedCharset.Except(_ascii).Any());
            Debug.Assert(!excludedCharset.Except(_ascii).Any());

            // Included and excluded charsets are disjoint
            Debug.Assert(!includedCharset.Intersect(excludedCharset).Any());

            // No suppressed charsets
            foreach (string charsetKey in _charsetKeys)
                Debug.Assert(_fullCharsets[charsetKey].Except(excludedCharset).Any());
            #endregion

            foreach (var toggleButton in _toggleButtons)
            {
                var charsetKey = (string) toggleButton.Tag;
                NumberBox charsetMin = _charsetMins[charsetKey];
                IEnumerable<char> charset = (bool) toggleButton.IsChecked
                    ? _fullCharsets[charsetKey].Except(ExcludeTextBox.Text)
                    : _fullCharsets[charsetKey].Intersect(includedCharset);

                mainCharset.UnionWith(charset);

                // Test charsetMin minimum
                Debug.Assert(charsetMin.Minimum == 0);

                // Test charsetMin IsEnabled
                Debug.Assert(charsetMin.IsEnabled == charset.Any());

                if (charsetMin.IsEnabled)
                {
                    // Validate charsetMin input (fractional part must be zero)
                    Debug.Assert(charsetMin.Value == (int) charsetMin.Value);

                    // Test charsetMin value
                    Debug.Assert(_password.Count(character => charset.Contains(character)) >= charsetMin.Value);

                    // Test charsetMin maximum
                    Debug.Assert(charsetMin.Maximum - charsetMin.Value == PasswordLengthSlider.Value - PasswordLengthSlider.Minimum);

                    enabledCharsetMinSum += charsetMin.Value;
                }
                else
                    Debug.Assert(double.IsNaN(charsetMin.Value));
            }

            // Test empty charset
            Debug.Assert(mainCharset.Any() == (PasswordTextBlock.Text != CHARSET_EMPTY_MESSAGE));

            // Test PasswordLengthSlider maximum, value and minimum
            Debug.Assert(PasswordLengthSlider.Maximum == BYTE_RANGE);
            if (mainCharset.Any())
                Debug.Assert(PasswordLengthSlider.Value == _password.Length);
            Debug.Assert(PasswordLengthSlider.Minimum == enabledCharsetMinSum);
        }


        private void PasswordLengthSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int delta = (int) (e.NewValue - e.OldValue);

            _passwordData[MAIN_CHARSET].Length += delta;

            // Maintain invariant #2
            foreach (NumberBox charsetMin in _charsetMins.Values.Where(charsetMin => charsetMin.IsEnabled))
                charsetMin.Maximum += delta;

            RefreshPassword();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshPassword();

        private readonly DataPackage _dataPackage = new DataPackage();
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            _dataPackage.SetText(_password);
            Clipboard.SetContent(_dataPackage);
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _advancedPageSettings[LENGTH] = (int) PasswordLengthSlider.Value;
            _advancedPageSettings[INCLUDED_CHARSET] = IncludeTextBox.Text;
            _advancedPageSettings[EXCLUDED_CHARSET] = ExcludeTextBox.Text;

            foreach (var toggleButton in _toggleButtons)
            {
                var charsetKey = (string) toggleButton.Tag;

                _advancedPageSettings[charsetKey] = new ApplicationDataCompositeValue
                {
                    { ON, (bool) toggleButton.IsChecked },
                    { MINIMUM_LENGTH, _charsetMins[charsetKey].Value },
                };
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) => ApplyUserSettings();

        private void ApplyUserSettings()
        {
            // Put these controls into a blank state to avoid validation conflicts
            IncludeTextBox.Text = string.Empty;
            ExcludeTextBox.Text = string.Empty;
            foreach (NumberBox charsetMin in _charsetMins.Values)
                charsetMin.Value = 0;

            IncludeTextBox.Text = (string) _advancedPageSettings[INCLUDED_CHARSET];
            ExcludeTextBox.Text = (string) _advancedPageSettings[EXCLUDED_CHARSET];
            PasswordLengthSlider.Value = (int) _advancedPageSettings[LENGTH];

            foreach (var toggleButton in _toggleButtons)
            {
                var charsetKey = (string) toggleButton.Tag;
                var charset = (ApplicationDataCompositeValue) _advancedPageSettings[charsetKey];

                toggleButton.IsChecked = (bool) charset[ON];
            }

            foreach (var charsetKey in _charsetKeys)
            {
                var charset = (ApplicationDataCompositeValue) _advancedPageSettings[charsetKey];

                _charsetMins[charsetKey].Value = (double) charset[MINIMUM_LENGTH];
            }
        }
    }
}
