namespace PasswordGen
{
    using PasswordGen.Pages;

    using System;
    using System.Diagnostics;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Core;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using MUXC = Microsoft.UI.Xaml.Controls;

    public sealed partial class MainPage : Page
    {
        private readonly string _homeTypeName = typeof(HomePage).FullName;
        private readonly string _advancedTypeName = typeof(AdvancedPage).FullName;

        // Window's title
        private readonly string _displayName = Package.Current.DisplayName;

        public MainPage()
        {
            InitializeComponent();
            SettingsPage.ApplyTheme();

            // Hide default title bar.
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
            => UpdateTitleBarLayout(sender);

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
            => AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            var navigationView = (MUXC.NavigationView) sender;
            var startPage = (string) ApplicationData.Current.LocalSettings.Values[SettingsPage.START_PAGE];

            foreach (MUXC.NavigationViewItem navigationViewItem in navigationView.MenuItems)
                if (startPage == (string) navigationViewItem.Tag)
                {
                    navigationView.SelectedItem = navigationViewItem;
                    break;
                }
            Debug.Assert(navigationView.SelectedItem is MUXC.NavigationViewItem);
        }

        private void NavigationView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (MUXC.NavigationViewItem) args.SelectedItem;
            Type pageType = args.IsSettingsSelected ? typeof(SettingsPage) : Type.GetType((string) selectedItem.Tag);

            MainFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        }
    }
}
