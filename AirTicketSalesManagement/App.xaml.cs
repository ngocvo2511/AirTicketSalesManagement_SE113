using System.Diagnostics.CodeAnalysis;
using System.Windows;
using QuestPDF.Infrastructure;

namespace AirTicketSalesManagement
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                        typeof(FrameworkElement),
                        new FrameworkPropertyMetadata(
                            System.Windows.Markup.XmlLanguage.GetLanguage("en-GB"))); // British culture for dd/MM/yyyy
            base.OnStartup(e);
            QuestPDF.Settings.License = LicenseType.Community;
        }

    }
}


