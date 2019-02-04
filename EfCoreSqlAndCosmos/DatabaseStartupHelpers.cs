using System;
using System.IO;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos
{
    public static class DatabaseStartupHelpers
    {
  private const string WwwRootDirectory = "wwwroot\\";

        public static string GetWwwRootPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), WwwRootDirectory);
        }

        public static IWebHost SetupDevelopmentDatabase(this IWebHost webHost, bool seedDatabase = true)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                using (var context = services.GetRequiredService<SqlDbContext>())
                {
                    try
                    {
                        context.DevelopmentEnsureCreated(GetWwwRootPath());
                        if (seedDatabase)
                            context.SeedDatabase(GetWwwRootPath());
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while setting upor seeding the development database.");
                    }
                }
            }

            return webHost;
        }
    }
}