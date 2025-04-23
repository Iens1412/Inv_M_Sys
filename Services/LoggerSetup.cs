using Serilog;

namespace Inv_M_Sys.Services
{
    public static class LoggerSetup
    {
        public static void SetupLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/app_log.txt", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, retainedFileCountLimit: 7)
                .MinimumLevel.Information()
                .CreateLogger();
        }
    }
}
