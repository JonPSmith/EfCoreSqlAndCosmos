// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestBookAverage
    {
        public TestBookAverage(ITestOutputHelper output)
        {
            _output = output;
        }

        private ITestOutputHelper _output;

        [Fact]
        public void TestAverageReviewOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogging<SqlDbContext>(x => _output.WriteLine(x.Message));
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                var aveReviews = context.Books
                    .Select(p => p.Reviews.Select(y => (double?) y.NumStars).Average())
                    .ToList();

                //VERIFY
                aveReviews.ShouldEqual(new List<double?>{null, null, null, 5});
            }
        }

        [Fact]
        public void TestAverageReviewSqlServerOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration("..\\EfCoreSqlAndCosmos");
            var connection = config.GetConnectionString("BookSqlConnection");
            var builder = new DbContextOptionsBuilder<SqlDbContext>()
                .UseSqlServer(connection)
                .UseLoggerFactory(new LoggerFactory(new[]
                    {new MyLoggerProviderActionOut(x => _output.WriteLine(x.Message))}));
            using (var context = new SqlDbContext(builder.Options))
            {

                //ATTEMPT
                var aveReviews = context.Books
                    .Select(p => p.Reviews.Select(y => (double?)y.NumStars).Average())
                    .ToList();

                //VERIFY
            }
        }

        [Fact]
        public void TestCreateBooksOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.SeedDatabaseFourBooks();

                //VERIFY
                context.Books.Count().ShouldEqual(4);
            }
        }
    }
}