// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Reflection;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using EfCoreSqlAndCosmos.Logger;
using GenericServices.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCore.AutoRegisterDi;
using ServiceLayer.BooksSql.Dtos;
using ServiceLayer.BooksSql.Services;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var sqlConnection = Configuration.GetConnectionString("BookSqlConnection");
            services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(sqlConnection, sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure())
            );
            //Note you don't need to set an ExecutionStrategy on Cosmos provider 
            //see https://github.com/aspnet/EntityFrameworkCore/issues/8443#issuecomment-465836181
            services.AddDbContext<NoSqlDbContext>(options =>
                options.UseCosmos(
                    //I use user secrets to provide the actual Azure Cosmos database, but fall back to local emulator if no secrets set
                    Configuration["CosmosUrl"] ?? Configuration["endpoint"],
                    Configuration["CosmosKey"] ?? Configuration["authKey"],
                    Configuration["database"]));
            //This registers the NoSqlBookUpdater and will cause changes to books to be updated in the NoSql database
            services.AddScoped<IBookUpdater, NoSqlBookUpdater>();

            //Setup GenericServices
            services.GenericServicesSimpleSetup<SqlDbContext>(Assembly.GetAssembly(typeof(BookListDto)));
            //register other services in the ServiceLayer
            services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(ListBooksService)))
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider,
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            //I setup the database here, because there is a problem in ASP.NET Core 2.2 about accessing the root directory
            //see this issue to track this problem https://github.com/aspnet/AspNetCore/issues/4206
            serviceProvider.SetupDevelopmentDatabase(env.WebRootPath);



        }
    }
}
