namespace PasswordGen.Pages
{
    using Windows.ApplicationModel;
    using Windows.Storage;
    using Windows.UI.Xaml.Controls;

    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            var versionNumber = (string) ApplicationData.Current.LocalSettings.Values[SettingsPage.VERSION];

            InitializeComponent();
            AppName.Content = $"{Package.Current.DisplayName} {versionNumber}";
        }
    }
}
