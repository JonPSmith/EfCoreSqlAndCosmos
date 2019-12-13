// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License file in the project root for license information.

using System;
using System.Linq;
using DataLayerEvents.EfClasses;
using DataLayerEvents.EfCode;
using Microsoft.EntityFrameworkCore;

namespace DataLayerEvents.QueryExtensions
{
    public static class BookQueries
    {
        public static string FormAuthorOrderedString(this DbContext context, Guid bookId)
        {
            return string.Join(", ", context.Set<BookWithEvents>().Where(x => x.BookId == bookId)
                .SelectMany(x => x.AuthorsLink.OrderBy(y => y.Order).Select(y => y.Author.Name)));
        }

        public static string FormAuthorOrderedString(this BookWithEvents book)
        {
            if (book.AuthorsLink == null || book.AuthorsLink.Any(x => x.Author == null))
                throw new InvalidOperationException("The book must have the AuthorLink collection filled, and the AuthorLink Author filled too.");

            return string.Join(", ", book.AuthorsLink.OrderBy(x => x.Order).Select(x => x.Author.Name));
        }

        public static (int ReviewCount, double ReviewsAverageVotes) CalcReviewCacheValuesFromDb(
            this DbContext context, Guid bookId)
        {
            var reviewData = context.Set<ReviewWithEvents>()
                .Where(x => x.BookId == bookId).Select(x => x.NumStars)
                .ToList();

            return (ReviewCount: reviewData.Count, ReviewsAverageVotes: reviewData.Sum() / (double) reviewData.Count);
        }
    }
}