// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.EfClassesSql
{
    public class Author  : IAuthorId
    {
        public const int NameLength = 100;
        public const int EmailLength = 100;

        public Author() { }

        public Guid AuthorId { get;  set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(NameLength)]
        public string Name { get;  set; }

        [MaxLength(EmailLength)]
        public string Email { get; set; }

        //------------------------------
        //Relationships

        public ICollection<BookAuthor> BooksLink { get; set; }
    }

}