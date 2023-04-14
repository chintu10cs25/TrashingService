using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using TrashingService;
using TrashingService.Common;
using TrashingService.Simulator;

class Program
{
    static void Main(string[] args)
    {

        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

        string appName = Assembly.GetEntryAssembly().GetName().Name;
        string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "logs");
        Directory.CreateDirectory(logDirectory);

        string logPath = Path.Combine(logDirectory, $"{appName}-.log");

        Log.Logger = new LoggerConfiguration()
             .ReadFrom.Configuration(configuration)
             .WriteTo.File(
                 path: logPath,
                 rollingInterval: RollingInterval.Day,
                 retainedFileCountLimit: 7,
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
             .CreateLogger();

        try
        {
            Log.Information("Starting up...");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .UseSerilog()
              .ConfigureServices((hostContext, services) =>
              {
                  //services.AddSingleton<Terminal>();
                  services.AddSingleton<TrashingProcessor>();
                  services.AddHostedService<TrashingWorker>();
                  
              });


}



