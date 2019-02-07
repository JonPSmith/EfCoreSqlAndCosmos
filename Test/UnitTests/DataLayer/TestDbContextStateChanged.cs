// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestDbContextStateChanged
    {
        public TestDbContextStateChanged(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        Guid _bookId = Guid.Empty;
        EntityState _newState = EntityState.Detached;
        private void StateChangeAction(object sender, EntityStateChangedEventArgs change)
        {
            if (change.Entry.Entity is Book book)
            {
                _bookId = book.BookId;
                _newState = change.OldState;
            }
        }

        [Fact]
        public void TestAddBookOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SqlDbContext>();
            using (var context = new SqlDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                context.ChangeTracker.StateChanged += StateChangeAction;

                //ATTEMPT
                context.Add(DddEfTestData.CreateDummyBookOneAuthor());
                context.SaveChanges();

                //VERIFY
                _bookId.ShouldNotEqual(Guid.Empty);
                _newState.ShouldEqual(EntityState.Added);
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

                context.ChangeTracker.StateChanged += StateChangeAction;

                //ATTEMPT
                context.Remove(context.Books.First());
                context.SaveChanges();

                //VERIFY
                _bookId.ShouldNotEqual(Guid.Empty);
                _newState.ShouldEqual(EntityState.Deleted);
            }
        }


    }
}