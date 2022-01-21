namespace PasswordGen.Pages
{
    using Windows.ApplicationModel;
    using Windows.Storage;
    using Windows.UI.Xaml.Controls;

    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            InitializeComponent();
            AppName.Content = $"{Package.Current.DisplayName} {localSettings.Values[SettingsPage.VERSION]}";
        }
    }
}
