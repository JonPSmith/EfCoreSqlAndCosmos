// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.BooksCommon;
using ServiceLayer.BooksSql;
using ServiceLayer.BooksSql.QueryObjects;
using ServiceLayer.BooksSql.Services;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayer
{
    public class TestSqlBookRead
    {
        private ITestOutputHelper _output;
        private readonly DbContextOptions<SqlDbContext> _options;

        public TestSqlBookRead(ITestOutputHelper output)
        {
            _output = output;
            _options = this.CreateUniqueClassOptions<SqlDbContext>();
            using (var context = new SqlDbContext(_options))
            {
                context.Database.EnsureCreated();
                if (!context.Books.Any())
                {
                    context.SeedDatabaseDummyBooks(stepByYears: true);
                }
            }
        }

        [Fact]
        public void TestBookListDtoSelect()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogging<SqlDbContext>(x => _output.WriteLine(x.Message));
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                var dtos = context.Books.MapBookToDto().OrderBy(x => x.PublishedOn).ToList();

                //VERIFY
                dtos.Select(x => x.AuthorsOrdered).ShouldEqual(new [] { "Martin Fowler", "Martin Fowler", "Eric Evans", "Future Person" });
                dtos.Select(x => x.ReviewsAverageVotes).ShouldEqual(new double? [] { null, null, null, 5 });
            }
        }

        [Theory]
        [InlineData(OrderByOptions.ByPublicationDate)]
        [InlineData(OrderByOptions.ByPriceHigestFirst)]
        [InlineData(OrderByOptions.ByPriceLowestFirst)]
        [InlineData(OrderByOptions.ByVotes)]
        public async Task TestOrderByOk(OrderByOptions orderBy)
        {
            //SETUP
            using (var context = new SqlDbContext(_options))
            {
                var service = new SqlSqlListBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SqlSortFilterPageOptions
                {
                    OrderByOptions = orderBy
                }).ToListAsync();

                //VERIFY
            }
        }

        [Theory]
        [InlineData(100, 1)]
        [InlineData(2, 3)]
        [InlineData(3, 2)]
        public async Task TestPageOk(int pageSize, int pageNum)  //pageNum starts at 1
        {
            //SETUP
            using (var context = new SqlDbContext(_options))
            {
                var service = new SqlSqlListBooksService(context);

                //ATTEMPT
                var filterPageOptions = new SqlSortFilterPageOptions
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
            using (var context = new SqlDbContext(_options))
            {
                var service = new SqlSqlListBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SqlSortFilterPageOptions
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
            var year = Math.Min(DateTime.UtcNow.Year, DddEfTestData.DummyBookStartDate.AddYears(5).Year);
            using (var context = new SqlDbContext(_options))
            {
                var service = new SqlSqlListBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SqlSortFilterPageOptions
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
            using (var context = new SqlDbContext(_options))
            {
                var service = new SqlSqlListBooksService(context);

                //ATTEMPT
                var books = await service.SortFilterPage(new SqlSortFilterPageOptions
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