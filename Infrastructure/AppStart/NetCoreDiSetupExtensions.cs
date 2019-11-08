// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Infrastructure.EventRunnerCode;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.AppStart
{
    public static class NetCoreDiSetupExtensions
    {
        public static void RegisterInfrastructureDi(this IServiceCollection services)
        {

            //Now we register all the Before/After event handlers in this assembly 
            services.RegisterEventHandlers();
            //This registers the code that will create and call the specific handlers
            services.RegisterEventRunner();

        }
    }
}