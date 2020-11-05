// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayerEvents.EfClasses;
using GenericEventRunner.DomainParts;

namespace DataLayerEvents.DomainEvents
{
    public class AuthorNameUpdatedEvent : IEntityEvent
    {
        public AuthorNameUpdatedEvent(AuthorWithEvents changedAuthor)
        {
            ChangedAuthor = changedAuthor;
        }

        public AuthorWithEvents ChangedAuthor { get; }
    }
}