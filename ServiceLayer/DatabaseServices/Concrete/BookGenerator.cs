// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using DataLayer.NoSqlCode;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceLayer.DatabaseServices.Concrete.Internal;

namespace ServiceLayer.DatabaseServices.Concrete
{
    public class BookGenerator
    {
        private readonly DbContextOptions<SqlDbContext> _options;
        private readonly IBookUpdater _bookUpdater;
        private ImmutableList<BookData> _loadedBookData;
        private int NumBooksInSet => _loadedBookData.Count;

        public BookGenerator(DbContextOptions<SqlDbContext> options, IBookUpdater bookUpdater)
        {
            _options = options;
            _bookUpdater = bookUpdater;
        }

        public async Task WriteBooksAsync(string filePath, int totalBooksNeeded, bool makeBookTitlesDistinct, Func<int, bool> progressCancel)
        {
            _loadedBookData = JsonConvert.DeserializeObject<List<BookData>>(File.ReadAllText(filePath))
                .ToImmutableList();

            
            //Find out how many in db so we can pick up where we left off
            int numBooksInDb;
            using (var context = new SqlDbContext(_options,_bookUpdater))
            {
                numBooksInDb = await context.Books.IgnoreQueryFilters().CountAsync();
            }

            var numWritten = 0;
            var numToWrite = totalBooksNeeded - numBooksInDb;
            while (numWritten < numToWrite)
            {
                if (progressCancel(numWritten))
                {
                    return;
                }

                using (var context = new SqlDbContext(_options, _bookUpdater))
                {
                    var authorsFinder = new AuthorFinder(context);
                    var batch = GenerateBooks(totalBooksNeeded, numBooksInDb, makeBookTitlesDistinct, authorsFinder).ToList();
                    context.AddRange(batch);
                    await context.SaveChangesAsync();
                    numWritten += batch.Count;

                    if (progressCancel(numWritten))
                    {
                        return;
                    }
                }
            }

            progressCancel(numWritten);
        }

        private IEnumerable<Book> GenerateBooks(int totalBooksNeeded, int numBooksInDb, bool makeBookTitlesDistinct, AuthorFinder authorsFinder)
        {
            for (int i = numBooksInDb; i < totalBooksNeeded; i++)
            {
                var sectionNum = Math.Truncate(i * 1.0 / NumBooksInSet);
                var reviews = new List<Review>();

                var authors = authorsFinder.GetAuthorsOfThisBook(_loadedBookData[i % _loadedBookData.Count].Authors).ToList();
                var title = _loadedBookData[i % _loadedBookData.Count].Title;
                if (i >= NumBooksInSet && makeBookTitlesDistinct)
                    title += $" (copy {sectionNum})";
                var book = Book.CreateBook(title,
                    $"Book{i:D4} Description",
                    _loadedBookData[i % _loadedBookData.Count].PublishDate.AddDays(sectionNum),
                    "Manning",
                    (i + 1),
                    null,
                    authors).Result;

                for (int j = 0; j < i % 12; j++)
                {
                    book.AddReview((Math.Abs(3 - j) % 4) + 2, null, j.ToString());
                }
                if (i % 7 == 0)
                {
                    book.AddPromotion(book.ActualPrice * 0.5m, "today only - 50% off! ");
                }

                yield return book;
            }
        }

        public class BookData
        {
            public DateTime PublishDate { get; set; }
            public string Title { get; set; }
            public string Authors { get; set; }
        }
    }
}