// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
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
    public class TestSqlSaveChanges
    {
        [Fact]
        public void TestSaveChangesUpdatesNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));

            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(options, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                sqlContext.SaveChanges();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                noSqlContext.Books.Count(p => p.BookId == book.BookId).ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSaveChangesUpdatesNoSqlFail()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    "UNKNOWNDATABASENAME");

            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(options, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var ex = Assert.Throws<WebException>( () => sqlContext.SaveChanges());

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(0);
            }
        }

        [Fact]
        public async Task TestSaveChangesAsyncUpdatesNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));

            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(options, new NoSqlBookUpdater(noSqlContext)))
            {
                await sqlContext.Database.EnsureCreatedAsync();
                await noSqlContext.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                (await noSqlContext.Books.CountAsync(p => p.BookId == book.BookId)).ShouldEqual(1);
            }
        }

        [Fact]
        public async Task TestSaveChangesAsyncUpdatesNoSqlFail()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    "UNKNOWNDATABASENAME");

            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(options, new NoSqlBookUpdater(noSqlContext)))
            {
                await sqlContext.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var ex = await Assert.ThrowsAsync<HttpException>(async () => await sqlContext.SaveChangesAsync());

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(0);
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
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(options, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.CreateEmptyViaWipe();
                await noSqlContext.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                (await noSqlContext.Books.CountAsync(p => p.BookId == book.BookId)).ShouldEqual(1);
            }
        }




    }
}