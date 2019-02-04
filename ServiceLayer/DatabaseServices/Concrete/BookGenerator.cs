// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DataLayer.EfClasses;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ServiceLayer.DatabaseServices.Concrete
{
    public class BookGenerator
    {
        private readonly Dictionary<string, Author> _authorDict = new Dictionary<string, Author>();

        private readonly ImmutableList<BookData> _loadedBookData;
        private readonly bool _makeBookTitlesDistinct;

        public BookGenerator(string filePath, bool makeBookTitlesDistinct)
        {
            _makeBookTitlesDistinct = makeBookTitlesDistinct;
            _loadedBookData = JsonConvert.DeserializeObject<List<BookData>>(File.ReadAllText(filePath))
                .ToImmutableList();
        }

        private int NumBooksInSet => _loadedBookData.Count;

        public ImmutableDictionary<string, Author> AuthorDict => _authorDict.ToImmutableDictionary();

        public void WriteBooks(int numBooks, DbContextOptions<SqlDbContext> options, Func<int, bool> progessCancel)
        {
            //Find out how many in db so we can pick up where we left off
            int numBooksInDb;
            using (var context = new SqlDbContext(options))
            {
                numBooksInDb = context.Books.IgnoreQueryFilters().Count();
            }

            var numWritten = 0;
            var batch = new List<Book>();
            foreach (var book in GenerateBooks(numBooks, numBooksInDb))
            {
                batch.Add(book);
                if (batch.Count < NumBooksInSet) continue;

                //have a batch to write out
                if (progessCancel(numWritten))
                {
                    return;
                }

                CreateContextAndWriteBatch(options, batch);
                numWritten += batch.Count;
                batch.Clear();
            }

            //write any final batch out
            if (batch.Count > 0)
            {
                CreateContextAndWriteBatch(options, batch);
                numWritten += batch.Count;
            }
            progessCancel(numWritten);
        }

        public IEnumerable<Book> GenerateBooks(int numBooks, int numBooksInDb)
        {
            for (int i = numBooksInDb; i < numBooksInDb + numBooks; i++)
            {
                var sectionNum = Math.Truncate(i * 1.0 / NumBooksInSet);
                var reviews = new List<Review>();


                var authors = GetAuthorsOfThisBook(_loadedBookData[i % _loadedBookData.Count].Authors).ToList();
                var title = _loadedBookData[i % _loadedBookData.Count].Title;
                if (i >= NumBooksInSet && _makeBookTitlesDistinct)
                    title += $" (copy {sectionNum})";
                var book = new Book(title,
                    $"Book{i:D4} Description",
                    _loadedBookData[i % _loadedBookData.Count].PublishDate.AddDays(sectionNum),
                    "Manning",
                    (i + 1),
                    null,
                    authors);

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

        private void CreateContextAndWriteBatch(DbContextOptions<SqlDbContext> options, List<Book> batch)
        {
            //if (_updater == null)
            //    throw new InvalidOperationException("The NoSql updater is null. This can be caused by the NoSql connection string being null or empty.");
            using (var context = new SqlDbContext(options))
            {
                //need to set the key of the authors entities. They aren't tarcked but the add will sort out whether to add/Unchanged based on primary key
                foreach (var dbAuthor in context.Authors.ToList())
                {
                    if (_authorDict.ContainsKey(dbAuthor.Name))
                    {
                        _authorDict[dbAuthor.Name].AuthorId = dbAuthor.AuthorId;
                    }
                }            
                context.AddRange(batch);
                context.SaveChanges();
                //Now we update the NoSql database
                //SendBatchToNoSql(batch);
            }
        }

        //private void SendBatchToNoSql(List<Book> batch)
        //{
        //    _updater.BulkLoad(batch.AsQueryable().ProjectBooks());
        //}

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