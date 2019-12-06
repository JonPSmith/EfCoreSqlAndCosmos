// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayerEvents.EfCode;
using Infrastructure.EventHandlers;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlEventsDbContextTests
{
    public class TestEntitiesWithEvents
    {
        private readonly ITestOutputHelper _output;

        public TestEntitiesWithEvents(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreateBookWithReviewsAndCheckReviewAddedHandlerOk()
        {
            //SETUP
            var showLog = false;
            var options =
                SqliteInMemory.CreateOptionsWithLogging<SqlEventsDbContext>(x =>
                {
                    if (showLog)
                        _output.WriteLine(x.DecodeMessage());
                });
            var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>();
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                showLog = true;
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();

                //VERIFY
                book.ReviewsCount.ShouldEqual(2);
                book.ReviewsAverageVotes.ShouldEqual(6.0/2);
            }
        }

        [Fact]
        public void TestAddReviewToCreatedBookAndCheckReviewAddedHandlerOk()
        {
            //SETUP
            var showLog = false;
            var options =
                SqliteInMemory.CreateOptionsWithLogging<SqlEventsDbContext>(x =>
                {
                    if (showLog)
                        _output.WriteLine(x.DecodeMessage());
                });
            var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>();
            context.Database.EnsureCreated();
            var book = WithEventsEfTestData.CreateDummyBookOneAuthor();
            context.Add(book);
            context.SaveChanges();

            //ATTEMPT
            showLog = true;
            book.AddReview(4, "OK", "me");
            context.SaveChanges();

            //VERIFY
            book.ReviewsCount.ShouldEqual(1);
            book.ReviewsAverageVotes.ShouldEqual(4);
        }

        [Fact]
        public void TestAddReviewToExistingBookAndCheckReviewAddedHandlerOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            {
                var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>();
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookOneAuthor();
                context.Add(book);
                context.SaveChanges();
            }
            {
                var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>();
                var book = context.Books.Single();

                //ATTEMPT
                book.AddReview(4, "OK", "me", context);
                context.SaveChanges();

                //VERIFY
                book.ReviewsCount.ShouldEqual(1);
                book.ReviewsAverageVotes.ShouldEqual(4);
            }
        }

        [Fact]
        public void TestCreateBookWithReviewsThenRemoveReviewCheckReviewRemovedHandlerOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            {
                var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>(); 
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();

                //ATTEMPT
                var reviewToRemove = book.Reviews.First();
                book.RemoveReview(reviewToRemove.ReviewId);
                context.SaveChanges();

                //VERIFY
                book.ReviewsCount.ShouldEqual(1);
                book.ReviewsAverageVotes.ShouldEqual(1);
            }
        }

    }
}