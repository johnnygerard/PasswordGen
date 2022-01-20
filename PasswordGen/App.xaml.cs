﻿namespace PasswordGen
{
    using System;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.Storage;
    using Windows.UI;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    using static Pages.SettingsPage;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        internal const string VERSION = nameof(VERSION);

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        private void InitSettings()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var rootFrame = (Frame) Window.Current.Content;
            PackageVersion version = Package.Current.Id.Version;
            string versionNumber = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            #region Remove all settings
#if false
            localSettings.Values.Clear();
            foreach (string containerKey in localSettings.Containers.Keys)
                localSettings.DeleteContainer(containerKey);
#endif 
            #endregion

            if (!localSettings.Values.ContainsKey(VERSION) || (string) localSettings.Values[VERSION] != versionNumber)
            {
                localSettings.Values.Add(VERSION, versionNumber);
                localSettings.Values.Add(THEME, ElementTheme.Default.ToString());
                InitializeHomePageSettings();
                InitializeAdvancedPageSettings();
            }

            // Restore theme from user settings
            rootFrame.RequestedTheme = Enum.Parse<ElementTheme>((string) localSettings.Values[THEME]);
            switch (rootFrame.RequestedTheme)
            {
                case ElementTheme.Default:
                    titleBar.ButtonForegroundColor = RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
                    break;
                case ElementTheme.Light:
                    titleBar.ButtonForegroundColor = Colors.Black;
                    break;
                case ElementTheme.Dark:
                    titleBar.ButtonForegroundColor = Colors.White;
                    break;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame is null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                InitSettings();
            }

            if (!e.PrelaunchActivated)
            {
                if (rootFrame.Content is null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
            => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
