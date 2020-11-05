// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.BooksCommon;
using ServiceLayer.BooksNoSql;
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
                var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await context.Books.Where(x => x.PublishedOn < DateTime.UtcNow).ToListAsync());

                //VERIFY
                books.Any().ShouldBeTrue();
                ex.Message.ShouldContain("Value cannot be null.");
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
                var books = await (service.SortFilterPageAsync(new NoSqlSortFilterPageOptions
                {
                    OrderByOptions = orderBy
                }));

                //VERIFY
                books.Any().ShouldBeTrue();
            }
        }

        //[Fact]
        //public async Task TestOrderByVotesOneNullOk()
        //{
        //    //SETUP
        //    var config = AppSettings.GetConfiguration();
        //    var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
        //        .UseCosmos(
        //            config["endpoint"],
        //            config["authKey"],
        //            GetType().Name+nameof(TestOrderByVotesOneNullOk));

        //    using (var context = new NoSqlDbContext(builder.Options))
        //    {
        //        await context.Database.EnsureCreatedAsync();
        //        var books = new List<BookListNoSql>
        //        {
        //            NoSqlTestData.CreateDummyNoSqlBook(),
        //            NoSqlTestData.CreateDummyNoSqlBook(5)
        //        };
        //        context.AddRange(books);
        //        await context.SaveChangesAsync();

        //        var service = new ListNoSqlBooksService(context);

        //        //ATTEMPT
        //        var foundBooks = await (service.SortFilterPageAsync(new NoSqlSortFilterPageOptions
        //        {
        //            OrderByOptions = OrderByOptions.ByVotes
        //        })).ToListAsync();

        //        //VERIFY
        //        context.Books.Select(x => x.ReviewsAverageVotes).ToArray().ShouldEqual(new double []{ 0, 5 });
        //    }
        //}

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
                var filterPageOptions = new NoSqlSortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByPriceLowestFirst,
                    PageSize = pageSize,
                    PageNum = pageNum
                };
                var temp = await service.SortFilterPageAsync(filterPageOptions); //to set the PrevCheckState
                filterPageOptions.PageNum = pageNum;
                var books = await service.SortFilterPageAsync(filterPageOptions);

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
                var books = await service.SortFilterPageAsync(new NoSqlSortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByPublicationDate,
                    FilterBy = BooksFilterBy.ByPublicationYear,
                    FilterValue = year.ToString()
                });

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
                var books = await service.SortFilterPageAsync(new NoSqlSortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByPublicationDate,
                    FilterBy = BooksFilterBy.ByPublicationYear,
                    FilterValue = "Coming Soon"
                });

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
                var books = await service.SortFilterPageAsync(new NoSqlSortFilterPageOptions
                {
                    OrderByOptions = OrderByOptions.ByVotes,
                    FilterBy = BooksFilterBy.ByVotes,
                    FilterValue = "2"
                });

                //VERIFY
                books.All(x => x.ReviewsAverageVotes > 2).ShouldBeTrue();
            }
        }



    }
}