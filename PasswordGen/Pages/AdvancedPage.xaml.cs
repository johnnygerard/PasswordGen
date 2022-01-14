namespace PasswordGen.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.UI.Xaml.Controls;

    using PasswordGen.Utilities;

    using Windows.ApplicationModel.DataTransfer;
    using Windows.Globalization.NumberFormatting;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    using static SettingsPage;
    using static Utilities.Charsets;
    using static Utilities.PasswordBuilder;

    public sealed partial class AdvancedPage : Page
    {
        private const char ZWSP = '\u200B'; // ZERO WIDTH SPACE
        private const string CHARSET_EMPTY_MESSAGE = "CHARACTER SET EMPTY";

        private readonly ReadOnlyCollection<ToggleButton> _toggleButtons;
        private readonly ReadOnlyDictionary<string, NumberBox> _charsetMins;
        private readonly Dictionary<string, PasswordDataEntry> _passwordData;
        private readonly HashSet<char> _mainCharset;
        private string _password;

        public AdvancedPage()
        {
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
            var numberFormatter = new DecimalFormatter
            {
                NumberRounder = new IncrementNumberRounder { RoundingAlgorithm = RoundingAlgorithm.RoundTowardsZero },
                FractionDigits = 0,
            };
            foreach (NumberBox charsetMin in _charsetMins.Values)
                charsetMin.NumberFormatter = numberFormatter;

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

            RefreshPassword();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = (ToggleButton) sender;
            var charsetKey = (string) toggleButton.Tag;
            NumberBox charsetMin = _charsetMins[charsetKey];

            UpdatePasswordData(ExcludeTextBox.Text, charsetKey, false);
            if (!charsetMin.IsEnabled)
                TurnOn(charsetMin);
            RefreshPassword();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var toggleButton = (ToggleButton) sender;
            var charsetKey = (string) toggleButton.Tag;
            NumberBox charsetMin = _charsetMins[charsetKey];

            UpdatePasswordData(IncludeTextBox.Text, charsetKey, true);
            if (!_passwordData[charsetKey].Charset.Any())
                TurnOff(charsetMin);
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
            foreach (NumberBox charsetMin in _charsetMins.Values.Where(charsetMin => charsetMin.IsEnabled))
                if (charsetMin != sender) charsetMin.Maximum -= delta;

            // Update _passwordData
            _passwordData[(string) sender.Tag].Length += delta;
            _passwordData[MAIN_CHARSET].Length -= delta;
        }

        private void IncludeTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            IEnumerable<char> sanitizedText = sender.Text.Intersect(_ascii).Except(ExcludeTextBox.Text);

            sender.Text = string.Join(null, sanitizedText);
            foreach (var toggleButton in _toggleButtons.Where(toggleButton => !(bool) toggleButton.IsChecked))
            {
                string charsetKey = (string) toggleButton.Tag;
                NumberBox charsetMin = _charsetMins[charsetKey];

                UpdatePasswordData(sender.Text, charsetKey, true);
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
            IEnumerable<char> sanitizedText = sender.Text.Intersect(_ascii).Except(IncludeTextBox.Text);

            // Suppressed charset validation
            foreach (string charsetKey in _charsetKeys)
            {
                if (_fullCharsets[charsetKey].All(character => sanitizedText.Contains(character)))
                {
                    sender.Text = _oldExcludedCharset;
                    return;
                }
            }

            sender.Text = string.Join(null, sanitizedText);
            _oldExcludedCharset = sender.Text;
            foreach (var toggleButton in _toggleButtons.Where(toggleButton => (bool) toggleButton.IsChecked))
                UpdatePasswordData(sender.Text, (string) toggleButton.Tag, false);
            RefreshPassword();
        }

        private void UpdatePasswordData(IEnumerable<char> charset, string charsetKey, bool included)
        {
            var passwordDataEntry = _passwordData[charsetKey];
            string fullCharset = _fullCharsets[charsetKey];

            passwordDataEntry.Charset = included ? fullCharset.Intersect(charset) : fullCharset.Except(charset);
            _mainCharset.ExceptWith(fullCharset);
            _mainCharset.UnionWith(passwordDataEntry.Charset);
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
                PasswordTextBlock.Text = _mainCharset.Any() ? InsertZWSP(_password) : CHARSET_EMPTY_MESSAGE;
                TestProgram();
                _open = true;
            }

            // Insert ZERO WIDTH SPACE (U+200B) characters to wrap the password at any character position.
            string InsertZWSP(string password)
            {
                var newPassword = new char[password.Length * 2];

                for (int i = 0, j = 0; i < password.Length; i++, j += 2)
                {
                    newPassword[j] = password[i];
                    newPassword[j + 1] = ZWSP;
                }
                return string.Join(null, newPassword);
            }
        }

        [Conditional("DEBUG")]
        private void TestProgram()
        {
            string includedCharset = IncludeTextBox.Text;
            string excludedCharset = ExcludeTextBox.Text;
            const int BYTE_RANGE = 256;

            if (PasswordTextBlock.Text == CHARSET_EMPTY_MESSAGE)
            {
                foreach (var toggleButton in _toggleButtons)
                    Debug.Assert(!(bool) toggleButton.IsChecked);
                Debug.Assert(IncludeTextBox.Text == string.Empty);
                return;
            }

            foreach (var toggleButton in _toggleButtons)
            {
                var charsetKey = (string) toggleButton.Tag;
                NumberBox charsetMin = _charsetMins[charsetKey];
                var min = (int) charsetMin.Value;
                IEnumerable<char> charset = (bool) toggleButton.IsChecked
                    ? _fullCharsets[charsetKey].Except(ExcludeTextBox.Text)
                    : _fullCharsets[charsetKey].Intersect(includedCharset);

                // Test charsetMin minimum
                Debug.Assert(charsetMin.Minimum == 0);

                if (charsetMin.IsEnabled)
                {
                    // Test charsetMin value
                    Debug.Assert(_password.Count(character => charset.Contains(character)) >= min);
                    Debug.Assert(charsetMin.Value == min); // Fractional part must be zero
                }
                else
                {
                    // must be NaN
                    Debug.Assert(double.IsNaN(charsetMin.Value));

                    // charset must be empty
                    Debug.Assert(!_password.Intersect(charset).Any());
                }
            }

            // Test charsetMin maximum (invariant #2)
            foreach (NumberBox charsetMin in _charsetMins.Values)
                if (charsetMin.IsEnabled)
                    Debug.Assert(charsetMin.Maximum - charsetMin.Value == PasswordLengthSlider.Value - PasswordLengthSlider.Minimum);

            // Test password length minimum (invariant #1)
            double enabledCharsetMinSum = 0;
            foreach (NumberBox charsetMin in _charsetMins.Values.Where(charsetMin => charsetMin.IsEnabled))
                enabledCharsetMinSum += charsetMin.Value;
            Debug.Assert(PasswordLengthSlider.Minimum == enabledCharsetMinSum);

            // Test password length value
            Debug.Assert(_password.Length == (int) PasswordLengthSlider.Value);

            // Test password length maximum
            Debug.Assert(PasswordLengthSlider.Maximum == BYTE_RANGE);

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
        }


        private void PasswordLengthSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var delta = (int) (e.NewValue - e.OldValue);

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

        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
