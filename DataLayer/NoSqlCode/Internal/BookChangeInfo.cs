// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using DataLayer.EfClassesSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

[assembly: InternalsVisibleTo("Test")]
namespace DataLayer.NoSqlCode.Internal
{
    internal class BookChangeInfo 
    {
        /// <summary>
        /// This method should be called whenever an entity that has the IBookId interface
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="entity"></param>
        private BookChangeInfo(Guid bookId, EntityEntry entity)
        {
            BookId = bookId;
            if (entity.Entity is Book book) 
            {
                var softDeletedProp = entity.Property(nameof(book.SoftDeleted));         

                if (softDeletedProp.IsModified)
                {                               
                    State = book.SoftDeleted   
                        ? EntityState.Deleted   
                        : EntityState.Added;    
                }
                else if (entity.State == EntityState.Deleted)        
                {                               
                    State = book.SoftDeleted   
                        ? EntityState.Unchanged 
                        : EntityState.Deleted;  
                }
                else
                {
                    State = book.SoftDeleted 
                        ? EntityState.Unchanged 
                        : entity.State;
                }
            }
            else
            {
                //The entity wasn't a book, but is related to a book so we mark the book as updated
                State = EntityState.Modified; 
            }
        }

        public EntityState State { get; }
        public Guid BookId { get; }

        /// <summary>
        /// This returns the list of books that have changes, and how they have changed
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        public static IImmutableList<BookChangeInfo> FindBookChanges(IEnumerable<EntityEntry> changes)
        {
            var bookChanges = changes
                .Select(x => new {entity = x, bookRef = x.Entity as IBookId})
                .Where(x => x.entity.State != EntityState.Unchanged && x.bookRef != null)
                .Select(x => new BookChangeInfo(x.bookRef.BookId, x.entity));

            //This dedups the book changes, with the Book entity State taking preference
            var booksDict = new Dictionary<Guid, BookChangeInfo>();
            foreach (var bookChange in bookChanges)
            {
                if (booksDict.ContainsKey(bookChange.BookId) && booksDict[bookChange.BookId].State != EntityState.Modified)
                    continue;

                booksDict[bookChange.BookId] = bookChange;
            }

            return booksDict.Values.ToImmutableList();
        }
    }
}