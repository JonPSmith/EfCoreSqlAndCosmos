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

        [Theory]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(20)]
        public async Task TestSaveChangesAddNoSqlOk(int numBooks)
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);

            var sqlOptions = this.CreateUniqueClassOptions<SqlDbContext>();
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                sqlContext.WipeAllDataFromDatabase();
                await noSqlContext.Database.EnsureDeletedAsync();
                await noSqlContext.Database.EnsureCreatedAsync();

                var filepath = TestData.GetFilePath("10ManningBooks.json");

                var generator = new BookGenerator(sqlOptions, new NoSqlBookUpdater(noSqlContext));

                //ATTEMPT
                await generator.WriteBooksAsync(filepath, numBooks, true, numWritten => false);

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(numBooks);
                noSqlContext.Books.Select(_ => 1).AsEnumerable().Count().ShouldEqual(numBooks);
            }
        }

        
    }
}