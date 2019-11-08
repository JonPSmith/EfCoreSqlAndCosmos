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
        
        //This holds events that are run before SaveChanges is called
        private readonly List<IDomainEvent> _transactionEvents = new List<IDomainEvent>();

        //This holds events that are run after SaveChanges finishes successfully
        private readonly List<IDomainEvent> _afterSaveChangesEvents = new List<IDomainEvent>();

        public void AddTransactionEvent(IDomainEvent dEvent)
        {
            _transactionEvents.Add(dEvent);
        }

        public void AddAfterSaveChangesEvent(IDomainEvent dEvent)
        {
            _afterSaveChangesEvents.Add(dEvent);
        }

        public ICollection<IDomainEvent> GetTransactionEventsThenClear()
        {
            var eventCopy = _transactionEvents.ToList();
            _transactionEvents.Clear();
            return eventCopy;
        }

        public ICollection<IDomainEvent> GetAfterSaveChangesEventsThenClear()
        {
            var eventCopy = _afterSaveChangesEvents.ToList();
            _afterSaveChangesEvents.Clear();
            return eventCopy;
        }
    }
}