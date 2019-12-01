using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        //2.x
        //        public Startup(IConfiguration configuration)
        //        {
        //            Configuration = configuration;

        //#if SQLCE
        //            ConnectionString = Configuration.GetConnectionString("SqlceConnection");
        //#else
        //            ConnectionString = Configuration.GetConnectionString("SqliteConnection");
        //#endif
        //        }


        //        public IConfiguration Configuration { get; }

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
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
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
                .AddEntityFrameworkStores<ApplicationDbContext>() //MARK: necessary
                .AddDefaultTokenProviders();

            //MARK: NEW: AddRazorRuntimeCompilation
            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            //services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env /*, ILoggerFactory loggerFactory , UserManager<ApplicationUser> userManager*/)
        {
            //loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                //loggerFactory.AddDebug();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            //app.UseHttpsRedirection();
            //app.UseCookiePolicy();

            //启用认证
            //https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/identity-2x?view=aspnetcore-2.1

            //app.UseIdentity();
            app.UseAuthentication();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            Core.UserManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
            //Core.UserManager = userManager;
            Core.StartTasks();
        }
    }
}