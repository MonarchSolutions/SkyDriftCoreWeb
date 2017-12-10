using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkyDriftCoreWeb.Data;
using SkyDriftCoreWeb.Models;
using SkyDriftCoreWeb.Services;


namespace SkyDriftCoreWeb
{
    public class Startup
    {
        public static string ConnectionString;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

#if SQLCE
            ConnectionString = Configuration.GetConnectionString("SqlceConnection");
#else
            ConnectionString = Configuration.GetConnectionString("SqliteConnection");
#endif
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if SQLCE
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlCe(ConnectionString));
#else
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlite(ConnectionString));
#endif
            //services.AddDbContext<ApplicationDbContext>(options =>
            //   options.UseSqlite(ConnectionString));

            // Add framework services.

            //添加认证服务

            services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
                {
                    options.Lockout.AllowedForNewUsers = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext, int>() //MARK: necessary
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            //services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            //启用认证
            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            Core.UserManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
            Core.StartTasks();
        }
    }
}
