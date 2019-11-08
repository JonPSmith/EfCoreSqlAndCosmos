// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using NetCore.AutoRegisterDi;

namespace Infrastructure.AppStart
{
    public static class NetCoreDiSetupExtensions
    {
        public static void RegisterInfrastructureDi(this IServiceCollection services)
        {

            services.RegisterAssemblyPublicNonGenericClasses()
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces();

            //Hand register classes that don't end in Service
        }
    }
}