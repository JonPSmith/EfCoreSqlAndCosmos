// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using DataLayer.EfClassesNoSql;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Test")]
namespace DataLayer.NoSqlCode
{
    internal class ApplyChangeToNoSql
    {
        private readonly SqlDbContext _sqlContext;
        private readonly NoSqlDbContext _noSqlContext;

        public ApplyChangeToNoSql(SqlDbContext sqlContext, NoSqlDbContext noSqlContext)
        {
            _sqlContext = sqlContext;
            _noSqlContext = noSqlContext;
        }

        public void UpdateNoSql(IImmutableList<BookChangeInfo> booksToUpdate)
        {
            if (_noSqlContext == null || !booksToUpdate.Any()) return;

            foreach (var bookToUpdate in booksToUpdate)
            {
                switch (bookToUpdate.State)
                {
                    case EntityState.Deleted:
                    {
                        var noSqlBook = _noSqlContext.Find<BookListNoSql>(bookToUpdate.BookId);
                        _noSqlContext.Remove<BookListNoSql>(noSqlBook);
                    }
                        break;
                    case EntityState.Modified:
                    {
                        var noSqlBook = _noSqlContext.Find<BookListNoSql>(bookToUpdate.BookId);
                        noSqlBook = _sqlContext.Books.ProjectBook(bookToUpdate.BookId);
                    }
                        break;
                    case EntityState.Added:
                        var newBook = _sqlContext.Books.ProjectBook(bookToUpdate.BookId);
                        _noSqlContext.Add(newBook);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}