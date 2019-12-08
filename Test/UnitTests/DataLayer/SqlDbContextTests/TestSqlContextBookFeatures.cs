// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;
using DataLayerEvents.EfCode;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlDbContextTests
{
    public class TestSqlContextBookFeatures
    {
        public TestSqlContextBookFeatures(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

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
            var options = this.CreateUniqueClassOptionsWithLogging<SqlDbContext>(
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
                    .Single();

                //VERIFY

                aveReviews.ShouldEqual(5);
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

        [Fact]
        public void TestRemoveReviewForLocalListOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();

                var bookWithReviews = context.SeedDatabaseFourBooks().Last();

                //ATTEMPT
                bookWithReviews.RemoveReview(bookWithReviews.Reviews.Last().ReviewId);

                //VERIFY
                bookWithReviews.Reviews.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestRemoveReviewForLocalListFail()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();

                var bookWithReviews = context.SeedDatabaseFourBooks().Last();

                //ATTEMPT
                var ex = Assert.Throws<InvalidOperationException>(() => bookWithReviews.RemoveReview(9999));

                //VERIFY
                ex.Message.ShouldEqual("The review with that key was not found in the book's Reviews.");
            }
        }
    }
}