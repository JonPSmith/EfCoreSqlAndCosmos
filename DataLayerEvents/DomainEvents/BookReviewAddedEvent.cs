// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayerEvents.EfClasses;
using GenericEventRunner.ForEntities;

namespace DataLayerEvents.DomainEvents
{
    public class BookReviewAddedEvent : IDomainEvent
    { 
        public BookReviewAddedEvent(int numStars, BookWithEvents book, Action<int,double> updateReviewCachedValues)
        {
            NumStars = numStars;
            Book = book;
            UpdateReviewCachedValues = updateReviewCachedValues;
        }

        public int NumStars { get; }

        public BookWithEvents Book { get; }

        public Action<int, double> UpdateReviewCachedValues { get; }
    }
}