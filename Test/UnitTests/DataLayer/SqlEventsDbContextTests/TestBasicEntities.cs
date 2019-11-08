// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.DomainEvents;
using DataLayer.EfCode;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlEventsDbContextTests
{
    public class TestBasicEntities
    {
        private readonly ITestOutputHelper _output;

        public TestBasicEntities(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreateDatabaseAndSeedOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogging<SqlEventsDbContext>(x => _output.WriteLine(x.Message));
            using (var context = new SqlEventsDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.SeedDatabaseFourBooks();

                //VERIFY
                context.Books.Count().ShouldEqual(4);
            }
        }

        [Fact]
        public void TestBookAddReviewCausesEventOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            using (var context = new SqlEventsDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();
                book.ReturnEventsAndThenClear();

                //ATTEMPT
                book.AddReview(5, "test", "someone");

                //VERIFY
                var dEvent = book.ReturnEventsAndThenClear().Single();
                dEvent.ShouldBeType<BookReviewsChangedEvent>();
            }
        }

    }
}