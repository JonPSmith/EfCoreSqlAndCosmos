// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Test.Helpers;
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
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    nameof(TestNoSqlDbContext));

            using (var context = new NoSqlDbContext(builder.Options))
            {
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                if (!context.Books.Any())
                {
                    var book = NoSqlTestData.CreateDummyNoSqlBook();
                    context.Add(book);
                    await context.SaveChangesAsync();
                }

                //VERIFY
                (await context.Books.CountAsync()).ShouldEqual(1);
            }
        }



        [Fact]
        public async Task TestCosmosDbCatchFailedRequestOk()
        {
            //SETUP
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
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
        public async Task TestCosmosDbCatchFailedRequestExecutionStrategy1OnOk()
        {
            //SETUP
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
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

        [Fact]
        public async Task TestCosmosDbCatchFailedRequestExecutionStrategy2OffOk()
        {
            //SETUP
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
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
    }
}