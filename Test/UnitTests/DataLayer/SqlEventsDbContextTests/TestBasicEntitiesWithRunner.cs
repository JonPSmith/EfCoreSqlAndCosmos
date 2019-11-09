// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using DataLayer.EfCode;
using DataLayerEvents.DomainEventCode;
using DataLayerEvents.DomainEvents;
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
        public void TestCreateBookAndCheckAddReviewHandlerOk()
        {
            //SETUP
            var options =
                SqliteInMemory.CreateOptionsWithLogging<SqlEventsDbContext>(x => _output.WriteLine(x.Message));
            var services = new ServiceCollection();
            services.RegisterEventRunner();
            services.RegisterEventHandlers(Assembly.GetAssembly( typeof(ReviewAddedHandler)));
            services.AddScoped(x =>
                new SqlEventsDbContext(options, x.GetRequiredService<IEventsRunner>()));
            var serviceProvider = services.BuildServiceProvider();
            using (var context = serviceProvider.GetRequiredService<SqlEventsDbContext>())
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var book = WithEventsEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                context.Add(book);
                context.SaveChanges();

                //VERIFY
                book.ReviewsCount.ShouldEqual(2);
                book.ReviewsAverageVotes.ShouldEqual(6.0/2);
            }
        }

        

    }
}