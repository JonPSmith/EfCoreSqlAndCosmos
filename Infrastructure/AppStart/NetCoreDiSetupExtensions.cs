// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForSetup;
using Infrastructure.ConcurrencyHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.AppStart
{
    public static class NetCoreDiSetupExtensions
    {
        public static void RegisterInfrastructureDi(this IServiceCollection services)
        {

            //This provides a SaveChangesExceptionHandler which handles concurrency issues around ReviewsCount and ReviewsAverageVotes
            var config = new GenericEventRunnerConfig
            {
                SaveChangesExceptionHandler = BookWithEventsConcurrencyHandler.HandleCacheValuesConcurrency
            };
            //Because I haven't provided any assemblies this will scan this assembly for event handlers
            services.RegisterGenericEventRunner(config);
        }
    }
}