using Inv_M_Sys.Services;
using Serilog;
using System.Windows;

namespace Inv_M_Sys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Called when the application starts
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // ✅ Set up logging (Serilog)
                LoggerSetup.SetupLogger();

                // ✅ Test the database connection when the app starts
                DatabaseHelper.TestConnection();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to connect to the database: {Error}", ex.Message);
                MessageBox.Show($"Error connecting to the database: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Called when the application exits
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // ✅ Ensure all logs are flushed before the app exits
            Log.CloseAndFlush();
        }
    }
}
