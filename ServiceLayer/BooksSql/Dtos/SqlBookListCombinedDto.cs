// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using ServiceLayer.BooksCommon;

namespace ServiceLayer.BooksSql.Dtos
{
    public class SqlBookListCombinedDto
    {
        public SqlBookListCombinedDto(SqlSortFilterPageOptions sortFilterPageData, IEnumerable<BookListDto> booksList)
        {
            SortFilterPageData = sortFilterPageData;
            BooksList = booksList;
        }

        public SqlSortFilterPageOptions SortFilterPageData { get; private set; }

        public IEnumerable<BookListDto> BooksList { get; private set; }
    }
}