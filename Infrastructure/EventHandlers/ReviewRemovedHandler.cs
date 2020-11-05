// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayerEvents.DomainEvents;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Infrastructure.EventHandlers
{
    public class ReviewRemovedHandler : IBeforeSaveEventHandler<BookReviewRemovedEvent>
    {
        public IStatusGeneric Handle(object callingEntity, BookReviewRemovedEvent domainEvent)
        {
            //Here is the fast (delta) version of the update. Doesn't need access to the database
            var numReviews = domainEvent.Book.ReviewsCount - 1;
            var totalStars = Math.Round(domainEvent.Book.ReviewsAverageVotes * domainEvent.Book.ReviewsCount)
                             - domainEvent.ReviewRemoved.NumStars;
            domainEvent.UpdateReviewCachedValues(numReviews, totalStars / numReviews);

            return null;
        }

    }
}