// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayerEvents.DomainEvents;
using DataLayerEvents.EfClasses;
using DataLayerEvents.EfCode;
using Infrastructure.EventRunnerCode;

namespace Infrastructure.EventHandlers
{
    public class ReviewRemovedHandler : IBeforeSaveEventHandler<BookReviewRemovedEvent>
    {
        private readonly SqlEventsDbContext _context;

        public ReviewRemovedHandler(SqlEventsDbContext context)
        {
            _context = context;
        }

        public void Handle(BookReviewRemovedEvent domainEvent)
        {
            if (domainEvent.Book.Reviews != null)
            {
                //The reviews collection is filled, either because the book was created, or it was loaded with .Include(x => x.Reviews)
                var numReviews = domainEvent.Book.Reviews.Count();
                var reviewsAverageVotes = domainEvent.Book.Reviews.ToList().Average(x => (double)x.NumStars);
                domainEvent.UpdateReviewCachedValues(numReviews, reviewsAverageVotes);

                return;
            }

            if (true)
            {
                //This is the slow but sure way
                var reviewsNumStarsWithoutRemoved = _context.Set<ReviewWithEvents>()
                    .Where(x => x.BookId == domainEvent.Book.BookId && x.ReviewId != domainEvent.ReviewRemoved.ReviewId)
                    .Select(x => x.NumStars).ToList();

                var numReviews = reviewsNumStarsWithoutRemoved.Count;
                var reviewsAverageVotes = reviewsNumStarsWithoutRemoved.Average();

                domainEvent.UpdateReviewCachedValues(numReviews, reviewsAverageVotes);
            }
            else
            {
                //Here is the fast (delta) version of the update. Doesn't need access to the database
                var numReviews = domainEvent.Book.ReviewsCount - 1;
                var totalStars = Math.Round(domainEvent.Book.ReviewsAverageVotes * domainEvent.Book.ReviewsCount)
                                 - domainEvent.ReviewRemoved.NumStars;
                domainEvent.UpdateReviewCachedValues(numReviews, totalStars/numReviews);
            }
        }
    }
}