// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlDbContextTests
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
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                book.AddReview(5, "test", "test");
                context.Add(book);
                context.SaveChanges();

                //ATTEMPT
                var aveReviews = context.Books
                    .Select(p => p.Reviews.Select(y => (double?) y.NumStars).Average())
                    .Single();

                //VERIFY
                aveReviews.ShouldEqual(5);
            }
        }

        [Fact]
        public void TestAverageReviewSqlServerOk()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptionsWithLogging<SqlDbContext>(
                log => _output.WriteLine(log.Message));
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.WipeAllDataFromDatabase();

                var book = DddEfTestData.CreateDummyBookOneAuthor();
                book.AddReview(5, "test", "test");
                context.Add(book);
                context.SaveChanges();

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