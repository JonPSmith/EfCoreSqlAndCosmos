// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.NoSqlDbContextTests
{
    public class TestNoSqlBookUpdaterAsync
    {
        private DbContextOptions<SqlDbContext> _sqlOptions;
        public TestNoSqlBookUpdaterAsync()
        {

            _sqlOptions = this.CreateUniqueClassOptions<SqlDbContext>();
            using (var context = new SqlDbContext(_sqlOptions))
            {
                context.Database.EnsureCreated();
                context.WipeAllDataFromDatabase();
            }
        }

        [RunnableInDebugOnly]
        public async Task DeleteNoSqlDatabase()
        {
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);
            using var context = new NoSqlDbContext(builder.Options);
            await context.Database.EnsureDeletedAsync();
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


            using (var sqlContext = new SqlDbContext(_sqlOptions))
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            {
                await sqlContext.Database.EnsureCreatedAsync();
                var updater = new NoSqlBookUpdater(noSqlContext);

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var numBooksChanged = updater.FindNumBooksChanged(sqlContext);
                var ex = await Assert.ThrowsAsync<CosmosException>(async () =>
                    await updater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(sqlContext, numBooksChanged, 
                        () => sqlContext.SaveChangesAsync()));

                //VERIFY
                ex.Message.ShouldStartWith("Response status code does not indicate success: NotFound (404)");
                numBooksChanged.ShouldEqual(1);
                sqlContext.Books.Count().ShouldEqual(0);
            }
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
                    GetType().Name);

            using var sqlContext = new SqlDbContext(_sqlOptions);
            using var noSqlContext = new NoSqlDbContext(builder.Options);
            
            await sqlContext.Database.EnsureCreatedAsync();
            await noSqlContext.Database.EnsureCreatedAsync();
            var updater = new NoSqlBookUpdater(noSqlContext);

            //ATTEMPT
            var book = DddEfTestData.CreateDummyBookOneAuthor();
            sqlContext.Add(book);
            var numBooksChanged = updater.FindNumBooksChanged(sqlContext);
            await updater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(sqlContext, numBooksChanged,
                () => sqlContext.SaveChangesAsync());

            //VERIFY
            numBooksChanged.ShouldEqual(1);
            sqlContext.Books.Count().ShouldEqual(1);
            noSqlContext.Books.Find(book.BookId).ShouldNotBeNull();
        }

        [Fact]
        public async Task TestNoSqlBookUpdaterWithReviewsOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


            using var sqlContext = new SqlDbContext(_sqlOptions);
            using var noSqlContext = new NoSqlDbContext(builder.Options);

            await sqlContext.Database.EnsureCreatedAsync();
            await noSqlContext.Database.EnsureCreatedAsync();
            var updater = new NoSqlBookUpdater(noSqlContext);

            //ATTEMPT
            var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
            sqlContext.Add(book);
            var numBooksChanged = updater.FindNumBooksChanged(sqlContext);
            await updater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(sqlContext, numBooksChanged,
                () => sqlContext.SaveChangesAsync());

            //VERIFY
            numBooksChanged.ShouldEqual(1);
            sqlContext.Books.Count().ShouldEqual(1);
            var noSqlResult = noSqlContext.Books.Find(book.BookId);
            noSqlResult.ReviewsCount.ShouldEqual(2);
            noSqlResult.ReviewsAverageVotes.ShouldEqual(3);
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
                    GetType().Name);
            var connection = this.GetUniqueDatabaseConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();
            optionsBuilder.UseSqlServer(connection);
            var options = optionsBuilder.Options;
            using (var sqlContext = new SqlDbContext(_sqlOptions))
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            {
                sqlContext.CreateEmptyViaWipe();
                await noSqlContext.Database.EnsureCreatedAsync();
                var updater = new NoSqlBookUpdater(noSqlContext);

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var numBooksChanged = updater.FindNumBooksChanged(sqlContext);
                await updater.CallBaseSaveChangesWithNoSqlWriteInTransactionAsync(sqlContext, numBooksChanged,
                    () => sqlContext.SaveChangesAsync());

                //VERIFY
                numBooksChanged.ShouldEqual(1);
                sqlContext.Books.Count().ShouldEqual(1);
                noSqlContext.Books.Find(book.BookId).ShouldNotBeNull();
            }
        }
    }
}