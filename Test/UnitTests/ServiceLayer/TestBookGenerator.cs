// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.DatabaseServices.Concrete;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayer
{
    public class TestBookGenerator
    {

        [Fact]
        public async Task TestWriteBooksAsyncFillsInCacheValuesOk()
        {
            //SETUP
            var sqlOptions = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var sqlContext = new SqlDbContext(sqlOptions))
            {
                sqlContext.Database.EnsureCreated();

                var filepath = TestData.GetFilePath("10ManningBooks.json");

                var generator = new BookGenerator(sqlOptions, null);

                //ATTEMPT
                await generator.WriteBooksAsync(filepath, 10, true, default(CancellationToken));

                //VERIFY
                var booksWithReviews = sqlContext.Books.Include(x => x.Reviews).ToList();
                booksWithReviews.Count().ShouldEqual(10);
                booksWithReviews.ForEach(x => x.ReviewsCount.ShouldEqual(x.Reviews.Count()));
                booksWithReviews.ForEach(x => x.AuthorsOrdered.ShouldNotBeNull());
            }
        }

        [Fact]
        public async Task TestWriteBooksAsyncPromotionValueOk()
        {
            //SETUP
            var sqlOptions = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var sqlContext = new SqlDbContext(sqlOptions))
            {
                sqlContext.Database.EnsureCreated();

                var filepath = TestData.GetFilePath("10ManningBooks.json");

                var generator = new BookGenerator(sqlOptions, null);

                //ATTEMPT
                await generator.WriteBooksAsync(filepath, 10, true, default(CancellationToken));

                //VERIFY
                var booksWithPromo = sqlContext.Books.Where(x => x.PromotionalText != null).ToList();
                booksWithPromo.ForEach(x => x.ActualPrice.ShouldEqual(x.OrgPrice * 0.5m));
            }
        }


        [Theory]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(20)]
        public async Task TestWriteBooksAsyncCorrectAmountOk(int numBooks)
        {
            //SETUP
            var noSqlOptions = this.GetCosmosDbToEmulatorOptions<NoSqlDbContext>();
            var sqlOptions = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var noSqlContext = new NoSqlDbContext(noSqlOptions))
            using (var sqlContext = new SqlDbContext(sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                await noSqlContext.Database.EnsureDeletedAsync();
                await noSqlContext.Database.EnsureCreatedAsync();

                var filepath = TestData.GetFilePath("10ManningBooks.json");

                var generator = new BookGenerator(sqlOptions, noSqlOptions);

                //ATTEMPT
                await generator.WriteBooksAsync(filepath, numBooks, true, default(CancellationToken));

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(numBooks);
                noSqlContext.Books.Select(_ => 1).AsEnumerable().Count().ShouldEqual(numBooks);
            }
        }

        [Fact]
        public async Task TestWriteBooksAsyncCalledTwiceOk()
        {
            //SETUP
            var noSqlOptions = this.GetCosmosDbToEmulatorOptions<NoSqlDbContext>();
            var sqlOptions = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var sqlContext = new SqlDbContext(sqlOptions, null))
            {
                sqlContext.Database.EnsureCreated();

                var filepath = TestData.GetFilePath("10ManningBooks.json");

                var generator = new BookGenerator(sqlOptions, noSqlOptions);

                //ATTEMPT
                await generator.WriteBooksAsync(filepath, 8, true, default(CancellationToken));
                await generator.WriteBooksAsync(filepath, 12, true, default(CancellationToken));

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(12);
            }
        }

        [Fact]
        public async Task TestWriteBooksAsyncCancelOk()
        {
            //SETUP
            var noSqlOptions = this.GetCosmosDbToEmulatorOptions<NoSqlDbContext>();
            var sqlOptions = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var sqlContext = new SqlDbContext(sqlOptions, null))
            {
                sqlContext.Database.EnsureCreated();

                var filepath = TestData.GetFilePath("10ManningBooks.json");

                var generator = new BookGenerator(sqlOptions, noSqlOptions);

                //ATTEMPT
                await generator.WriteBooksAsync(filepath, 8, true, default(CancellationToken));
                await generator.WriteBooksAsync(filepath, 12, true, new CancellationToken(true));

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(8);
            }
        }

    }
}