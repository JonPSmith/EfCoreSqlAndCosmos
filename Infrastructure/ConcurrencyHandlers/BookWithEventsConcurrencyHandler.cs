// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License file in the project root for license information.

using System;
using System.Linq;
using DataLayerEvents.EfClasses;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace Infrastructure.ConcurrencyHandlers
{
    public static class BookWithEventsConcurrencyHandler
    {
        public static IStatusGeneric HandleReviewConcurrency(this Exception ex, DbContext context)
        {
            var dbUpdateEx = ex as DbUpdateConcurrencyException;
            if (dbUpdateEx == null || dbUpdateEx.Entries.Count != 1)
                return null; //can't handle this error

            var entry = dbUpdateEx.Entries.Single();
            if (!(entry.Entity is BookWithEvents failedBook))
                return null;

            var status = new StatusGenericHandler();
            //This entity MUST be read as NoTracking otherwise it will interfere with the same entity we are trying to write
            var bookThatOverwrote = context.Set<BookWithEvents>().AsNoTracking().SingleOrDefault(p => p.BookId == failedBook.BookId);
            if (bookThatOverwrote == null)
                //The book was deleted so don't need to do anything else
                return status.AddError("The book you where updating has been deleted by another user.");

            //we recover the change that the failed write was trying to 
            var previousCount = (int)entry.Property(nameof(BookWithEvents.ReviewsCount)).OriginalValue;
            var previousAverageVotes = (double)entry.Property(nameof(BookWithEvents.ReviewsAverageVotes)).OriginalValue;
            var previousTotalStars = Math.Round(previousAverageVotes * previousCount);
            var countChange = failedBook.ReviewsCount - previousCount;
            var starsChange = Math.Round(failedBook.ReviewsAverageVotes * failedBook.ReviewsCount) - previousTotalStars;

            //Now find out what is the new starting point from the write that caused the concurrency exception
            var newCount = bookThatOverwrote.ReviewsCount + countChange;
            var totalStars = Math.Round(bookThatOverwrote.ReviewsAverageVotes * bookThatOverwrote.ReviewsCount) +
                             starsChange;

            entry.Property(nameof(BookWithEvents.ReviewsCount)).CurrentValue = newCount;
            entry.Property(nameof(BookWithEvents.ReviewsAverageVotes)).CurrentValue = totalStars / newCount;

            //Now set the original values to the bookOverwrote 
            entry.Property(nameof(BookWithEvents.ReviewsCount)).OriginalValue = bookThatOverwrote.ReviewsCount;
            entry.Property(nameof(BookWithEvents.ReviewsAverageVotes)).OriginalValue =
                bookThatOverwrote.ReviewsAverageVotes;

            return status; //We return a status with no errors, which tells the caller to retry the SaveChanges
        }
    }
}