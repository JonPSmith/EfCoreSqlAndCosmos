// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataLayerEvents.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericServices;
using Microsoft.EntityFrameworkCore;

namespace DataLayerEvents.EfClasses
{
    public class BookWithEvents : EntityEvents
    {
        public const int PromotionalTextLength = 200;
        private HashSet<BookAuthorWithEvents> _authorsLink;

        //-----------------------------------------------
        //relationships

        //Use uninitialized backing fields - this means we can detect if the collection was loaded
        private HashSet<ReviewWithEvents> _reviews;

        //-----------------------------------------------
        //ctors

        private BookWithEvents() { }

        [Required(AllowEmptyStrings = false)]
        public string Title { get; private set; }

        public string Description { get; private set; }
        public DateTime PublishedOn { get; set; }
        public string Publisher { get; private set; }
        public decimal OrgPrice { get; private set; }
        public decimal ActualPrice { get; private set; }

        [MaxLength(PromotionalTextLength)]
        public string PromotionalText { get; private set; }

        public string ImageUrl { get; private set; }

        public bool SoftDeleted { get; private set; }

        public IEnumerable<ReviewWithEvents> Reviews => _reviews?.ToList();
        public IEnumerable<BookAuthorWithEvents> AuthorsLink => _authorsLink?.ToList();

        public Guid BookId { get; private set; }

        //----------------------------------------------
        //Extra properties filled in by events
        public string AuthorsOrdered { get; set; }

        [ConcurrencyCheck]
        public int ReviewsCount { get; set; }

        [ConcurrencyCheck]
        public double ReviewsAverageVotes { get; set; }

        //This is an action provided in the review add/remove event so that the review handler can update these properties
        private void UpdateReviewCachedValues(int reviewsCount, double reviewsAverageVotes)
        {
            ReviewsCount = reviewsCount;
            ReviewsAverageVotes = reviewsAverageVotes;
        }
        //----------------------------------------------

        public static IStatusGeneric<BookWithEvents> CreateBook(string title, string description, DateTime publishedOn,
            string publisher, decimal price, string imageUrl, ICollection<AuthorWithEvents> authors)
        {
            var status = new StatusGenericHandler<BookWithEvents>();
            if (string.IsNullOrWhiteSpace(title))
                status.AddError("The bookWithEvents title cannot be empty.");

            var book = new BookWithEvents
            {
                BookId = Guid.NewGuid(),
                Title = title,
                Description = description,
                PublishedOn = publishedOn,
                Publisher = publisher,
                ActualPrice = price,
                OrgPrice = price,
                ImageUrl = imageUrl,
                //We need to initialise the AuthorsOrdered string when the entry is created
                AuthorsOrdered = string.Join(", ", authors.Select(x => x.Name)),
                //We don't need to initialise the ReviewsCount and the ReviewsAverageVotes  as they default to zero
                _reviews = new HashSet<ReviewWithEvents>()       //We add an empty list on create. I allows reviews to be added when building test data
            };
            if (authors == null)
                throw new ArgumentNullException(nameof(authors));

            byte order = 0;
            book._authorsLink = new HashSet<BookAuthorWithEvents>(authors.Select(a => new BookAuthorWithEvents(book, a, order++)));
            if (!book._authorsLink.Any())
                status.AddError("You must have at least one Author for a bookWithEvents.");

            return status.SetResult(book);
        }

        public void UpdatePublishedOn(DateTime publishedOn)
        {
            PublishedOn = publishedOn;
        }

        public void AddReview(int numStars, string comment, string voterName, 
            DbContext context = null) 
        {
            if (_reviews != null)    
            {
                _reviews.Add(new ReviewWithEvents(numStars, comment, voterName));   
            }
            else if (context == null)
            {
                throw new ArgumentNullException(nameof(context), 
                    "You must provide a context if the Reviews collection isn't valid.");
            }
            else if (context.Entry(this).IsKeySet)  
            {
                context.Add(new ReviewWithEvents(numStars, comment, voterName, BookId));
            }
            else                                     
            {                                        
                throw new InvalidOperationException("Could not add a new review.");  
            }

            AddEvent(new BookReviewAddedEvent(numStars, this, UpdateReviewCachedValues));
        }

        public void RemoveReview(int reviewId, DbContext context = null)
        {
            ReviewWithEvents review;
            if (_reviews != null)
            {
                //This is there to handle the add/remove of reviews when first created (or someone uses an .Include(p => p.Reviews)
                review = _reviews.SingleOrDefault(x => x.ReviewId == reviewId);
                if (review == null)
                    throw new InvalidOperationException("The review with that key was not found in the book's Reviews.");
                _reviews.Remove(review);
            }
            else if (context == null)
            {
                throw new ArgumentNullException(nameof(context),
                    "You must provide a context if the Reviews collection isn't valid.");
            }
            else
            {
                review = context.Find<ReviewWithEvents>(reviewId);
                if (review == null || review.BookId != BookId)
                {
                    // This ensures that the review is a) linked to the book you defined, and b) the review has a valid primary key
                    throw new InvalidOperationException("The review either wasn't found or was not linked to this Book.");
                }

                context.Remove(review);
            }

            AddEvent(new BookReviewRemovedEvent(review, this, UpdateReviewCachedValues));
        }

        public IStatusGeneric AddPromotion(decimal actualPrice, string promotionalText)                  
        {
            var status = new StatusGenericHandler();
            if (string.IsNullOrWhiteSpace(promotionalText))
            {
                status.AddError("You must provide some text to go with the promotion.", nameof(PromotionalText));
                return status;
            }

            ActualPrice = actualPrice;  
            PromotionalText = promotionalText;

            status.Message = $"The book's new price is ${actualPrice:F}.";

            return status; 
        }

        public void RemovePromotion() 
        {
            ActualPrice = OrgPrice; 
            PromotionalText = null; 
        }
    }

}