using System;
using System.IO;
using System.Reflection;
using BurnSystems.Logging;
using BurnSystems.Logging.Provider;
using DatenMeister.Integration;
using DatenMeister.Integration.DotNet;
using DatenMeister.Modules.PublicSettings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AdressenMeister.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                InitializeLogging();

                var assembly = Assembly.GetEntryAssembly() ??
                               throw new InvalidOperationException("Entry assembly is null");

                var assemblyDirectoryName = Path.GetDirectoryName(assembly.Location) ??
                                            throw new InvalidOperationException("Assembly Directory Name is null");

                // Loads the DatenMeister
                var defaultSettings = GiveMe.GetDefaultIntegrationSettings();
                defaultSettings.IsLockingActivated = true;
                defaultSettings.DatabasePath = Path.Combine(assemblyDirectoryName, "data");

                GiveMe.Scope = GiveMe.DatenMeister(defaultSettings);

                CreateHostBuilder(args).Build().Run();
            }
            finally
            {
                // Unloads the Datenmeister
                GiveMe.TryGetScope()?.UnuseDatenMeister();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        private static void InitializeLogging()
        {
            TheLog.AddProvider(new DebugProvider(), LogLevel.Trace);

            // Preload Public Settings
            var publicSettingsPath = Assembly.GetEntryAssembly()?.Location;
            var publicSettings =
                PublicSettingHandler.LoadSettingsFromDirectory(
                    Path.GetDirectoryName(publicSettingsPath) ?? throw new InvalidOperationException("Path returned null"));
            if (publicSettings == null || publicSettings.logLocation != LogLocation.None)
            {
                var location = publicSettings?.logLocation ?? LogLocation.Application;

                var logPath = location switch
                {
                    LogLocation.Application =>
                        Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!,
                            "log.txt"),
                    LogLocation.Desktop =>
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            "AdressenMeister.log.txt"),
                    LogLocation.LocalAppData =>
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "AdressenMeister/log.txt"),
                    _ => Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, "log.txt")
                };

                TheLog.AddProvider(new FileProvider(logPath, true), LogLevel.Trace);
            }

            TheLog.AddProvider(InMemoryDatabaseProvider.TheOne, LogLevel.Debug);
            TheLog.AddProvider(new ConsoleProvider(), LogLevel.Debug);
        }
    }
}