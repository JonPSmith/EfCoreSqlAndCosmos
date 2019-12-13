// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayerEvents.EfClasses;
using DataLayerEvents.EfCode;
using GenericEventRunner.ForSetup;
using Infrastructure.ConcurrencyHandlers;
using Infrastructure.EventHandlers;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlEventsDbContextTests
{
    public class TestEntitiesWithEventsConcurrency
    {
        private readonly ITestOutputHelper _output;

        public TestEntitiesWithEventsConcurrency(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestAddReviewCauseConcurrencyThrown()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>();
            context.Database.EnsureCreated();
            var book = WithEventsEfTestData.CreateDummyBookOneAuthor();
            context.Add(book);
            context.SaveChanges();

            //ATTEMPT
            book.AddReview(4, "OK", "me");
            //This simulates adding a review with NumStars of 2 before the AddReview 
            context.Database.ExecuteSqlRaw(
                "UPDATE Books SET ReviewsCount = @p0, ReviewsAverageVotes = @p1 WHERE BookId = @p2",
                1, 2, book.BookId);

            //VERIFY
            Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
        }

        [Fact]
        public void TestAddReviewConcurrencyFixed()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            var config = new GenericEventRunnerConfig
            {
                SaveChangesExceptionHandler = BookWithEventsConcurrencyHandler.HandleReviewConcurrency
            };
            var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>(config: config);
            context.Database.EnsureCreated();
            var book = WithEventsEfTestData.CreateDummyBookOneAuthor();
            context.Add(book);
            context.SaveChanges();

            book.AddReview(4, "OK", "me");
            //This simulates adding a review with NumStars of 2 before the AddReview 
            context.Database.ExecuteSqlRaw(
                "UPDATE Books SET ReviewsCount = @p0, ReviewsAverageVotes = @p1 WHERE BookId = @p2",
                1, 2, book.BookId);

            //ATTEMPT
            context.SaveChanges();

            //VERIFY
            var foundBook = context.Find<BookWithEvents>(book.BookId);
            foundBook.ReviewsCount.ShouldEqual(2);
            foundBook.ReviewsAverageVotes.ShouldEqual(6.0/2.0);
        }

        [Fact]
        public void TestAddSaveChangesExceptionHandlerButStillFailsOnOtherDbExceptions()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            var config = new GenericEventRunnerConfig
            {
                SaveChangesExceptionHandler = BookWithEventsConcurrencyHandler.HandleReviewConcurrency
            };
            var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>(config: config);
            context.Database.EnsureCreated();

            var review = new ReviewWithEvents(1,"hello", "Me", Guid.NewGuid());
            context.Add(review);

            //ATTEMPT
            var ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());

            //VERIFY
            ex.InnerException.Message.ShouldEqual("SQLite Error 19: 'FOREIGN KEY constraint failed'.");
        }

        //---------------------------------------------------
        //AuthorsOrdered concurrency handling

        [Fact]
        public void TestChangeAuthorOrderedCauseConcurrencyThrown()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>();
            context.Database.EnsureCreated();
            var books = WithEventsEfTestData.CreateDummyBooks(2);
            context.AddRange(books);
            context.SaveChanges();

            //ATTEMPT
            books.First().AuthorsLink.Last().Author.ChangeName("New common name");
            //This simulates changing the AuthorsOrdered value
            context.Database.ExecuteSqlRaw(
                "UPDATE Books SET AuthorsOrdered = @p0 WHERE BookId = @p1",
                "different author string", books.First().BookId);

            //VERIFY
            Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
        }

        [Fact]
        public void TestChangeAuthorOrderedConcurrencyFixed()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            var config = new GenericEventRunnerConfig
            {
                SaveChangesExceptionHandler = BookWithEventsConcurrencyHandler.HandleReviewConcurrency
            };
            var context = options.CreateDbWithDiForHandlers<SqlEventsDbContext, ReviewAddedHandler>(config: config);
            context.Database.EnsureCreated();
            var books = WithEventsEfTestData.CreateDummyBooks(2);
            context.AddRange(books);
            context.SaveChanges();

            //ATTEMPT
            books.First().AuthorsLink.Last().Author.ChangeName("New common name");
            //This simulates changing the AuthorsOrdered value
            context.Database.ExecuteSqlRaw(
                "UPDATE Books SET AuthorsOrdered = @p0 WHERE BookId = @p1",
                "different author string", books.First().BookId);

            //ATTEMPT
            context.SaveChanges();

            //VERIFY
            var readBooks = context.Books.ToList();
            readBooks.First().AuthorsOrdered.ShouldEqual("Author0000, New common name");
            readBooks.Last().AuthorsOrdered.ShouldEqual("Author0001, New common name");
        }
    }
}