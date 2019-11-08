// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataLayerEvents.DomainEventCode;
using DataLayerEvents.DomainEvents;

namespace DataLayerEvents.EfClasses
{
    public class AuthorWithEvents : EventsHolder
    {
        public const int NameLength = 100;
        public const int EmailLength = 100;

        private string _name;

        public AuthorWithEvents() { }

        [Key]
        public Guid AuthorId { get;  set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(NameLength)]
        public string Name
        {
            get => _name;
            set
            {
                if (value != Name)
                    AddEvent(new AuthorNameUpdatedEvent());
                _name = value;
            }
        }

        [MaxLength(EmailLength)]
        public string Email { get; set; }

        //------------------------------
        //Relationships

        public ICollection<BookAuthorWithEvents> BooksLink { get; set; }
    }

}