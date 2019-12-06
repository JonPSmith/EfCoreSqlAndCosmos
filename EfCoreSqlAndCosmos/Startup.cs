// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Reflection;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using DataLayerEvents.EfCode;
using EfCoreSqlAndCosmos.Logger;
using GenericServices.Setup;
using Infrastructure.AppStart;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceLayer.AppStart;
using ServiceLayer.BooksSql.Dtos;

namespace EfCoreSqlAndCosmos
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
            services.AddRazorPages();
            services.AddHttpContextAccessor();

            var sqlConnection = Configuration.GetConnectionString("BookSqlConnection");
            services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(sqlConnection, sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure())
            );
            services.AddDbContext<SqlEventsDbContext>(options =>
                options.UseSqlServer(sqlConnection, sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure())
            );

            //The user secrets provides an actual Azure Cosmos database. The takeUserSecrets flag controls this  
            var takeUserSecrets = false;
            var cosmosUtl = takeUserSecrets ? Configuration["CosmosUrl"] : Configuration["endpoint"];
            var cosmosKey = takeUserSecrets ? Configuration["CosmosKey"] : Configuration["authKey"];
            //Note you don't need to set an ExecutionStrategy on Cosmos provider 
            //see https://github.com/aspnet/EntityFrameworkCore/issues/8443#issuecomment-465836181
            services.AddDbContext<NoSqlDbContext>(options =>
                options.UseCosmos(
                    cosmosUtl, cosmosKey,Configuration["database"]));
            //This registers the NoSqlBookUpdater and will cause changes to books to be updated in the NoSql database
            services.AddScoped<IBookUpdater, NoSqlBookUpdater>();

            //The other projects that need DI have their own extension methods to handle that
            services.RegisterInfrastructureDi();
            services.RegisterServiceLayerDi();

            //Setup GenericServices (two DbContexts) 
            //NOTE: must come after the call to RegisterInfrastructureDi which sets up the EventsRunner 
            services.ConfigureGenericServicesEntities(typeof(SqlDbContext), typeof(SqlEventsDbContext))
                .ScanAssemblesForDtos(Assembly.GetAssembly(typeof(BookListDto)))
                .RegisterGenericServices();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            loggerFactory.AddProvider(new RequestTransientLogger(() => httpContextAccessor));
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });


        }
    }
}
