// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.AppStart
{
    public static class NetCoreDiSetupExtensions
    {
        public static void RegisterInfrastructureDi(this IServiceCollection services)
        {
            services.RegisterGenericEventRunner();


        }
    }
}