using SRQCC.Models;

namespace SRQCC.Services
{
    public static class AppVersionManager
    {
        public const string CurrentVersion = "1.0.0"; // CAMBIAR SEGUN LA VERSION
        public const int CurrentVersionCode = 100; // CAMBIAR SEGUN LA VERSION

        public static async Task<VersionInfo> CheckForUpdatesAsync()
        {
            try
            {
                var databaseService = new DatabaseService();
                return await databaseService.GetLatestVersionAsync();
            }
            catch
            {
                return null; // Silencioso en producción
            }
        }
    }
}