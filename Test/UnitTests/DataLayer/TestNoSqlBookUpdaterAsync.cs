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
        private DbContextOptions<SqlDbContext> _sqlOptions;
        public TestNoSqlBookUpdaterAsync()
        {

            _sqlOptions = this.CreateUniqueClassOptions<SqlDbContext>();
            using var context = new SqlDbContext(_sqlOptions);
            context.Database.EnsureCreated();
            var filepath = TestData.GetFilePath(@"..\..\EfCoreSqlAndCosmos\wwwroot\AddUserDefinedFunctions.sql");
            context.ExecuteScriptFileInTransaction(filepath);
            context.WipeAllDataFromDatabase();
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


            await using var sqlContext = new SqlDbContext(_sqlOptions);
            await using var noSqlContext = new NoSqlDbContext(builder.Options);
            await sqlContext.Database.EnsureCreatedAsync();
            var updater = new NoSqlBookUpdater(noSqlContext);

            //ATTEMPT
            var book = DddEfTestData.CreateDummyBookOneAuthor();
            sqlContext.Add(book);
            var hasUpdates = updater.FindBookChangesToProjectToNoSql(sqlContext);
            var ex = await Assert.ThrowsAsync<HttpException>(async () =>
                await updater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(sqlContext, () => sqlContext.SaveChangesAsync()));

            //VERIFY
            ex.Message.ShouldEqual("NotFound");
            hasUpdates.ShouldBeTrue();
            sqlContext.Books.Count().ShouldEqual(0);
        }

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


            await using var sqlContext = new SqlDbContext(_sqlOptions);
            await using var noSqlContext = new NoSqlDbContext(builder.Options);
            await sqlContext.Database.EnsureCreatedAsync();
            await noSqlContext.Database.EnsureCreatedAsync();
            var updater = new NoSqlBookUpdater(noSqlContext);

            //ATTEMPT
            var book = DddEfTestData.CreateDummyBookOneAuthor();
            sqlContext.Add(book);
            var hasUpdates = updater.FindBookChangesToProjectToNoSql(sqlContext);
            await updater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(sqlContext, () => sqlContext.SaveChangesAsync());

            //VERIFY
            hasUpdates.ShouldBeTrue();
            sqlContext.Books.Count().ShouldEqual(1);
            noSqlContext.Books.Where(p => p.BookId == book.BookId).Count().ShouldEqual(1);
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
            await using var sqlContext = new SqlDbContext(_sqlOptions);
            await using var noSqlContext = new NoSqlDbContext(builder.Options);
            
            sqlContext.CreateEmptyViaWipe();
            await noSqlContext.Database.EnsureCreatedAsync();
            var updater = new NoSqlBookUpdater(noSqlContext);

            //ATTEMPT
            var book = DddEfTestData.CreateDummyBookOneAuthor();
            sqlContext.Add(book);
            var hasUpdates = updater.FindBookChangesToProjectToNoSql(sqlContext);
            await updater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(sqlContext,
                () => sqlContext.SaveChangesAsync());

            //VERIFY
            hasUpdates.ShouldBeTrue();
            sqlContext.Books.Count().ShouldEqual(1);
            (await noSqlContext.Books.CountAsync(p => p.BookId == book.BookId)).ShouldEqual(1);
        }
    }
}