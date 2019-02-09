// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using ServiceLayer.BooksCommon;
using ServiceLayer.BooksSql.QueryObjects;

namespace ServiceLayer.BooksNoSql
{
    public interface IBookNoSqlFilterDropdownService
    {
        IEnumerable<DropdownTuple> GetFilterDropDownValues(BooksFilterBy filterBy);
    }
}