// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using ServiceLayer.Books.Dtos;

namespace ServiceLayer.Books
{
    public interface IListBooksService
    {
        Task<IQueryable<BookListDto>> SortFilterPage
            (SortFilterPageOptions options);
    }
}