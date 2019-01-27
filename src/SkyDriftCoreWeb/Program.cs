using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyDriftCoreWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Core.Init();
            //2.x, can not get UserManager anymore
            //            CreateWebHostBuilder(args)
            //#if !DEBUG
            //                .UseUrls(Core.Config.ListenUrls)
            //#endif
            //                .Build().Run();

            //1.x
            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
#if !DEBUG
                .UseUrls(Core.Config.ListenUrls)
#endif
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                })
                .Build();
            host.Run();

            Core.StopTasks();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                //.ConfigureAppConfiguration((hostContext, config) =>
                //{
                //    // delete all default configuration providers
                //    config.Sources.Clear();
                //    config.AddJsonFile("appsettings.json", optional: true);
                //})
        ;
    }
}
