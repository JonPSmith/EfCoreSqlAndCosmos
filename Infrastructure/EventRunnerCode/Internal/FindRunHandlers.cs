// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayerEvents.DomainEventCode;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.EventRunnerCode.Internal
{
    internal class FindRunHandlers
    {
        private readonly IServiceProvider _serviceProvider;

        public FindRunHandlers(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// This finds and runs all the BeforeSave handlers built to take this domain event 
        /// </summary>
        /// <param name="domainEvent"></param>
        public void DispatchBeforeSave(IDomainEvent domainEvent)
        {
            var wrapperType = GetMatchingEventType(domainEvent, true);
            var wrappedHandlers = _serviceProvider.GetServices(wrapperType)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(domainEvent);
            }
        }

        /// <summary>
        /// This finds and runs all the AfterSave handlers built to take this domain event 
        /// </summary>
        /// <param name="domainEvent"></param>
        public void DispatchAfterSave(IDomainEvent domainEvent)
        {
            var wrapperType = GetMatchingEventType(domainEvent, false);
            var wrappedHandlers = _serviceProvider.GetServices(wrapperType)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(domainEvent);
            }
        }

        private Type GetMatchingEventType(IDomainEvent domainEvent, bool beforeSave)
        {
            var handlerTypeToBuildFor =
                beforeSave ? typeof(IBeforeSaveEventHandler<>) : typeof(IAfterSaveEventHandler<>);
            var handlerInterface = handlerTypeToBuildFor.MakeGenericType(domainEvent.GetType());
            var wrapperType = handlerTypeToBuildFor.MakeGenericType(domainEvent.GetType());
            return wrapperType;
        }

        private abstract class BeforeSaveEventHandler
        {
            public abstract void Handle(IDomainEvent domainEvent);
        }

        private class BeforeSaveHandler<T> : BeforeSaveEventHandler
            where T : IDomainEvent
        {
            private readonly IBeforeSaveEventHandler<T> _handler;

            public BeforeSaveHandler(IBeforeSaveEventHandler<T> handler)
            {
                _handler = handler;
            }

            public override void Handle(IDomainEvent domainEvent)
            {
                _handler.Handle((T)domainEvent);
            }
        }

        private abstract class AfterSaveEventHandler
        {
            public abstract void Handle(IDomainEvent domainEvent);
        }

        private class AfterSaveHandler<T> : AfterSaveEventHandler
            where T : IDomainEvent
        {
            private readonly IAfterSaveEventHandler<T> _handler;

            public AfterSaveHandler(IAfterSaveEventHandler<T> handler)
            {
                _handler = handler;
            }

            public override void Handle(IDomainEvent domainEvent)
            {
                _handler.Handle((T)domainEvent);
            }
        }
    }
}