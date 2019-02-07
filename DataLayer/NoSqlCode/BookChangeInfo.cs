// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
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
namespace DataLayer.NoSqlCode
{
    internal class BookChangeInfo 
    {
        private readonly Book _book; 
        public EntityState State { get; }
        public Guid BookId => _book.BookId;

        private BookChangeInfo(EntityEntry entity) 
        {
            _book = entity.Entity as Book;  

            if (_book != null) 
            {
                var softDeletedProp = entity.Property(nameof(_book.SoftDeleted));         

                if (softDeletedProp.IsModified) 
                {                               
                    State = _book.SoftDeleted   
                        ? EntityState.Deleted   
                        : EntityState.Added;    
                }
                else if (entity.State == EntityState.Deleted)        
                {                               
                    State = _book.SoftDeleted   
                        ? EntityState.Unchanged 
                        : EntityState.Deleted;  
                }
                else
                {
                    State = _book.SoftDeleted 
                        ? EntityState.Unchanged 
                        : entity.State;         
                }
            }
            else
            {
                State = EntityState.Modified; 
            }
        }
    }
}