using System.Configuration;
using System.Data;
using System.Windows;
using Telecom_ThesisProject.Data;

namespace Telecom_ThesisProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                System.Windows.Markup.XmlLanguage.GetLanguage("ru-RU")));
        }

        // App.xaml.cs
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using var ctx = new TelecomDbContext();
                ctx.Database.CanConnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось подключиться к базе данных.\n\n" +
                    $"Проверьте файл appsettings.json\n\n{ex.Message}",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
                return;
            }
        }
    }
}
