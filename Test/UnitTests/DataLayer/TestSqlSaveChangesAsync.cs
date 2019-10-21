// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestSqlSaveChangesAsync
    {
        private DbContextOptions<SqlDbContext> _sqlOptions;
        public TestSqlSaveChangesAsync()
        {

            _sqlOptions = this.CreateUniqueClassOptions<SqlDbContext>();
            using (var context = new SqlDbContext(_sqlOptions))
            {
                context.Database.EnsureCreated();
                var filepath = TestData.GetFilePath(@"..\..\EfCoreSqlAndCosmos\wwwroot\AddUserDefinedFunctions.sql");
                context.ExecuteScriptFileInTransaction(filepath);
                context.WipeAllDataFromDatabase();
            }
        }

        [Fact]
        public async Task TestSaveChangesAsyncAddNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                await sqlContext.Database.EnsureCreatedAsync();
                await noSqlContext.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                var noSqlBook = noSqlContext.Books.SingleOrDefault(p => p.BookId == book.BookId);
                noSqlBook.ShouldNotBeNull();
                noSqlBook.AuthorsOrdered.ShouldEqual("Author1, Author2");
                noSqlBook.ReviewsCount.ShouldEqual(2);
            }
        }

        [Fact]
        public async Task TestSaveChangesDeleteNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                await sqlContext.SaveChangesAsync();

                //ATTEMPT
                sqlContext.Remove(book);
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(0);
                var noSqlBook = noSqlContext.Books.SingleOrDefault(p => p.BookId == book.BookId);
                noSqlBook.ShouldBeNull();
            }
        }

        [Fact]
        public async Task TestSaveChangesDirectUpdatesNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                await sqlContext.SaveChangesAsync();
            }
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                //ATTEMPT
                var book = sqlContext.Books.Single();
                book.PublishedOn = DddEfTestData.DummyBookStartDate.AddDays(1);
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                var noSqlBook = noSqlContext.Books.Single(p => p.BookId == book.BookId);
                noSqlBook.PublishedOn.ShouldEqual(DddEfTestData.DummyBookStartDate.AddDays(1));
                noSqlBook.AuthorsOrdered.ShouldEqual("Author1, Author2");
                noSqlBook.ReviewsCount.ShouldEqual(2);
            }
        }

        [Fact]
        public async Task TestSaveChangesIndirectUpdatesNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                await sqlContext.SaveChangesAsync();
            }
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                //ATTEMPT
                var book = sqlContext.Books.Single();
                book.AddReview(5, "xxx","yyy", sqlContext);
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                var noSqlBook = noSqlContext.Books.Single(p => p.BookId == book.BookId);
                noSqlBook.AuthorsOrdered.ShouldEqual("Author1, Author2");
                noSqlBook.ReviewsCount.ShouldEqual(3);
            }
        }

        [Fact]
        public async Task TestSaveChangesChangeAuthorTwoBooksNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                sqlContext.SeedDatabaseFourBooks();
            }
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                //ATTEMPT
                var author = sqlContext.Authors.Single(x => x.Name == "Martin Fowler");
                var bookIds = sqlContext.BookAuthors
                    .Where(x => x.AuthorId == author.AuthorId)
                    .Select(x => x.BookId).ToList();
                author.Name = "Different Name";
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(4);
                var noSqlBooks = noSqlContext.Books.Where(p => bookIds.Contains(p.BookId)).ToList();
                noSqlBooks.Count.ShouldEqual(2);
                noSqlBooks.All(x => x.AuthorsOrdered == "Different Name").ShouldBeTrue();
            }
        }

        [Fact]
        public async Task TestSaveChangesSoftDeleteNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    nameof(TestSqlSaveChanges));


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                await sqlContext.SaveChangesAsync();

                //ATTEMPT
                book.SoftDeleted = true;
                await sqlContext.SaveChangesAsync();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(0);
                sqlContext.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
                var noSqlBook = noSqlContext.Books.SingleOrDefault(p => p.BookId == book.BookId);
                noSqlBook.ShouldBeNull();
            }
        }

        //--------------------------------------------------------------
        //error situations


        [Fact]
        public async Task TestSaveChangesAsyncUpdatesNoSqlFail()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    "UNKNOWNDATABASENAME");


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                await sqlContext.Database.EnsureCreatedAsync();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookOneAuthor();
                sqlContext.Add(book);
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sqlContext.SaveChangesAsync());

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(0);
                ex.Message.ShouldEqual("1 books were changed in SQL, but the NoSQL changed 0");
            }
        }
    }
}