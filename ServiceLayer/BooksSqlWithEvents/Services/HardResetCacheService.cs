// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License file in the project root for license information.

using System;
using System.Linq;
using System.Text;
using DataLayerEvents.EfCode;
using DataLayerEvents.QueryExtensions;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace ServiceLayer.BooksSqlWithEvents.Services
{
    public class HardResetCacheService : IHardResetCacheService
    {
        [Flags]
        private enum Errors {None, AuthorsWrong = 1, ReviewWrong = 2}
        private readonly SqlEventsDbContext _context;

        public HardResetCacheService(SqlEventsDbContext context)
        {
            _context = context;
        }

        public IStatusGeneric<string> CheckUpdateBookCacheProperties()
        {
            var status = new StatusGenericHandler<string>();
            var errorStrings = new StringBuilder();
            var numBooksChecked = 0;
            var numErrors = 0;
            foreach (var book in _context.Books
                .Include(x => x.Reviews)
                .Include(x => x.AuthorsLink).ThenInclude(x => x.Author))
            {
                var error = Errors.None;
                numBooksChecked++;

                var authorsOrdered = book.FormAuthorOrderedString();
                var reviewsCount = book.Reviews.Count();
                var reviewsAverageVotes = reviewsCount == 0 ? 0 : book.Reviews.Sum(x => x.NumStars) / (double)book.ReviewsCount;

                if (authorsOrdered != book.AuthorsOrdered)
                {
                    book.AuthorsOrdered = authorsOrdered;
                    error = Errors.AuthorsWrong;
                }
                if (reviewsCount != book.ReviewsCount || reviewsAverageVotes != book.ReviewsAverageVotes)
                {
                    book.ReviewsCount = reviewsCount;
                    book.ReviewsAverageVotes = reviewsAverageVotes;
                    error |= Errors.ReviewWrong;
                }

                if (error != Errors.None)
                {
                    errorStrings.AppendLine($"Book: {book.Title} had the following errors: {error.ToString()}");
                    numErrors++;
                }
            }

            if (numErrors > 0)
                _context.SaveChanges();

            status.SetResult(errorStrings.ToString());
            status.Message = numErrors == 0
                ? $"Processed {numBooksChecked} and no cache errors found"
                : $"Processed {numBooksChecked} books and {numErrors} errors found. See returned string for details";

            return status;
        }

    }
}