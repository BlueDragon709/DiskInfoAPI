using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace DiskInfoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("");
                Log.Information("////////////////////////////////////////////////////////////");
                Log.Information("Starting DiskInfoAPI");

                bool isService = !(Debugger.IsAttached || args.Contains("--console"));

                if (isService)
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                    Directory.SetCurrentDirectory(pathToContentRoot);
                }

                var builder = CreateWebHostBuilder(args.Where(arg => arg != "--console").ToArray());
                var host = builder.Build();

                if (isService)
                {
                    Log.Information("Starting as service");
                    host.RunAsService();
                }
                else
                {
                    Log.Information("Starting as console");
                    host.Run();
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseUrls("http://0.0.0.0:5000")
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
