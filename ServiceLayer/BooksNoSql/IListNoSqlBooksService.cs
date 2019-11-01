// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfClassesNoSql;
using ServiceLayer.BooksCommon;

namespace ServiceLayer.BooksNoSql
{
    public interface IListNoSqlBooksService
    {
        Task<IList<BookListNoSql>> SortFilterPageAsync(NoSqlSortFilterPageOptions options);
    }
}