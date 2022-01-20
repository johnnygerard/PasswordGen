namespace PasswordGen.Pages
{
    using Microsoft.UI.Xaml.Controls;

    using System;
    using System.Collections.Generic;

    using Windows.Storage;
    using Windows.UI;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using static Utilities.Charsets;

    public sealed partial class SettingsPage : Page
    {
        // Default values
        internal const int DEFAULT_LENGTH = 16;
        internal const double DEFAULT_CHARSET_MINIMUM = 1;

        // Settings keys
        internal const string HOME_PAGE_SETTINGS = nameof(HOME_PAGE_SETTINGS);
        internal const string ADVANCED_PAGE_SETTINGS = nameof(ADVANCED_PAGE_SETTINGS);
        internal const string LENGTH = nameof(LENGTH);
        internal const string MINIMUM_LENGTH = nameof(MINIMUM_LENGTH);
        internal const string ON = nameof(ON);
        internal const string INCLUDED_CHARSET = nameof(INCLUDED_CHARSET);
        internal const string EXCLUDED_CHARSET = nameof(EXCLUDED_CHARSET);
        internal const string THEME = nameof(THEME);
        internal const string VERSION = nameof(VERSION);

        private static readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        internal static readonly IDictionary<string, object> _homePageSettings;
        internal static readonly IDictionary<string, object> _advancedPageSettings;
        private readonly Dictionary<string, RadioButton> _radioButtons = new Dictionary<string, RadioButton>();

        static SettingsPage()
        {
            _homePageSettings = _localSettings.CreateContainer(HOME_PAGE_SETTINGS, ApplicationDataCreateDisposition.Always).Values;
            _advancedPageSettings = _localSettings.CreateContainer(ADVANCED_PAGE_SETTINGS, ApplicationDataCreateDisposition.Always).Values;
        }
        public SettingsPage()
        {
            InitializeComponent();
            foreach (RadioButton radioButton in ThemeSettings.Items)
                _radioButtons.Add((string) radioButton.Tag, radioButton);
            ThemeSettings.SelectedItem = _radioButtons[(string) _localSettings.Values[THEME]];
            ThemeSettings.SelectionChanged += Theme_SelectionChanged;
        }

        internal static void InitializeHomePageSettings()
        {
            _homePageSettings[LENGTH] = DEFAULT_LENGTH;
            foreach (string charsetKey in _charsetKeys)
                _homePageSettings[charsetKey] = true;
        }
        internal static void InitializeAdvancedPageSettings()
        {
            _advancedPageSettings[LENGTH] = DEFAULT_LENGTH;
            _advancedPageSettings[INCLUDED_CHARSET] = string.Empty;
            _advancedPageSettings[EXCLUDED_CHARSET] = string.Empty;
            foreach (string charsetKey in _charsetKeys)
            {
                _advancedPageSettings[charsetKey] = new ApplicationDataCompositeValue
                {
                    { MINIMUM_LENGTH, DEFAULT_CHARSET_MINIMUM },
                    { ON, true },
                };
            }
        }

        private void ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            InitializeHomePageSettings();
            InitializeAdvancedPageSettings();
        }

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is RadioButton selectedItem)
            {
                _localSettings.Values[THEME] = selectedItem.Tag;
                ApplyTheme();
            }
        }

        /// <summary>
        /// Set application theme from local settings.
        /// </summary>
        internal static void ApplyTheme()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            var rootFrame = (Frame) Window.Current.Content;

            rootFrame.RequestedTheme = Enum.Parse<ElementTheme>((string) _localSettings.Values[THEME]);
            // Update title bar buttons
            switch (rootFrame.RequestedTheme)
            {
                case ElementTheme.Default:
                    titleBar.ButtonForegroundColor = Application.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
                    break;
                case ElementTheme.Light:
                    titleBar.ButtonForegroundColor = Colors.Black;
                    break;
                case ElementTheme.Dark:
                    titleBar.ButtonForegroundColor = Colors.White;
                    break;
            }
        }
    }
}
