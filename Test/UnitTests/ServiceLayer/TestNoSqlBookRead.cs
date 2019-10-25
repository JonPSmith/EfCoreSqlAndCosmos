// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.BooksCommon;
using ServiceLayer.BooksNoSql.Services;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayer
{
    public class TestNoSqlBookRead
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<NoSqlDbContext> _options;

        public TestNoSqlBookRead(ITestOutputHelper output)
        {
            _output = output;
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);
            _options = builder.Options;

            using var context = new NoSqlDbContext(_options);
            context.Database.EnsureCreated();
            if (context.Books.Select(_ => 1).AsEnumerable().Count() < 5)
            {
                var books = NoSqlTestData.CreateDummyBooks(10,true);
                context.AddRange(books);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task TestDateFilterFailsDateTimeUtcNow()
        {
            //SETUP
            using (var context = new NoSqlDbContext(_options))
            {
                //ATTEMPT
                var now = DateTime.UtcNow;
                var books = await context.Books.Where(x => x.PublishedOn < now).ToListAsync();
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await context.Books.Where(x => x.PublishedOn < DateTime.UtcNow).ToListAsync());

                //VERIFY
                books.Any().ShouldBeTrue();
                ex.Message.ShouldContain("could not be translated.");
            }
        }

        [RunnableInDebugOnly]
        public async Task DeleteNoSqlDatabase()
        {
            using var context = new NoSqlDbContext(_options);
            await context.Database.EnsureDeletedAsync();
        }

        [Theory]
        [InlineData(OrderByOptions.ByPublicationDate)]
        [InlineData(OrderByOptions.ByPriceHigestFirst)]
        [InlineData(OrderByOptions.ByPriceLowestFirst)]
        [InlineData(OrderByOptions.ByVotes)]
        public async Task TestOrderByOk(OrderByOptions orderBy)
        {
            //SETUP
            using (var context = new NoSqlDbContext(_options))
            {
                var service = new ListNoSqlBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SortFilterPageOptions
                {
                    OrderByOptions = orderBy
                }).ToListAsync();

                //VERIFY
            }
        }

        [Theory]
        [InlineData(100,1)]
        [InlineData(2, 3)]
        [InlineData(3, 2)]
        public async Task TestPageOk(int pageSize, int pageNum)  //pageNum starts at 1
        {
            //SETUP
            pageSize.ShouldBeInRange(1, 1000);
            using (var context = new NoSqlDbContext(_options))
            {
                var service = new ListNoSqlBooksService(context);

                //ATTEMPT
                var filterPageOptions = new SortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByPriceLowestFirst,
                    PageSize = pageSize,
                    PageNum = pageNum
                };
                var temp = service.SortFilterPage(filterPageOptions); //to set the PrevCheckState
                filterPageOptions.PageNum = pageNum;
                var books = await service.SortFilterPage(filterPageOptions).ToListAsync();

                //VERIFY
                books.Count.ShouldEqual(Math.Min(pageSize, books.Count));
                books.First().ActualPrice.ShouldEqual(1 + pageSize * (pageNum - 1));
            }
        }

        [Fact]
        public async Task TestFilterDatesOk()
        {
            //SETUP
            var year = Math.Min(DateTime.UtcNow.Year, DddEfTestData.DummyBookStartDate.AddYears(5).Year);
            using (var context = new NoSqlDbContext(_options))
            {
                var service = new ListNoSqlBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByPublicationDate,
                    FilterBy = BooksFilterBy.ByPublicationYear,
                    FilterValue = year.ToString()
                }).ToListAsync();

                //VERIFY
                books.Single().PublishedOn.Year.ShouldEqual(year);
            }
        }

        [Fact]
        public async Task TestFilterDatesFutureOk()
        {
            //SETUP
            using (var context = new NoSqlDbContext(_options))
            {
                var service = new ListNoSqlBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByPublicationDate,
                    FilterBy = BooksFilterBy.ByPublicationYear,
                    FilterValue = "Coming Soon"
                }).ToListAsync();

                //VERIFY
                books.All(x => x.PublishedOn > DateTime.UtcNow).ShouldBeTrue();
            }
        }

        [Fact]
        public async Task TestFilterVotesOk()
        {
            //SETUP
            var year = DddEfTestData.DummyBookStartDate.AddYears(5).Year;
            using (var context = new NoSqlDbContext(_options))
            {
                var service = new ListNoSqlBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByVotes,
                    FilterBy = BooksFilterBy.ByVotes,
                    FilterValue = "2"
                }).ToListAsync();

                //VERIFY
                books.All(x => x.ReviewsAverageVotes > 2).ShouldBeTrue();
            }
        }



    }
}