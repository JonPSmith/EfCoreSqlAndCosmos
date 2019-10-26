// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;

namespace ServiceLayer.DatabaseServices.Concrete.Internal
{
    internal class AuthorFinder
    {
        private readonly Dictionary<string, Author> _authorDict;
        private SqlDbContext _context;

        public AuthorFinder(SqlDbContext context)
        {
            _context = context;
            _authorDict = context.Authors.ToDictionary(x => x.Name);
        }

        public IEnumerable<Author> GetAuthorsOfThisBook(string authors)
        {
            foreach (var authorName in ExtractAuthorsFromBookData(authors))
            {
                if (!_authorDict.ContainsKey(authorName))
                {
                    _authorDict[authorName] = new Author { Name = authorName };
                }

                yield return _authorDict[authorName];
            }
        }

        private static IEnumerable<string> ExtractAuthorsFromBookData(string authors)
        {
            return authors.Replace(" and ", ",").Replace(" with ", ",")
                .Split(',').Select(x => x.Trim()).Where(x => x.Length > 1);
        }
    }
}