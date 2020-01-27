// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer.SqlDbContextTests
{
    public class TestSqlSaveChanges
    {
        private DbContextOptions<SqlDbContext> _sqlOptions;
        public TestSqlSaveChanges()
        {

            _sqlOptions = this.CreateUniqueClassOptions<SqlDbContext>();
            using (var context = new SqlDbContext(_sqlOptions))
            {
                context.Database.EnsureCreated();
                context.WipeAllDataFromDatabase();
            }
        }

        [Fact]
        public void TestSaveChangesAddNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                sqlContext.SaveChanges();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                var noSqlBook = noSqlContext.Books.SingleOrDefault(p => p.BookId == book.BookId);
                noSqlBook.ShouldNotBeNull();
                noSqlBook.AuthorsOrdered.ShouldEqual("Author1, Author2");
                noSqlBook.ReviewsCount.ShouldEqual(2);
            }
        }

        [Fact]
        public void TestSaveChangesDeleteNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                sqlContext.SaveChanges();

                //ATTEMPT
                sqlContext.Remove(book);
                sqlContext.SaveChanges();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(0);
                var noSqlBook = noSqlContext.Books.SingleOrDefault(p => p.BookId == book.BookId);
                noSqlBook.ShouldBeNull();
            }
        }

        [Fact]
        public void TestSaveChangesDirectUpdatesNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                sqlContext.SaveChanges();
            }
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                //ATTEMPT
                var book = sqlContext.Books.Single();
                book.PublishedOn = DddEfTestData.DummyBookStartDate.AddDays(1);
                sqlContext.SaveChanges();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                var noSqlBook = noSqlContext.Books.Single(p => p.BookId == book.BookId);
                noSqlBook.PublishedOn.ShouldEqual(DddEfTestData.DummyBookStartDate.AddDays(1));
                noSqlBook.AuthorsOrdered.ShouldEqual("Author1, Author2");
                noSqlBook.ReviewsCount.ShouldEqual(2);
            }
        }

        [Fact]
        public void TestSaveChangesIndirectUpdatesNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                sqlContext.SaveChanges();
            }
            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                //ATTEMPT
                var book = sqlContext.Books.Include(x => x.Reviews).Single();
                book.AddReview(5, "xxx","yyy");
                sqlContext.SaveChanges();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(1);
                var noSqlBook = noSqlContext.Books.Single(p => p.BookId == book.BookId);
                noSqlBook.ReviewsCount.ShouldEqual(3);
            }
        }

        [Fact]
        public void TestSaveChangesChangeAuthorTwoBooksNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


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
                sqlContext.SaveChanges();

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(4);
                var noSqlBooks = noSqlContext.Books.Where(p => bookIds.Contains(p.BookId)).ToList();
                noSqlBooks.Count.ShouldEqual(2);
                noSqlBooks.All(x => x.AuthorsOrdered == "Different Name").ShouldBeTrue();
            }
        }

        [Fact]
        public void TestSaveChangesSoftDeleteNoSqlOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var builder = new DbContextOptionsBuilder<NoSqlDbContext>()
                .UseCosmos(
                    config["endpoint"],
                    config["authKey"],
                    GetType().Name);


            using (var noSqlContext = new NoSqlDbContext(builder.Options))
            using (var sqlContext = new SqlDbContext(_sqlOptions, new NoSqlBookUpdater(noSqlContext)))
            {
                sqlContext.Database.EnsureCreated();
                noSqlContext.Database.EnsureCreated();
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                sqlContext.SaveChanges();

                //ATTEMPT
                book.SoftDeleted = true;
                sqlContext.SaveChanges();

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
        public void TestSaveChangesUpdatesNoSqlFail()
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
                sqlContext.Database.EnsureCreated();

                //ATTEMPT
                var book = DddEfTestData.CreateDummyBookTwoAuthorsTwoReviews();
                sqlContext.Add(book);
                var ex = Assert.Throws<CosmosException> (() => sqlContext.SaveChanges());

                //VERIFY
                sqlContext.Books.Count().ShouldEqual(0);
                ex.Message.ShouldStartWith("Response status code does not indicate success: 404 Substatus:");
            }
        }
    }
}