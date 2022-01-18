namespace PasswordGen
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Core;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using MUXC = Microsoft.UI.Xaml.Controls;

    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        // Window's title
        private readonly string _displayName = Package.Current.DisplayName;

        private readonly MUXC::NavigationViewItem _startPage;

        public MainPage()
        {
            InitializeComponent();
            _startPage = HomePage;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

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
            => ((MUXC::NavigationView) sender).SelectedItem = _startPage;

        private void NavigationView_SelectionChanged(MUXC::NavigationView sender, MUXC::NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (MUXC::NavigationViewItem) args.SelectedItem;
            Type sourcePageType = args.IsSettingsSelected
                ? typeof(Pages.SettingsPage)
                : Type.GetType($"PasswordGen.Pages.{selectedItem.Name}");

            MainFrame.Navigate(sourcePageType, null, args.RecommendedNavigationTransitionInfo);
        }



        #region Theme
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = default)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ElementTheme _appRequestedTheme;
        internal ElementTheme AppRequestedTheme
        {
            get => _appRequestedTheme;
            set
            {
                if (_appRequestedTheme != value)
                {
                    _appRequestedTheme = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
    }
}
