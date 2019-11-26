// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using DataLayerEvents.DomainEventCode;
using DataLayerEvents.EfCode;
using Infrastructure.EventHandlers;
using Infrastructure.EventRunnerCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlEventsDbContextTests
{
    public class TestBasicEntitiesWithRunner
    {
        private readonly ITestOutputHelper _output;

        public TestBasicEntitiesWithRunner(ITestOutputHelper output)
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
            var services = new ServiceCollection();
            services.RegisterEventRunner();
            services.RegisterEventHandlers(Assembly.GetAssembly( typeof(ReviewAddedHandler)));
            services.AddScoped(x =>
                new SqlEventsDbContext(options, x.GetRequiredService<IEventsRunner>()));
            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<SqlEventsDbContext>();
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
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            {
                var context = CreateSqlEventsDbContextWithServices(options);
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookOneAuthor();
                context.Add(book);
                context.SaveChanges();

                //ATTEMPT
                book.AddReview(4, "OK", "me");
                context.SaveChanges();

                //VERIFY
                book.ReviewsCount.ShouldEqual(1);
                book.ReviewsAverageVotes.ShouldEqual(4);
            }
        }

        [Fact]
        public void TestAddReviewToExistingBookAndCheckReviewAddedHandlerOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlEventsDbContext>();
            {
                var context = CreateSqlEventsDbContextWithServices(options);
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookOneAuthor();
                context.Add(book);
                context.SaveChanges();
            }
            {
                var context = CreateSqlEventsDbContextWithServices(options);
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
                var context = CreateSqlEventsDbContextWithServices(options); 
                context.Database.EnsureCreated();
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();

                //ATTEMPT
                var reviewToRemove = book.Reviews.First();
                book.RemoveReview(reviewToRemove);
                context.SaveChanges();

                //VERIFY
                book.ReviewsCount.ShouldEqual(1);
                book.ReviewsAverageVotes.ShouldEqual(1);
            }
        }


        private static SqlEventsDbContext CreateSqlEventsDbContextWithServices(
            DbContextOptions<SqlEventsDbContext> options)
        {
            var services = new ServiceCollection();
            services.RegisterEventRunner();
            services.RegisterEventHandlers(Assembly.GetAssembly(typeof(ReviewAddedHandler)));
            services.AddScoped(x =>
                new SqlEventsDbContext(options, x.GetRequiredService<IEventsRunner>()));
            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<SqlEventsDbContext>();
            return context;
        }
    }
}