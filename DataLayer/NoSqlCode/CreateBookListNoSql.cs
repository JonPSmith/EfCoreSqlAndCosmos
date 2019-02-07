// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.EfClassesNoSql;
using DataLayer.EfClassesSql;

namespace DataLayer.NoSqlCode
{
    public static class CreateBookListNoSql
    {
        private class BookWithParts
        {
            public List<string> Authors { get; set; }
            public BookListNoSql Book { get; set; }

            public void UpdateBooksAuthorsOrdered()
            {
                Book.AuthorsOrdered = string.Join(", ", Authors);
            }
        }

        public static BookListNoSql ProjectBook(this IQueryable<Book> books, Guid bookId)
        {
            var results = books.ProjectBooksNoAuthors();
            var result = results.Single(x => x.Book.BookId == bookId);
            result.UpdateBooksAuthorsOrdered();
            return result.Book;
        }

        public static IList<BookListNoSql> ProjectBooks(this IQueryable<Book> books)
        {
            var results = books.ProjectBooksNoAuthors().ToList();
            results.ForEach(x => x.UpdateBooksAuthorsOrdered());
            return results.Select(x => x.Book).ToList();
        }

        private static IQueryable<BookWithParts> ProjectBooksNoAuthors(this IQueryable<Book> books)
        {
            return books.Select(p => new BookWithParts
            {
                Authors = p.AuthorsLink
                    .OrderBy(q => q.Order)
                    .Select(q => q.Author.Name).ToList(),
                Book = new BookListNoSql
                {
                    BookId = p.BookId,
                    Title = p.Title,
                    OrgPrice = p.OrgPrice,
                    PublishedOn = p.PublishedOn,
                    ActualPrice = p.ActualPrice,
                    PromotionText = p.PromotionalText,
                    ReviewsCount = p.Reviews.Count(),
                    ReviewsAverageVotes = p.Reviews.Select(y => (double?)y.NumStars).Average()
                }
            });
        }

    }
}