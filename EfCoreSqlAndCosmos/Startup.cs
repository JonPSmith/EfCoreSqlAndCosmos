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
using Microsoft.Extensions.Logging;
using NetCore.AutoRegisterDi;
using ServiceLayer.Books.Dtos;
using ServiceLayer.Books.Services;

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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var sqlConnection = Configuration.GetConnectionString("BookSqlConnection");
            services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(sqlConnection, sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure())
            );
            services.AddDbContext<NoSqlDbContext>(options =>
                options.UseCosmos(
                    Configuration["endpoint"],
                    Configuration["authKey"],
                    Configuration["database"],
                    noSqlOptions => noSqlOptions.ExecutionStrategy(c => new CosmosExecutionStrategy(c))));
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider,
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //I setup the database here, as its easier to get the wwwroot directory
            //see this issue to track this problem https://github.com/aspnet/AspNetCore/issues/4206
            serviceProvider.SetupDevelopmentDatabase(env.WebRootPath);



        }
    }
}
