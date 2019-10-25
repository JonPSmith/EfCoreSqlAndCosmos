// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using DataLayer.EfCode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceLayer.DatabaseServices.Concrete;

namespace EfCoreSqlAndCosmos
{
    public static class DatabaseStartupHelpers
    {
        public static void SetupDevelopmentDatabase(this IHost iHost, string currDirectory, bool seedDatabase = true)
        {
            using var scope = iHost.Services.CreateScope();
            var services = scope.ServiceProvider;
            using var sqlContext = services.GetRequiredService<SqlDbContext>();
            using var noSqlContext = services.GetRequiredService<NoSqlDbContext>();

            var wwwRootPath = Path.Combine(currDirectory, "wwwroot\\");
            sqlContext.DevelopmentEnsureCreated(noSqlContext);
            if (seedDatabase)
                sqlContext.SeedDatabase(wwwRootPath);
        }
    }
}