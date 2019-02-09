// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfClassesNoSql;
using DataLayer.EfCode;
using DataLayer.QueryObjects;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.BooksCommon;
using ServiceLayer.BooksNoSql.QueryObjects;

namespace ServiceLayer.BooksNoSql.Services
{
    public class ListNoSqlBooksService : IListNoSqlBooksService
    {
        private readonly NoSqlDbContext _context;

        public ListNoSqlBooksService(NoSqlDbContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<BookListNoSql>> SortFilterPageAsync(SortFilterPageOptions options)
        {
            var booksQuery = _context.Books
                .AsNoTracking()                                             
                .OrderBooksBy(options.OrderByOptions)  
                .FilterBooksBy(options.FilterBy,       
                               options.FilterValue);   

            await options.SetupRestOfDtoAsync(booksQuery);        

            return booksQuery.Page(options.PageNum-1,  
                                   options.PageSize);  
        }
    }

}