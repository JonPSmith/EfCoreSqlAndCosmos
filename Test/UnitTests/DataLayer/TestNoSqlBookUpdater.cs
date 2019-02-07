// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestNoSqlBookUpdater
    {
        [Fact]
        public async Task TestCosmosDbLocalDbEmulatorCreateDatabaseOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestNoSqlBookUpdater));

            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var sqlContext = new SqlDbContext(options))
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            {
                await sqlContext.Database.EnsureCreatedAsync();
                await noSqlContext.Database.EnsureCreatedAsync();
                var updater = new NoSqlBookUpdater(sqlContext, noSqlContext);

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                updater.FindTheChangesBeforeSaveChangesIsCalled();
                sqlContext.SaveChanges();
                await updater.UpdateNoSqlIfBooksHaveChangedAsync();            

                //VERIFY
                (await noSqlContext.Books.CountAsync(p => p.BookId == book.BookId)).ShouldEqual(1);
            }
        }



    }
}