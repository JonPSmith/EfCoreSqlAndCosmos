// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayerEvents.EfClasses;
using ServiceLayer.BooksCommon;
using ServiceLayer.BooksSql.Dtos;

namespace ServiceLayer.BooksSqlWithEvents.QueryObjects
{


    public static class BookEventsListDtoSort
    {
        public static IQueryable<BookListDto> OrderBooksEventsBy
            (this IQueryable<BookListDto> books, OrderByOptions orderByOptions)
        {
            switch (orderByOptions)
            {              
                case OrderByOptions.ByVotes:              
                    return books.OrderByDescending(x =>   
                        x.ReviewsAverageVotes);           
                case OrderByOptions.ByPublicationDate:    
                    return books.OrderByDescending(       
                        x => x.PublishedOn);              
                case OrderByOptions.ByPriceLowestFirst:   
                    return books.OrderBy(x => x.ActualPrice);
                case OrderByOptions.ByPriceHigestFirst:   
                    return books.OrderByDescending(       
                        x => x.ActualPrice);              
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(orderByOptions), orderByOptions, null);
            }
        }
    }

}