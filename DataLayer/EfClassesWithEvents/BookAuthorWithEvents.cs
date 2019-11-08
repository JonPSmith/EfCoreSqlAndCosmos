// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace DataLayer.EfClassesWithEvents
{
    public class BookAuthorWithEvents
    {
        private BookAuthorWithEvents() { }

        internal BookAuthorWithEvents(BookWithEvents bookWithEvents, AuthorWithEvents authorWithEvents, byte order)
        {
            Book = bookWithEvents;
            Author = authorWithEvents;
            Order = order;
        }

        public Guid AuthorId { get; private set; }
        public byte Order { get; private set; }

        //-----------------------------
        //Relationships

        public BookWithEvents Book { get; private set; }
        public AuthorWithEvents Author { get; private set; }

        public Guid BookId { get; private set; }
    }
}