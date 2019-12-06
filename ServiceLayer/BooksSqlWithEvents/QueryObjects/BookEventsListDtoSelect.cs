// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayerEvents.EfClasses;
using ServiceLayer.BooksSql.Dtos;

namespace ServiceLayer.BooksSqlWithEvents.QueryObjects
{
    public static class BookEventsListDtoSelect
    {
        public static IQueryable<BookListDto> MapBookEventsToDto(this IQueryable<BookWithEvents> books)     
        {
            return books.Select(p => new BookListDto
            {
                BookId = p.BookId,                        
                Title = p.Title,                                                  
                PublishedOn = p.PublishedOn, 
                ActualPrice = p.ActualPrice,
                OrgPrice = p.OrgPrice,
                PromotionalText = p.PromotionalText,   
                AuthorsOrdered = p.AuthorsOrdered,
                ReviewsCount = p.ReviewsCount,
                ReviewsAverageVotes = p.ReviewsAverageVotes
            });
        }
    }
}