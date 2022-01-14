﻿namespace PasswordGen.Pages
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Utilities;

    using Windows.ApplicationModel.DataTransfer;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    using static Utilities.Charsets;
    using static Utilities.PasswordBuilder;

    public sealed partial class HomePage : Page
    {
        // Throttle delay of 4 ms to match a monitor refresh rate of 240 Hz.
        private const int THROTTLE_DELAY = 4;
        private const string DEBUG = nameof(DEBUG);

        private readonly ReadOnlyCollection<ToggleSwitch> _toggleSwitches;
        private readonly HashSet<ToggleSwitch> _toggleSwitchesOn;
        private readonly Dictionary<string, PasswordDataEntry> _passwordData;
        private readonly HashSet<char> _mainCharset;

        public HomePage()
        {
            InitializeComponent();
            _toggleSwitches = new ReadOnlyCollection<ToggleSwitch>(new ToggleSwitch[]
            {
                DigitSwitch,
                SymbolSwitch,
                LowercaseSwitch,
                UppercaseSwitch
            });
            _toggleSwitchesOn = new HashSet<ToggleSwitch>(_toggleSwitches);
            _passwordData = GetInitialPasswordData(out _mainCharset);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach event handlers
            foreach (var toggleSwitch in _toggleSwitches)
                toggleSwitch.Toggled += ToggleSwitch_Toggled;
            PasswordLengthSlider.ValueChanged += PasswordLengthSlider_ValueChanged;

            RefreshPassword();

            // Page_Loaded removes itself to execute on first load only.
            ((Page) Frame.Content).Loaded -= Page_Loaded;
        }

        private ToggleSwitch _disabledToggleSwitch;
        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch) sender;
            string charsetKey = (string) toggleSwitch.Tag;
            string charset = _fullCharsets[charsetKey];

            if (toggleSwitch.IsOn)
            {
                // Re-enable a disabled ToggleSwitch
                if (_toggleSwitchesOn.Count == 1)
                    _disabledToggleSwitch.IsEnabled = true;

                _toggleSwitchesOn.Add(toggleSwitch);

                // Update _passwordData
                _mainCharset.UnionWith(charset);
                _passwordData[MAIN_CHARSET].Length--;
                _passwordData[charsetKey].Charset = charset;
            }
            else
            {
                _toggleSwitchesOn.Remove(toggleSwitch);

                // Disable last ToggleSwitch on to avoid empty charset
                if (_toggleSwitchesOn.Count == 1)
                {
                    _disabledToggleSwitch = _toggleSwitchesOn.First();
                    _disabledToggleSwitch.IsEnabled = false;
                }

                // Update _passwordData
                _mainCharset.ExceptWith(charset);
                _passwordData[MAIN_CHARSET].Length++;
                _passwordData[charsetKey].Charset = string.Empty;
            }
            RefreshPassword();
        }

        private bool _open = true;

        /// <summary>
        /// Get a new password and refresh text on screen.
        /// </summary>
        private async void RefreshPassword()
        {
            if (_open)
            {
                _open = false;
                await Task.Delay(THROTTLE_DELAY);
                PasswordTextBlock.Text = BuildPassword(_passwordData.Values, (int) PasswordLengthSlider.Value);
                TestProgram();
                _open = true;
            }
        }

        [Conditional(DEBUG)]
        private void TestProgram()
        {
            int toggleSwitchesOnCount = _toggleSwitches.Where(toggleSwitch => toggleSwitch.IsOn).Count();

            // Test empty charset validation
            Debug.Assert(toggleSwitchesOnCount > 0 && toggleSwitchesOnCount <= _toggleSwitches.Count);
            if (toggleSwitchesOnCount == 1)
                foreach (var toggleSwitch in _toggleSwitches)
                    Debug.Assert(toggleSwitch.IsOn == !toggleSwitch.IsEnabled);
            else
                foreach (var toggleSwitch in _toggleSwitches)
                    Debug.Assert(toggleSwitch.IsEnabled);

            // Test length
            Debug.Assert(PasswordTextBlock.Text.Length == (int) PasswordLengthSlider.Value);

            // Test length per charset (at least one when on, zero when off)
            foreach (var toggleSwitch in _toggleSwitches)
            {
                string charsetKey = (string) toggleSwitch.Tag;
                string charset = _fullCharsets[charsetKey];

                Debug.Assert(PasswordTextBlock.Text.Intersect(charset).Any() == toggleSwitch.IsOn);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshPassword();

        private void PasswordLengthSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _passwordData[MAIN_CHARSET].Length += (int) (e.NewValue - e.OldValue);
            RefreshPassword();
        }

        private readonly DataPackage _dataPackage = new DataPackage();
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            _dataPackage.SetText(PasswordTextBlock.Text);
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
