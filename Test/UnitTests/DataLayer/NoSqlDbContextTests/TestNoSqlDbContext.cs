// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfClassesNoSql;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using EfCoreSqlAndCosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.NoSqlDbContextTests
{
    public class TestNoSqlDbContext
    {
        private readonly ITestOutputHelper _output;

        public TestNoSqlDbContext(ITestOutputHelper output)
        {
            _output = output;
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
                var ex = await Assert.ThrowsAsync<CosmosException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldStartWith("Response status code does not indicate success: NotFound (404)");
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
                    GetType().Name);

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestCosmosDbBulkLoadDirectAndParallelOk(bool goParallel)
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            CosmosClientOptions options = new CosmosClientOptions {AllowBulkExecution = goParallel};
            CosmosClient cosmosClient = new CosmosClient(config["endpoint"], config["authKey"], options);

            var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync(GetType().Name)).Database;
            var container = (await database.CreateContainerIfNotExistsAsync("BulkLoad", "/__partitionKey")).Container;

            //ATTEMPT
            var totalToWrite = 1000;
            var numParallel = 10;

            var books = NoSqlTestData.CreateDummyBooks(totalToWrite);
            if (goParallel)
            {
                var count = 0;
                using (new TimeThings(_output, "Parallel", totalToWrite))
                    for (int i = 0; i < totalToWrite / numParallel; i++)
                    {
                        List<Task> concurrentTasks = new List<Task>();
                        for (int j = 0; j < numParallel; j++)
                        {
                            concurrentTasks.Add(container.CreateItemAsync(books[i*numParallel + j]));
                        }
                        await Task.WhenAll(concurrentTasks);
                        count++;
                    }

                _output.WriteLine($"Went around {count} times");
            }
            else
            {
                using (new TimeThings(_output, "single", totalToWrite))
                {
                    foreach (var itemToInsert in books)
                    {
                        await container.CreateItemAsync(itemToInsert);
                    }
                }
            }

            //VERIFY
            var resultSet = container.GetItemQueryIterator<int>(new QueryDefinition("SELECT VALUE Count(c) FROM c"));
            var result = (await resultSet.ReadNextAsync()).First();
            _output.WriteLine($"BulkLoad now has {result} entries");
        }

        [Fact]
        public async Task TestCosmosDbBulkLoadDirectOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            CosmosClientOptions options = new CosmosClientOptions();
            CosmosClient cosmosClient = new CosmosClient(config["endpoint"], config["authKey"], options);

            var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync(GetType().Name)).Database;
            var container = (await database.CreateContainerIfNotExistsAsync("BulkLoad", "/__partitionKey")).Container;

            //ATTEMPT
            await TimeCosmosSdkWriteOut(10, container);
            await TimeCosmosSdkWriteOut(100, container);
            await TimeCosmosSdkWriteOut(100, container);
            await TimeCosmosSdkWriteOut(10, container);
            await TimeCosmosSdkWriteOut(100, container);

            //VERIFY
            var resultSet = container.GetItemQueryIterator<int>(new QueryDefinition("SELECT VALUE Count(c) FROM c"));
            var result = (await resultSet.ReadNextAsync()).First();
            _output.WriteLine($"BulkLoad now has {result} entries");
        }

        private async Task TimeCosmosSdkWriteOut(int totalToWrite, Container container)
        {
            var books = NoSqlTestData.CreateDummyBooks(totalToWrite);
            using (new TimeThings(_output, $"CosmosDb write out {totalToWrite} books", totalToWrite))
            {
                foreach (var itemToInsert in books)
                {
                    await container.CreateItemAsync(itemToInsert);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000)]
        public async Task TestCosmosDbDirectReadViaSqlOk(int offset)
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            CosmosClientOptions options = new CosmosClientOptions();
            CosmosClient cosmosClient = new CosmosClient(config["endpoint"], config["authKey"], options);

            var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync(GetType().Name)).Database;
            var container = (await database.CreateContainerIfNotExistsAsync("BulkLoad", "/__partitionKey")).Container;

            //ATTEMPT
            using (new TimeThings(_output, $"read all via Cosmos SQL"))
            {
                var booksQuery = container.GetItemQueryIterator<BookListNoSql>(new QueryDefinition($"SELECT * FROM c OFFSET {offset} LIMIT 100"));
                var books = (await booksQuery.ReadNextAsync()).ToList();
                //VERIFY
                _output.WriteLine($"Read {books.Count} Books");
            }
        }

        [Fact]
        public async Task TestCosmosDbBulkLoadViaEfCoreOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);

            using var noSqlContext = new NoSqlDbContext(builder.Options, "BulkLoad");

            //ATTEMPT
            await TimeEfCoreWrite(10, noSqlContext);
            await TimeEfCoreWrite(100, noSqlContext);
            await TimeEfCoreWrite(100, noSqlContext);
            await TimeEfCoreWrite(10, noSqlContext);
            await TimeEfCoreWrite(100, noSqlContext);

            //VERIFY
        }

        private async Task TimeEfCoreWrite(int totalToWrite, NoSqlDbContext noSqlContext)
        {
            using (new TimeThings(_output, $"Wrote out {totalToWrite} via EF Core"))
            {
                var books = NoSqlTestData.CreateDummyBooks(totalToWrite);
                noSqlContext.AddRange(books);
                await noSqlContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task TestCosmosDbReadViaEfCoreOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);

            using var noSqlContext = new NoSqlDbContext(builder.Options, "BulkLoad");

            //ATTEMPT
            await TimeReadBooksEfCore(10, noSqlContext);
            await TimeReadBooksEfCore(100, noSqlContext);
            await TimeReadBooksEfCore(100, noSqlContext);
            await TimeReadBooksEfCore(10, noSqlContext);
            await TimeReadBooksEfCore(100, noSqlContext);
        }

        private async Task TimeReadBooksEfCore(int numRead, NoSqlDbContext noSqlContext)
        {
            using (new TimeThings(_output, $"read {numRead} books via EF Core"))
            {
                var books = await noSqlContext.Books.Take(numRead).ToListAsync();
            }
        }

        [Fact]
        public async Task TestCosmosDbReadViaEfCoreWithOffsetOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);

            using var noSqlContext = new NoSqlDbContext(builder.Options, "BulkLoad");

            //ATTEMPT
            await TimeReadBooksWithOffsetEfCore(0, noSqlContext);
            await TimeReadBooksWithOffsetEfCore(100, noSqlContext);
            await TimeReadBooksWithOffsetEfCore(1000, noSqlContext);
            await TimeReadBooksWithOffsetEfCore(1000, noSqlContext);
            await TimeReadBooksWithOffsetEfCore(0, noSqlContext);
            await TimeReadBooksWithOffsetEfCore(1000, noSqlContext);
            await TimeReadBooksWithOffsetEfCore(100, noSqlContext);

        }

        private async Task TimeReadBooksWithOffsetEfCore(int offset, NoSqlDbContext noSqlContext)
        {
            using (new TimeThings(_output, $"read 100 with offset {offset} via EF Core"))
            {
                var books = await noSqlContext.Books.Skip(offset).Take(100).ToListAsync();
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
                    GetType().Name);

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