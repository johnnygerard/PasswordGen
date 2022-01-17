﻿namespace PasswordGen.Pages
{
    using System.Collections.Generic;

    using Windows.Storage;
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

        private static readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        internal static readonly IDictionary<string, object> _homePageSettings;
        internal static readonly IDictionary<string, object> _advancedPageSettings;


        static SettingsPage()
        {
            _homePageSettings = _localSettings.CreateContainer(HOME_PAGE_SETTINGS, ApplicationDataCreateDisposition.Existing).Values;
            _advancedPageSettings = _localSettings.CreateContainer(ADVANCED_PAGE_SETTINGS, ApplicationDataCreateDisposition.Existing).Values;
        }

        public SettingsPage() => InitializeComponent();

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
    }
}
