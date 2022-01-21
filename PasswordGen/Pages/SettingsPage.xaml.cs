﻿namespace PasswordGen.Pages
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Windows.Storage;
    using Windows.UI;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using static Utilities.Charsets;

    public sealed partial class SettingsPage : Page
    {
        private readonly string _homeTypeName = typeof(HomePage).FullName;
        private readonly string _advancedTypeName = typeof(AdvancedPage).FullName;

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
        internal const string START_PAGE = nameof(START_PAGE);

        private static readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        internal static readonly IDictionary<string, object> _homePageSettings;
        internal static readonly IDictionary<string, object> _advancedPageSettings;

        static SettingsPage()
        {
            _homePageSettings = _localSettings.CreateContainer(HOME_PAGE_SETTINGS, ApplicationDataCreateDisposition.Always).Values;
            _advancedPageSettings = _localSettings.CreateContainer(ADVANCED_PAGE_SETTINGS, ApplicationDataCreateDisposition.Always).Values;
        }
        public SettingsPage() => InitializeComponent();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize theme setting control
            var theme = (string) _localSettings.Values[THEME];
            foreach (RadioButton radioButton in ThemeSetting.Items)
                if (theme == (string) radioButton.Tag)
                {
                    ThemeSetting.SelectedItem = radioButton;
                    break;
                }
            ThemeSetting.SelectionChanged += Theme_SelectionChanged;
            Debug.Assert(ThemeSetting.SelectedItem is RadioButton);

            // Initialize start page setting control
            var startPage = (string) _localSettings.Values[START_PAGE];
            foreach (ComboBoxItem comboBoxItem in StartSetting.Items)
            {
                if (startPage == (string) comboBoxItem.Tag)
                {
                    StartSetting.SelectedItem = comboBoxItem;
                    break;
                }
            }
            StartSetting.SelectionChanged += StartSetting_SelectionChanged;
            Debug.Assert(StartSetting.SelectedItem is ComboBoxItem);

            Loaded -= Page_Loaded; // Execute once
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
            bool isDarkTheme = rootFrame.RequestedTheme == ElementTheme.Dark ||
                (rootFrame.RequestedTheme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Dark);
            // Update title bar buttons
            if (isDarkTheme)
            {
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = GetColor(0x2D);
                titleBar.InactiveForegroundColor = GetColor(0x72);
                titleBar.ButtonPressedForegroundColor = GetColor(0xA7);
                titleBar.ButtonPressedBackgroundColor = GetColor(0x29);
            }
            else
            {
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = GetColor(0xE9);
                titleBar.InactiveForegroundColor = GetColor(0x9C);
                titleBar.ButtonPressedForegroundColor = GetColor(0x5F);
                titleBar.ButtonPressedBackgroundColor = GetColor(0xED);
            }
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Get an opaque gray color.
            Color GetColor(byte value) => new Color { A = byte.MaxValue, R = value, G = value, B = value };
        }

        private void StartSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => _localSettings.Values[START_PAGE] = ((ComboBoxItem) e.AddedItems[0]).Tag;
    }
}
