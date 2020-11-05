﻿// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayerEvents.EfClasses;
using GenericEventRunner.DomainParts;

namespace DataLayerEvents.DomainEvents
{
    public class BookReviewRemovedEvent : IEntityEvent
    {
        public BookReviewRemovedEvent(ReviewWithEvents reviewRemoved, BookWithEvents book, Action<int, double> updateReviewCachedValues)
        {
            ReviewRemoved = reviewRemoved;
            Book = book;
            UpdateReviewCachedValues = updateReviewCachedValues;
        }

        public ReviewWithEvents ReviewRemoved { get; }

        public BookWithEvents Book { get; }

        public Action<int, double> UpdateReviewCachedValues { get; }
    }
}