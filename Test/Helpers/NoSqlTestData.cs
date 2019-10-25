// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DataLayer.EfClassesNoSql;
using DataLayer.EfClassesSql;

namespace Test.Helpers
{
    public static class NoSqlTestData
    {
        public static BookListNoSql CreateDummyNoSqlBook(double votes = 0)
        {

            var book = new BookListNoSql
            {
                BookId = Guid.NewGuid(),
                Title = "Test",
                AuthorsOrdered = "Author1,Author2",
                ActualPrice = 1,
                OrgPrice = 1,
                PublishedOn = new DateTime(2000, 1, 1),
                YearPublished = 2000,
                ReviewsAverageVotes = votes,
                ReviewsCount = 1
            };

            return book;
        }

        public static List<BookListNoSql> CreateDummyBooks(int numBooks = 10, bool stepByYears = false)
        {
            var random = new Random(0);
            var result = new List<BookListNoSql>();
            var commonAuthor = new Author { Name = "CommonAuthor" };
            for (int i = 0; i < numBooks; i++)
            {
                var publishedDate = stepByYears
                    ? DddEfTestData.DummyBookStartDate.AddYears(i)
                    : DddEfTestData.DummyBookStartDate.AddDays(i);
                var book = new BookListNoSql
                {
                    BookId = Guid.NewGuid(),
                    Title = $"Book{i:D4} Title",
                    AuthorsOrdered = $"Author{i:D4}, CommonAuthor",
                    OrgPrice = i + 1,
                    ActualPrice = i + 1,
                    PublishedOn = publishedDate,
                    YearPublished = publishedDate.Year,
                    ReviewsAverageVotes = random.NextDouble() * 5,
                    ReviewsCount = (i % 5) + 1
                };
                result.Add(book);
            }
            return result;
        }
    }
}