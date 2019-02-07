// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.EfClassesNoSql;

namespace Test.Helpers
{
    public static class NoSqlTestData
    {

        public static BookListNoSql CreateDummyNoSqlBook()
        {

            var book = new BookListNoSql
            {
                BookId = Guid.NewGuid(),
                Title = "Test",
                AuthorsOrdered = "Author1,Author2",
                ActualPrice = 1,
                OrgPrice = 1,
                PublishedOn = new DateTime(2000, 1, 1),
                ReviewsAverageVotes = 5,
                ReviewsCount = 1
            };

            return book;
        }
    }
}