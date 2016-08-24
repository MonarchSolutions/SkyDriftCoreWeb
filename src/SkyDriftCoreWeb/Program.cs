using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace SkyDriftCoreWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Core.Init();

            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
#if !DEBUG
                .UseUrls(Core.Config.ListenUrls)
#endif
                .Build();
            
            host.Run();

            Core.StopTasks();
        }
    }
}
