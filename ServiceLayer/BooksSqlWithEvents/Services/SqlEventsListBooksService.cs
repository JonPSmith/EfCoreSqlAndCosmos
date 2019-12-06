// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using DataLayer.QueryObjects;
using DataLayerEvents.EfClasses;
using DataLayerEvents.EfCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.BooksSql;
using ServiceLayer.BooksSql.Dtos;
using ServiceLayer.BooksSql.QueryObjects;
using ServiceLayer.BooksSqlWithEvents.QueryObjects;

namespace ServiceLayer.BooksSqlWithEvents.Services
{
    public class SqlEventsListBooksService : ISqlEventsListBooksService
    {
        private readonly SqlEventsDbContext _context;

        public SqlEventsListBooksService(SqlEventsDbContext context)
        {
            _context = context;
        }

        public IQueryable<BookListDto> SortFilterPage(SqlSortFilterPageOptions options)
        {
            var booksQuery = _context.Books            
                .AsNoTracking()
                .MapBookEventsToDto()
                .OrderBooksEventsBy(options.OrderByOptions)  
                .FilterBooksEventsBy(options.FilterBy,       
                               options.FilterValue);   

            options.SetupRestOfDto(booksQuery);        

            return booksQuery.Page(options.PageNum-1,  
                                   options.PageSize);  
        }
    }

}