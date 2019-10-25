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

namespace ServiceLayer.DatabaseServices.Concrete
{
    public class BookGenerator
    {
        private readonly Dictionary<string, Author> _authorDict = new Dictionary<string, Author>();

        private readonly DbContextOptions<SqlDbContext> _options;
        private readonly IBookUpdater _bookUpdater;
        private ImmutableList<BookData> _loadedBookData;
        private int NumBooksInSet => _loadedBookData.Count;

        public ImmutableDictionary<string, Author> AuthorDict => _authorDict.ToImmutableDictionary();

        public BookGenerator(DbContextOptions<SqlDbContext> options, IBookUpdater bookUpdater)
        {
            _options = options;
            _bookUpdater = bookUpdater;
        }


        public async Task WriteBooksAsync(string filePath, int numBooks, bool makeBookTitlesDistinct, Func<int, bool> progressCancel)
        {
            _loadedBookData = JsonConvert.DeserializeObject<List<BookData>>(File.ReadAllText(filePath))
                .ToImmutableList();

            //Find out how many in db so we can pick up where we left off
            int numBooksInDb;
            using (var context = new SqlDbContext(_options,_bookUpdater))
            {
                numBooksInDb = context.Books.IgnoreQueryFilters().Count();
            }

            var numWritten = 0;
            var batch = new List<Book>();
            foreach (var book in GenerateBooks(numBooks, numBooksInDb, makeBookTitlesDistinct))
            {
                batch.Add(book);
                if (batch.Count < NumBooksInSet) continue;

                //have a batch to write out
                if (progressCancel(numWritten))
                {
                    return;
                }

                await CreateContextAndWriteBatchAsync(batch);
                numWritten += batch.Count;
                batch.Clear();
            }

            //write any final batch out
            if (batch.Count > 0)
            {
                await CreateContextAndWriteBatchAsync(batch);
                numWritten += batch.Count;
            }
            progressCancel(numWritten);
        }

        private IEnumerable<Book> GenerateBooks(int numBooks, int numBooksInDb, bool makeBookTitlesDistinct)
        {
            for (int i = numBooksInDb; i < numBooksInDb + numBooks; i++)
            {
                var sectionNum = Math.Truncate(i * 1.0 / NumBooksInSet);
                var reviews = new List<Review>();


                var authors = GetAuthorsOfThisBook(_loadedBookData[i % _loadedBookData.Count].Authors).ToList();
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

        //------------------------------------------------------------------
        //private methods

        private async Task CreateContextAndWriteBatchAsync(List<Book> batch)
        {
            if (_bookUpdater == null)
                throw new InvalidOperationException("The NoSql updater is null. This can be caused by the NoSql connection string being null or empty.");
            using (var context = new SqlDbContext(_options, _bookUpdater))
            {
                //need to set the key of the authors entities. They aren't tracked but the add will sort out whether to add/Unchanged based on primary key
                foreach (var dbAuthor in context.Authors.ToList())
                {
                    if (_authorDict.ContainsKey(dbAuthor.Name))
                    {
                        _authorDict[dbAuthor.Name].AuthorId = dbAuthor.AuthorId;
                    }
                }            
                context.AddRange(batch);
                await context.SaveChangesAsync();
            }
        }

        private IEnumerable<Author> GetAuthorsOfThisBook(string authors)
        {
            foreach(var authorName in ExtractAuthorsFromBookData(authors))
            {
                if (!_authorDict.ContainsKey(authorName))
                {
                    _authorDict[authorName] = new Author {Name = authorName};
                }

                yield return _authorDict[authorName];
            }
        }

        private static IEnumerable<string> ExtractAuthorsFromBookData(string authors)
        {
            return authors.Replace(" and ", ",").Replace(" with ", ",")
                .Split(',').Select(x => x.Trim()).Where(x => x.Length > 1);
        }

        public class BookData
        {
            public DateTime PublishDate { get; set; }
            public string Title { get; set; }
            public string Authors { get; set; }
        }
    }
}