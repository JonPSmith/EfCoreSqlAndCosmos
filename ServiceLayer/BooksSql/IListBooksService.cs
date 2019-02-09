// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using ServiceLayer.BooksSql.Dtos;

namespace ServiceLayer.BooksSql
{
    public interface IListBooksService
    {
        IQueryable<BookListDto> SortFilterPage(SortFilterPageOptions options);
    }
}