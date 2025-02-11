using System.Configuration;
using System.Data;
using System.Windows;

namespace Inv_M_Sys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ✅ Initialize the database connection when the app starts
            DatabaseHelper.TestConnection();

            // ✅ Start the MainWindow after the database check
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
