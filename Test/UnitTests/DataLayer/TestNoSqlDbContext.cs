// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Test.Helpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestNoSqlDbContext
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
                    nameof(TestNoSqlDbContext));

            using (var context = new NoSqlDbContext(builder.Options))
            {
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = NoSqlTestData.CreateDummyNoSqlBook();
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                (await context.Books.CountAsync(p => p.BookId == book.BookId)).ShouldEqual(1);
            }
        }



        [Fact]
        public async Task TestCosmosDbCatchFailedRequestOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    "UNKNOWNDATABASE");

            using (var context = new NoSqlDbContext(builder.Options))
            {
                //ATTEMPT
                var book = NoSqlTestData.CreateDummyNoSqlBook();
                context.Add(book);
                var ex = await Assert.ThrowsAsync<HttpException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldEqual("NotFound");
            }
        }

        [Fact]
        public async Task TestCosmosDbCatchFailedRequestExecutionStrategyOnOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    "UNKNOWNDATABASE",
                    options => options.ExecutionStrategy(c => new CosmosExecutionStrategy(c)));

            using (var context = new NoSqlDbContext(builder.Options))
            {
                //ATTEMPT
                var book = NoSqlTestData.CreateDummyNoSqlBook();
                context.Add(book);
                var ex = await Assert.ThrowsAsync<HttpException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldEqual("NotFound");
            }
        }
    }
}