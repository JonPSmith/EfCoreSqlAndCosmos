// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestNoSqlBookUpdaterAsync
    {
        [Fact]
        public async Task TestNoSqlBookUpdaterOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestNoSqlBookUpdaterAsync));

            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var sqlContext = new SqlDbContext(options))
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            {
                await sqlContext.Database.EnsureCreatedAsync();
                await noSqlContext.Database.EnsureCreatedAsync();
                var updater = new NoSqlBookUpdater(noSqlContext);

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var hasUpdates = updater.FoundBookChangesToProjectToNoSql(sqlContext);
                await updater.ExecuteTransactionToSaveBookUpdatesAsync(sqlContext, true);

                //VERIFY
                hasUpdates.ShouldBeTrue();
                sqlContext.Books.Count().ShouldEqual(1);
                (await noSqlContext.Books.CountAsync(p => p.BookId == book.BookId)).ShouldEqual(1);
            }
        }

        [Fact]
        public async Task TestNoSqlBookUpdaterWithRetryStrategyOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestNoSqlBookUpdaterAsync));
            var connection = this.GetUniqueDatabaseConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();
            optionsBuilder.UseSqlServer(connection);
            var options = optionsBuilder.Options;
            using (var sqlContext = new SqlDbContext(options))
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            {
                sqlContext.CreateEmptyViaWipe();
                await noSqlContext.Database.EnsureCreatedAsync();
                var updater = new NoSqlBookUpdater(noSqlContext);

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var hasUpdates = updater.FoundBookChangesToProjectToNoSql(sqlContext);
                await updater.ExecuteTransactionToSaveBookUpdatesAsync(sqlContext, true);

                //VERIFY
                hasUpdates.ShouldBeTrue();
                sqlContext.Books.Count().ShouldEqual(1);
                (await noSqlContext.Books.CountAsync(p => p.BookId == book.BookId)).ShouldEqual(1);
            }
        }

        [Fact]
        public async Task TestNoSqlBookUpdaterFail_NoBookAddedToSqlDatabase()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    "UNKNOWNDATASBASENAME");

            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var sqlContext = new SqlDbContext(options))
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            {
                await sqlContext.Database.EnsureCreatedAsync();
                var updater = new NoSqlBookUpdater(noSqlContext);

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var hasUpdates = updater.FoundBookChangesToProjectToNoSql(sqlContext);
                var ex = await Assert.ThrowsAsync<HttpException>(async () => 
                    await updater.ExecuteTransactionToSaveBookUpdatesAsync(sqlContext, true));

                //VERIFY
                ex.Message.ShouldEqual("NotFound");
                hasUpdates.ShouldBeTrue();
                sqlContext.Books.Count().ShouldEqual(0);
            }
        }



    }
}