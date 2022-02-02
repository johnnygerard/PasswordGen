namespace PasswordGen.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Utilities;

    using Windows.ApplicationModel.DataTransfer;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    using static SettingsPage;
    using static Utilities.Charsets;
    using static Utilities.PasswordBuilder;

    public sealed partial class HomePage : Page
    {
        // Throttle delay of 4 ms to match a monitor refresh rate of 240 Hz.
        private const int THROTTLE_DELAY = 4;

        private readonly IDictionary<string, object> _homePageSettings;
        private readonly IReadOnlyList<ToggleSwitch> _charsetSwitches;
        private readonly HashSet<ToggleSwitch> _charsetSwitchesOn;
        private readonly Dictionary<string, PasswordDataEntry> _passwordData;
        private readonly HashSet<char> _mainCharset;

        public HomePage()
        {
            InitializeComponent();
            _homePageSettings = ApplicationData.Current.LocalSettings.Containers[HOME_PAGE_SETTINGS].Values;
            _charsetSwitches = new ToggleSwitch[]
            {
                DigitSwitch,
                SymbolSwitch,
                LowercaseSwitch,
                UppercaseSwitch
            };
            _charsetSwitchesOn = new HashSet<ToggleSwitch>(_charsetSwitches);
            _passwordData = GetInitialPasswordData(out _mainCharset);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach event handlers
            foreach (ToggleSwitch charsetSwitch in _charsetSwitches)
                charsetSwitch.Toggled += CharsetSwitch_Toggled;
            PasswordLengthSlider.ValueChanged += PasswordLengthSlider_ValueChanged;

            ApplyUserSettings();
            RefreshPassword();

            Loaded -= Page_Loaded; // Execute once
        }

        private ToggleSwitch _disabledCharsetSwitch;
        private void CharsetSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var charsetSwitch = (ToggleSwitch) sender;
            string charsetKey = (string) charsetSwitch.Tag;
            string charset = _fullCharsets[charsetKey];

            if (charsetSwitch.IsOn)
            {
                // Re-enable a disabled charset switch
                if (_charsetSwitchesOn.Count == 1)
                    _disabledCharsetSwitch.IsEnabled = true;

                _charsetSwitchesOn.Add(charsetSwitch);

                // Update password data
                _mainCharset.UnionWith(charset);
                _passwordData[MAIN_CHARSET].Length--;
                _passwordData[charsetKey].Charset = charset;
            }
            else
            {
                _charsetSwitchesOn.Remove(charsetSwitch);

                // Disable last charset switch on to avoid empty charset
                if (_charsetSwitchesOn.Count == 1)
                {
                    _disabledCharsetSwitch = _charsetSwitchesOn.First();
                    _disabledCharsetSwitch.IsEnabled = false;
                }

                // Update password data
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
                _open = true;
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
            foreach (ToggleSwitch charsetSwitch in _charsetSwitches)
                _homePageSettings[(string) charsetSwitch.Tag] = charsetSwitch.IsOn;
            _homePageSettings[LENGTH] = (int) PasswordLengthSlider.Value;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) => ApplyUserSettings();

        private void ApplyUserSettings()
        {
            // Turn a charset switch on first to avoid validation conflicts
            foreach (ToggleSwitch charsetSwitch in _charsetSwitches)
                if ((bool) _homePageSettings[(string) charsetSwitch.Tag])
                {
                    charsetSwitch.IsOn = true;
                    break;
                }

            foreach (ToggleSwitch charsetSwitch in _charsetSwitches)
                charsetSwitch.IsOn = (bool) _homePageSettings[(string) charsetSwitch.Tag];

            PasswordLengthSlider.Value = (int) _homePageSettings[LENGTH];
        }
    }
}
