// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly DbContextOptions<SqlDbContext> _sqlOptions;
        private readonly DbContextOptions<NoSqlDbContext> _noSqlOptions;
        private ImmutableList<BookData> _loadedBookData;
        private int NumBooksInSet => _loadedBookData.Count;

        public BookGenerator(DbContextOptions<SqlDbContext> sqlOptions, DbContextOptions<NoSqlDbContext> noSqlOptions)
        {
            _sqlOptions = sqlOptions;
            _noSqlOptions = noSqlOptions;
        }

        public async Task WriteBooksAsync(string filePath, int totalBooksNeeded, bool makeBookTitlesDistinct, CancellationToken cancellationToken)
        {
            _loadedBookData = JsonConvert.DeserializeObject<List<BookData>>(File.ReadAllText(filePath))
                .ToImmutableList();

            
            //Find out how many in db so we can pick up where we left off
            int numBooksInDb;
            using (var context = new SqlDbContext(_sqlOptions,null))
            {
                numBooksInDb = await context.Books.IgnoreQueryFilters().CountAsync();
            }

            var numWritten = 0;
            var numToWrite = totalBooksNeeded - numBooksInDb;
            while (numWritten < numToWrite)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                //noSql can be null. If so then it doesn't write to CosmosDb
                var noSqlBookUpdater = _noSqlOptions != null
                    ? new NoSqlBookUpdater(new NoSqlDbContext(_noSqlOptions))
                    : null;
                using (var sqlDbContext = new SqlDbContext(_sqlOptions, noSqlBookUpdater))
                {
                    var authorsFinder = new AuthorFinder(sqlDbContext);
                    var batchToAdd = Math.Min(_loadedBookData.Count, numToWrite - numWritten);
                    var batch = GenerateBooks(batchToAdd, numBooksInDb, makeBookTitlesDistinct, authorsFinder).ToList();
                    sqlDbContext.AddRange(batch);
                    await sqlDbContext.SaveChangesAsync();
                    numWritten += batch.Count;
                }
            }
        }

        private IEnumerable<Book> GenerateBooks(int batchToAdd, int numBooksInDb, bool makeBookTitlesDistinct, AuthorFinder authorsFinder)
        {
            for (int i = numBooksInDb; i < numBooksInDb + batchToAdd; i++)
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

                //setup the author cache value for the SQL Events version
                book.AuthorsOrdered = string.Join(", ", authors.Select(x => x.Name));

                for (int j = 0; j < i % 12; j++)
                {
                    book.AddReview((Math.Abs(3 - j) % 4) + 2, null, j.ToString());
                }
                if (i % 7 == 0)
                {
                    book.AddPromotion(book.ActualPrice * 0.5m, "today only - 50% off! ");
                }

                if (book.Reviews.Any())
                {
                    //setup the reviews cache values for the SQL Events version
                    book.ReviewsCount = book.Reviews.Count();
                    book.ReviewsAverageVotes = book.Reviews.Sum(x => x.NumStars) / (double)book.ReviewsCount;
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