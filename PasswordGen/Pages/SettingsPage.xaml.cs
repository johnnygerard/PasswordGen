namespace PasswordGen.Pages
{
    using Windows.UI.Xaml.Controls;

    public sealed partial class SettingsPage : Page
    {
        /// <summary>
        /// Throttle delay of 4 ms to match a monitor refresh rate of 240 Hz.
        /// </summary>
        internal const int THROTTLE_DELAY = 4;

        // Initial password settings
        internal const int INIT_LENGTH = 16;
        internal const int INIT_CHARSET_MIN = 1;

        public SettingsPage() => InitializeComponent();
    }
}
