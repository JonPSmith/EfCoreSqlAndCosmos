// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using DataLayer.EfClassesNoSql;
using ServiceLayer.BooksCommon;

namespace ServiceLayer.BooksNoSql
{
    public class NoSqlBookListCombinedDto
    {
        public NoSqlBookListCombinedDto(NoSqlSortFilterPageOptions sortFilterPageData, IList<BookListNoSql> booksList)
        {
            SortFilterPageData = sortFilterPageData;
            BooksList = booksList;
        }

        public NoSqlSortFilterPageOptions SortFilterPageData { get; private set; }

        public IList<BookListNoSql> BooksList { get; private set; }
    }
}