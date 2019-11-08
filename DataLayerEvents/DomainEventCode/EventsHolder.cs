// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace DataLayerEvents.DomainEventCode
{
    /// <summary>
    /// This is a class that the EF Core entity classes inherit to add events
    /// </summary>
    public class EventsHolder
    {
        //Events are NOT stored in the database - they are transitory events
        //Events are created within a single DBContext and are cleared every time SaveChanges/SaveChangesAsync is called
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

        public void AddEvent(IDomainEvent dEvent)
        {
            _domainEvents.Add(dEvent);
        }

        public ICollection<IDomainEvent> ReturnEventsAndThenClear()
        {
            var eventCopy = _domainEvents.ToList();
            _domainEvents.Clear();
            return eventCopy;
        }
    }
}