namespace PasswordGen.Pages
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
        private const string MAIN_CHARSET = nameof(MAIN_CHARSET);
        private const int THROTTLE_DELAY = 4; // 4 ms to match a monitor refresh rate of 240 Hz
        private const int CHARSET_MIN = 1;

        private readonly Dictionary<string, PasswordDataEntry> _passwordData = new Dictionary<string, PasswordDataEntry>();
        private readonly List<char> _mainCharset = new List<char>(_ascii.Length);
        private readonly HashSet<ToggleSwitch> _toggleSwitchesOn = new HashSet<ToggleSwitch>();
        private readonly ReadOnlyCollection<ToggleSwitch> _toggleSwitches;

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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Page_Loaded; // Execute only at first page load.

            // Initialize _passwordData
            foreach (var toggleSwitch in _toggleSwitches)
            {
                var charsetKey = (string) toggleSwitch.Tag;
                string charset = _fullCharsets[charsetKey];

                Debug.Assert(toggleSwitch.IsOn);
                _toggleSwitchesOn.Add(toggleSwitch);
                _mainCharset.AddRange(charset);
                _passwordData.Add(charsetKey, new PasswordDataEntry(charset, CHARSET_MIN));
            }
            int mainCharsetLength = (int) PasswordLengthSlider.Value - (_toggleSwitchesOn.Count * CHARSET_MIN);
            _passwordData.Add(MAIN_CHARSET, new PasswordDataEntry(_mainCharset, mainCharsetLength));

            // Attach event handlers
            foreach (var toggleSwitch in _toggleSwitches)
                toggleSwitch.Toggled += ToggleSwitch_Toggled;
            PasswordLengthSlider.ValueChanged += PasswordLengthSlider_ValueChanged;

            RefreshPassword();
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
                _mainCharset.AddRange(charset);
                _passwordData[MAIN_CHARSET].Length -= CHARSET_MIN;
                _passwordData[charsetKey].Length = CHARSET_MIN;
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
                _mainCharset.RemoveRange(_mainCharset.IndexOf(charset[0]), charset.Length);
                _passwordData[MAIN_CHARSET].Length += CHARSET_MIN;
                _passwordData[charsetKey].Length = 0;
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
                #region Test password
#if DEBUG
                string password = PasswordTextBlock.Text;

                // validate length
                Debug.Assert(password.Length == (int) PasswordLengthSlider.Value);

                // validate charsets
                foreach (var toggleSwitch in _toggleSwitches)
                {
                    string charsetKey = (string) toggleSwitch.Tag;
                    string charset = _fullCharsets[charsetKey];

                    Debug.Assert(password.Intersect(charset).Any() == toggleSwitch.IsOn);
                }
#endif 
                #endregion
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

        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
