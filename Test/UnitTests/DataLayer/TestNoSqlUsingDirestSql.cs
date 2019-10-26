// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataLayer.EfCode;
using EfCoreSqlAndCosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestNoSqlUsingDirestSql
    {
        private ITestOutputHelper _output;

        public TestNoSqlUsingDirestSql(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestGetCosmosClientOnMainDatabasesOk()
        {
            //SETUP
            var mainConfig = AppSettings.GetConfiguration("../EfCoreSqlAndCosmos/");
            var noSQlDatabaseName = mainConfig["database"];
            var noSqlOptions = noSQlDatabaseName.GetCosmosDbToEmulatorOptions<NoSqlDbContext>();
            using (var noSqlDbContext = new NoSqlDbContext(noSqlOptions))
            {
                var cosmosClient = noSqlDbContext.Database.GetCosmosClient();
                var database = cosmosClient.GetDatabase(noSQlDatabaseName);
                var container = database.GetContainer(nameof(NoSqlDbContext));

                //ATTEMPT
                _output.WriteLine($"SQL count = {noSqlDbContext.Books.Select(_ => 1).AsEnumerable().Count()}");
                using (new TimeThings(_output, "NoSQL count, EF Core"))
                {
                    var result = noSqlDbContext.Books.Select(_ => 1).AsEnumerable().Count();
                }

                using (new TimeThings(_output, "NoSQL count, EF Core"))
                {
                    var result = noSqlDbContext.Books.Select(_ => 1).AsEnumerable().Count();
                }

                using (new TimeThings(_output, "NoSQL count, via client"))
                {
                    var resultSet = container.GetItemQueryIterator<int>(new QueryDefinition("SELECT VALUE Count(c) FROM c"));
                    var result = (await resultSet.ReadNextAsync()).First();
                }

                using (new TimeThings(_output, "NoSQL count, via client"))
                {
                    var resultSet = container.GetItemQueryIterator<int>(new QueryDefinition("SELECT VALUE Count(c) FROM c"));
                    var result = (await resultSet.ReadNextAsync()).First();
                }
            }

            var sqlConnection = mainConfig.GetConnectionString("BookSqlConnection");
            var builder = new DbContextOptionsBuilder<SqlDbContext>()
                .UseSqlServer(sqlConnection);
            using (var sqlContext = new SqlDbContext(builder.Options))
            {
                _output.WriteLine($"SQL count = {sqlContext.Books.Count()}");
                using (new TimeThings(_output, "SQL count"))
                {
                    var num = await sqlContext.Books.CountAsync();
                }

                using (new TimeThings(_output, "SQL count"))
                {
                    var num = await sqlContext.Books.CountAsync();
                }
                
            }
        }
    }
}