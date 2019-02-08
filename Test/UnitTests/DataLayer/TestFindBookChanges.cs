// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.NoSqlCode.Internal;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestFindBookChanges
    {

        [Fact]
        public void TestAddBookOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                context.Add(DddEfTestData.CreateDummyBookOneAuthor());
                var changes = BookChangeInfo.FindBookChanges(context.ChangeTracker.Entries());

                //VERIFY
                changes.Single().BookId.ShouldNotEqual(Guid.Empty);
                changes.Single().State.ShouldEqual(EntityState.Added);
            }
        }

        [Fact]
        public void TestUpdateDirectOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
            }
            using (var context = new SqlDbContext(options))
            {
                //ATTEMPT
                var book = context.Books.First();
                book.AddPromotion(123, "now 123");
                var changes = BookChangeInfo.FindBookChanges(context.ChangeTracker.Entries());

                //VERIFY
                changes.Single().BookId.ShouldNotEqual(Guid.Empty);
                changes.Single().State.ShouldEqual(EntityState.Modified);
            }
        }

        [Fact]
        public void TestUpdateViaNonBookEntityOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
            }
            using (var context = new SqlDbContext(options))
            {
                //ATTEMPT
                var book = context.Books.First();
                book.AddReview(5, "test", "test", context);
                var changes = BookChangeInfo.FindBookChanges(context.ChangeTracker.Entries());

                //VERIFY
                changes.Single().BookId.ShouldNotEqual(Guid.Empty);
                changes.Single().State.ShouldEqual(EntityState.Modified);
            }
        }

        [Fact]
        public void TestDeleteBookOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                context.Remove(context.Books.First());
                var changes = BookChangeInfo.FindBookChanges(context.ChangeTracker.Entries());

                //VERIFY
                changes.Single().BookId.ShouldNotEqual(Guid.Empty);
                changes.Single().State.ShouldEqual(EntityState.Deleted);
            }
        }

        [Fact]
        public void TestSoftDeleteBookOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                var book = context.Books.First();
                book.SoftDeleted = true;
                var changes = BookChangeInfo.FindBookChanges(context.ChangeTracker.Entries());

                //VERIFY
                changes.Single().BookId.ShouldNotEqual(Guid.Empty);
                changes.Single().State.ShouldEqual(EntityState.Deleted);
            }
        }


    }
}