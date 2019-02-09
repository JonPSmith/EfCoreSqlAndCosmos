// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataLayer.EfClassesNoSql;
using ServiceLayer.BooksCommon;

namespace ServiceLayer.BooksNoSql.QueryObjects
{

    public static class BookListDtoSort
    {
        public static IQueryable<BookListNoSql> OrderBooksBy (this IQueryable<BookListNoSql> books, 
             OrderByOptions orderByOptions)
        {
            switch (orderByOptions)
            {              
                case OrderByOptions.ByVotes:              
                    return books.OrderByDescending(x => x.ReviewsAverageVotes);           
                case OrderByOptions.ByPublicationDate:    
                    return books.OrderByDescending(x => x.PublishedOn);              
                case OrderByOptions.ByPriceLowestFirst:   
                    return books.OrderBy(x => x.ActualPrice);
                case OrderByOptions.ByPriceHigestFirst:   
                    return books.OrderByDescending(x => x.ActualPrice);              
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(orderByOptions), orderByOptions, null);
            }
        }
    }

}