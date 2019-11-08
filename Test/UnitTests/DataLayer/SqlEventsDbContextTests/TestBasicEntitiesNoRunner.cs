// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using DataLayerEvents.DomainEvents;
using DataLayerEvents.EfCode;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlEventsDbContextTests
{
    public class TestBasicEntitiesNoRunner
    {
        private readonly ITestOutputHelper _output;

        public TestBasicEntitiesNoRunner(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreateDatabaseAndSeedOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogging<SqlEventsDbContext>(x => _output.WriteLine(x.Message));
            using (var context = new SqlEventsDbContext(options, null))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.SeedDatabaseFourBooks();

                //VERIFY
                context.Books.Count().ShouldEqual(4);
            }
        }

        [Fact]
        public void TestCreateBookAndCheckPartsOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogging<SqlEventsDbContext>(x => _output.WriteLine(x.Message));
            using (var context = new SqlEventsDbContext(options, null))
            {
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();
            }
            using (var context = new SqlEventsDbContext(options, null))
            {
                //ATTEMPT
                var bookWithRelationships = context.Books
                    .Include(p => p.AuthorsLink).ThenInclude(p => p.Author)
                    .Include(p => p.Reviews)
                    .Single();

                //VERIFY
                bookWithRelationships.AuthorsLink.Select(y => y.Author.Name).OrderBy(x => x).ToArray()
                    .ShouldEqual(new[]{ "Author1" , "Author2" });
                bookWithRelationships.Reviews.Count().ShouldEqual(2);
            }
        }

        [Fact]
        public void TestBookAddReviewCausesEventOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            using (var context = new SqlEventsDbContext(options, null))
            {
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();
                book.GetBeforeSaveEventsThenClear();

                //ATTEMPT
                book.AddReview(5, "test", "someone");

                //VERIFY
                var dEvent = book.GetBeforeSaveEventsThenClear().Single();
                dEvent.ShouldBeType<BookReviewsChangedEvent>();
            }
        }

        [Fact]
        public void TestBookRemoveReviewCausesEventOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            using (var context = new SqlEventsDbContext(options, null))
            {
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();
                book.GetBeforeSaveEventsThenClear();

                //ATTEMPT
                book.RemoveReview(book.Reviews.First());

                //VERIFY
                var dEvent = book.GetBeforeSaveEventsThenClear().Single();
                dEvent.ShouldBeType<BookReviewsChangedEvent>();
            }
        }

        [Fact]
        public void TestAuthorChangeNameCausesEventOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            using (var context = new SqlEventsDbContext(options, null))
            {
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();
                var author = book.AuthorsLink.First().Author;
                author.GetBeforeSaveEventsThenClear();

                //ATTEMPT
                author.Name = "new name";

                //VERIFY
                var dEvent = author.GetBeforeSaveEventsThenClear().Single();
                dEvent.ShouldBeType<AuthorNameUpdatedEvent>();
            }
        }

    }
}