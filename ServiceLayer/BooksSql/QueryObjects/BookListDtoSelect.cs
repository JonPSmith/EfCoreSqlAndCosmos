// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfClassesSql;
using DataLayer.SqlCode;
using ServiceLayer.BooksSql.Dtos;

namespace ServiceLayer.BooksSql.QueryObjects
{
    public static class BookListDtoSelect
    {
        public static IQueryable<BookListDto> MapBookToDto(this IQueryable<Book> books)     
        {
            return books.Select(p => new BookListDto
            {
                BookId = p.BookId,                        
                Title = p.Title,                                                  
                PublishedOn = p.PublishedOn, 
                ActualPrice = p.ActualPrice,
                OrgPrice = p.OrgPrice,
                PromotionalText = p.PromotionalText,   
                AuthorsOrdered = UdfDefinitions.AuthorsStringUdf(p.BookId),
                ReviewsCount = p.Reviews.Count(),
                ReviewsAverageVotes = UdfDefinitions.AverageVotesUdf(p.BookId)
            });
        }
    }
}