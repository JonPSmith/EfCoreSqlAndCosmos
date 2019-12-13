// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DataLayerEvents.EfClasses;
using DataLayerEvents.EfCode;

namespace Test.Helpers
{
    public static class WithEventsEfTestData
    {
        public const string DummyUserId = "UnitTestUserId";
        public static readonly DateTime DummyBookStartDate = new DateTime(2017, 1, 1);

        public static void SeedDatabaseDummyBooks(this SqlEventsDbContext context, int numBooks = 10, bool stepByYears = false)
        {
            context.Books.AddRange(CreateDummyBooks(numBooks, stepByYears));
            context.SaveChanges();
        }

        public static BookWithEvents CreateDummyBookOneAuthor()
        {
            var book = BookWithEvents.CreateBook
            (
                "Book Title",
                "Book Description",
                DummyBookStartDate,
                "Book Publisher",
                123,
                null,
                new[] { new AuthorWithEvents( "Test Author", null) }
            );

            return book.Result;
        }

        public static BookWithEvents CreateDummyBookTwoAuthorsTwoReviews()
        {
            var book = BookWithEvents.CreateBook
            (
                "Book Title",
                "Book Description",
                DummyBookStartDate,
                "Book Publisher",
                123,
                null,
                new[] { new AuthorWithEvents( "Author1", null), new AuthorWithEvents( "Author2", null) }
            );
            book.Result.AddReview(5, null, "test1");
            book.Result.AddReview(1, null, "test2");

            return book.Result;
        }

        public static List<BookWithEvents> CreateDummyBooks(int numBooks = 10, bool stepByYears = false)
        {
            var result = new List<BookWithEvents>();
            var commonAuthor = new AuthorWithEvents( "CommonAuthor", null);
            for (int i = 0; i < numBooks; i++)
            {
                var book = BookWithEvents.CreateBook
                (
                    $"Book{i:D4} Title",
                    $"Book{i:D4} Description",
                    stepByYears ? DummyBookStartDate.AddYears(i) : DummyBookStartDate.AddDays(i),
                    "Publisher",
                    (short)(i + 1),
                    $"Image{i:D4}",
                    new[] { new AuthorWithEvents( $"Author{i:D4}", null), commonAuthor}
                ).Result;
                for (int j = 0; j < i; j++)
                {
                    book.AddReview((j % 5) + 1, null, j.ToString());
                }

                result.Add(book);
            }

            return result;
        }

        public static List<BookWithEvents> SeedDatabaseFourBooks(this SqlEventsDbContext context)
        {
            var fourBooks = CreateFourBooks();
            context.Books.AddRange(fourBooks);
            context.SaveChanges();
            return fourBooks;
        }

        public static List<BookWithEvents> CreateFourBooks()
        {
            var martinFowler = new AuthorWithEvents( "Martin Fowler", null);

            var books = new List<BookWithEvents>();

            var book1 = BookWithEvents.CreateBook
            (
                "Refactoring",
                "Improving the design of existing code",
                new DateTime(1999, 7, 8),
                null,
                40,
                null,
                new[] { martinFowler }
            ).Result;
            books.Add(book1);

            var book2 = BookWithEvents.CreateBook
            (
                "Patterns of Enterprise Application Architecture",
                "Written in direct response to the stiff challenges",
                new DateTime(2002, 11, 15),
                null,
                53,
                null,
                new []{martinFowler}
            ).Result;
            books.Add(book2);

            var book3 = BookWithEvents.CreateBook
            (
                "Domain-Driven Design",
                 "Linking business needs to software design",
                 new DateTime(2003, 8, 30),
                 null,
                56,
                null,
                new[] { new AuthorWithEvents( "Eric Evans", null) }
            ).Result;
            books.Add(book3);

            var book4 = BookWithEvents.CreateBook
            (
                "Quantum Networking",
                "Entangled quantum networking provides faster-than-light data communications",
                new DateTime(2057, 1, 1),
                "Future Published",
                220,
                null,
                new[] { new AuthorWithEvents( "Future Person", null) }
            ).Result;
            book4.AddReview(5,
                "I look forward to reading this book, if I am still alive!", "Jon P Smith");
            book4.AddReview(5,
                "I write this book if I was still alive!", "Albert Einstein"); book4.AddPromotion(219, "Save $1 if you order 40 years ahead!");
            book4.AddPromotion(219, "Save 1$ by buying 40 years ahead");

            books.Add(book4);

            return books;
        }
    }
}