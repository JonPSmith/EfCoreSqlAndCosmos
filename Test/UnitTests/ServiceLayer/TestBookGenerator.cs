// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using EfCoreSqlAndCosmos;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.DatabaseServices.Concrete;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayer
{
    public class TestBookGenerator
    {
        private DbContextOptions<SqlDbContext> _sqlOptions;
        public TestBookGenerator()
        {

            _sqlOptions = this.CreateUniqueClassOptions<SqlDbContext>();
            using (var context = new SqlDbContext(_sqlOptions))
            {
                context.Database.EnsureCreated();
                context.WipeAllDataFromDatabase();
            }
        }

        [Fact]
        public async Task TestSaveChangesAddNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                await noSqlContext.Database.EnsureDeletedAsync();
                await noSqlContext.Database.EnsureCreatedAsync();

                var aspNetAppDir = TestData.GetCallingAssemblyTopLevelDir();
                var filepath = Path.GetFullPath(Path.Combine(aspNetAppDir, "..\\EfCoreSqlAndCosmos\\wwwroot", 
                    SetupHelpers.SeedFileSubDirectory, SetupHelpers.TemplateFileName));

                var generator = new BookGenerator(_sqlOptions, new NoSqlBookUpdater(noSqlContext));

                //ATTEMPT
                await generator.WriteBooksAsync(filepath, 100, true, numWritten => false);

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(100);
                noSqlContext.Books.Select(_ => 1).AsEnumerable().Count().ShouldEqual(100);
            }
        }

        
    }
}