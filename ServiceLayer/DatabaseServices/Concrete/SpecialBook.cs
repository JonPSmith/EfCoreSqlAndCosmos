// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.EfClassesSql;

namespace ServiceLayer.DatabaseServices.Concrete
{
    public static class SpecialBook
    {
        public static Book CreateSpecialBook()
        {
            var book4 = Book.CreateBook("Quantum Networking",
                "Entangled quantum networking provides faster-than-light data communications",
                new DateTime(2057, 1, 1), "Manning", 220, null,
                new List<Author> {new Author {Name = "Future Person"}}).Result;

            book4.AddReview(5, "I look forward to reading this book, if I am still alive!", "Jon P Smith");
            book4.AddReview(5, "I would write this book if I was still alive!", "Albert Einstein");

            //Fill in cache values
            book4.AuthorsOrdered = "Future Person";
            book4.ReviewsCount = book4.Reviews.Count();
            book4.ReviewsAverageVotes = book4.Reviews.Sum(x => x.NumStars) / (double)book4.ReviewsCount;

            book4.AddPromotion(219, "Save $1 if you order 40 years ahead!");

            return book4;
        }
    }
}