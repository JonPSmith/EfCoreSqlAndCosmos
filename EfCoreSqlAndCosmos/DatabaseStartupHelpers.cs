using System;
using DataLayer.EfCode;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos
{
    public static class DatabaseStartupHelpers
    {
        public static void SetupDevelopmentDatabase(this IServiceProvider serviceProvider, string wwwRootPath, bool seedDatabase = true)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                using (var context = services.GetRequiredService<SqlDbContext>())
                {

                    context.DevelopmentEnsureCreated(wwwRootPath);
                    if (seedDatabase)
                        context.SeedDatabase(wwwRootPath);
                }
            }
        }
    }
}