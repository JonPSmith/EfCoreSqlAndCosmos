// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using DataLayer.EfCode;
using EfCoreSqlAndCosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestNoSqlDbContext
    {
        [Fact]
        public void TestCosmosDbCatchFailedRequestOk()
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
                var numNoSqlChanges = context.SaveChanges();

                //VERIFY
                numNoSqlChanges.ShouldEqual(0);
            }
        }

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
                context.Books.Find(book.BookId).ShouldNotBeNull();
            }
        }

        [RunnableInDebugOnly]
        public async Task TestCosmosDbAzureCosmosDbOk()
        {
            //SETUP
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .Build();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["CosmosUrl"],
                    config["CosmosKey"],
                    nameof(TestNoSqlDbContext));

            using (var context = new NoSqlDbContext(builder.Options))
            {
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = NoSqlTestData.CreateDummyNoSqlBook();
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                context.Books.Find(book.BookId).ShouldNotBeNull();
            }
        }
    }
}