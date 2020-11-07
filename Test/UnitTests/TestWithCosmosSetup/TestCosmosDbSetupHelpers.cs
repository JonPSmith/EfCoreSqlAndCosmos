// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestWithCosmosSetup
{
    public class TestCosmosDbSetupHelpers
    {
        private readonly ITestOutputHelper _output;

        public TestCosmosDbSetupHelpers(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("CosmosEmulator", "TestCosmosDb")]
        [InlineData("AzureUnitTest", "AzureTestDatabase")]
        [InlineData("BadSection", null)]
        public void TestGetConfigWithCheck(string groupName, string expectedDatabaseName)
        {
            //SETUP

            //ATTEMPT
            var settings = groupName.GetConfigWithCheck();

            //VERIFY
            settings?.Database.ShouldEqual(expectedDatabaseName);
        }


        [Theory]
        [InlineData(null, null, true)]
        [InlineData("BadDb", null, false)]
        [InlineData(null, "BadContainer", false)]
        public async Task TestEmulatorCosmosContainerExistsOk(string databaseName, string containerName, bool shouldExist)
        {
            //SETUP

            //ATTEMPT
            var exists = await "CosmosEmulator".CheckCosmosDbContainerExistsAsync(databaseName ?? GetType().Name, containerName);

            //VERIFY
            exists.ShouldEqual(shouldExist);
        }

        [Fact]
        public async Task TestEmulatorCosmosDbOptionsOk()
        {
            //SETUP
            var options = "CosmosEmulator".GetCosmosEfCoreOptions<NoSqlDbContext>(GetType().Name);

            using (var context = new NoSqlDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = NoSqlTestData.CreateDummyNoSqlBook();
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                (await context.Books.CountAsync()).ShouldBeInRange(1, 10000);
            }
        }

        [Fact]
        public async Task TestAzureUnitTestCosmosDbOptionsOk()
        {
            //SETUP
            var options = "AzureUnitTest".GetCosmosEfCoreOptions<NoSqlDbContext>(GetType().Name);

            using (var context = new NoSqlDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = NoSqlTestData.CreateDummyNoSqlBook();
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                (await context.Books.CountAsync()).ShouldBeInRange(1, 10000);
            }
        }

        [Fact]
        public async Task TestAzureUnitTestCosmosWriteManyOk()
        {
            //SETUP
            var options = "AzureUnitTest".GetCosmosEfCoreOptions<NoSqlDbContext>(GetType().Name);

            using (var context = new NoSqlDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                await TimeEfCoreWrite(10, context);
                await TimeEfCoreWrite(100, context);
                await TimeEfCoreWrite(100, context);
                await TimeEfCoreWrite(10, context);
                await TimeEfCoreWrite(100, context);

                //VERIFY
                (await context.Books.CountAsync()).ShouldBeInRange(1, 10000);
            }
        }

        private async Task TimeEfCoreWrite(int totalToWrite, NoSqlDbContext context)
        {
            using (new TimeThings(_output, $"Wrote out {totalToWrite} via EF Core"))
            {
                var books = NoSqlTestData.CreateDummyBooks(totalToWrite);
                context.AddRange(books);
                await context.SaveChangesAsync();
            }
        }


        [Fact]
        public async Task TestCosmosDbReadViaEfCoreOk()
        {
            //SETUP
            var options = "AzureUnitTest".GetCosmosEfCoreOptions<NoSqlDbContext>(GetType().Name);

            using var noSqlContext = new NoSqlDbContext(options);

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
                var books = await noSqlContext.Books.AsNoTracking().Take(numRead).ToListAsync();
            }
        }


        [Fact]
        public async Task TestAzureUnitTestCosmosDbOptionsDeleteDatabaseAndRecreateOk()
        {
            //SETUP
            var options = "AzureUnitTest".GetCosmosEfCoreOptions<NoSqlDbContext>(GetType().Name +"_different");

            using (var context = new NoSqlDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = NoSqlTestData.CreateDummyNoSqlBook();
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                (await context.Books.CountAsync()).ShouldEqual(1);
            }
        }
    }
}