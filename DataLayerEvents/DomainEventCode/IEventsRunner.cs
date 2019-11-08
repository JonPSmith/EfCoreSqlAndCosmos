// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataLayerEvents.DomainEventCode
{
    public interface IEventsRunner
    {
        int RunEventsBeforeAfterSaveChanges(Func<IEnumerable<EntityEntry<EventsHolder>>> getTrackedEntities,  
            Func<int> callBaseSaveChanges);

        Task<int> RunEventsBeforeAfterSaveChangesAsync(Func<IEnumerable<EntityEntry<EventsHolder>>> getTrackedEntities, 
            Func<Task<int>> callBaseSaveChangesAsync);
    }
}