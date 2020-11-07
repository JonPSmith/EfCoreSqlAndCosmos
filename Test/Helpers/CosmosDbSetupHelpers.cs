// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using EfCoreSqlAndCosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestSupport.Helpers;

namespace Test.Helpers
{
    public static class CosmosDbSetupHelpers
    {
        public static async Task<bool> CheckCosmosDbContainerExistsAsync(this CosmosDbSettings settings, string databaseName = null,
            string containerName = null)
        {
            return (await settings.LinkToExistingContainerAsync(databaseName, containerName)) != null;
        }

        public static async Task<bool> CheckCosmosDbContainerExistsAsync(this string configGroupName, string databaseName = null,
            string containerName = null)
        {
            return (await configGroupName.LinkToExistingContainerAsync(databaseName, containerName)) != null;
        }

        public static Task<Container> LinkToExistingContainerAsync(this string configGroupName, string databaseName = null, string containerName = null)
        {
            var settings = configGroupName.GetConfigWithCheck();

            return settings.LinkToExistingContainerAsync(databaseName, containerName);
        }

        public static async Task<Container> LinkToExistingContainerAsync(this CosmosDbSettings settings, string databaseName = null, string containerName = null)
        {
            var cosmosClient = new CosmosClient(settings.Endpoint, settings.AuthKey);

            var database = cosmosClient.GetDatabase(databaseName ?? settings.Database);
            var container = database.GetContainer(containerName ?? settings.Container);

            try
            {
                var response = await container.ReadContainerAsync();
                return response.StatusCode == HttpStatusCode.OK ? container : null;
            }
            catch
            {
                return null;
            }
        }

        public static DbContextOptions<TContext> GetCosmosEfCoreOptions<TContext>(this string configGroupName, string databaseName = null)
            where TContext : DbContext
        {
            var settings = configGroupName.GetConfigWithCheck();
            return settings.GetCosmosEfCoreOptions<TContext>(databaseName);
        }

        public static DbContextOptions<TContext> GetCosmosEfCoreOptions<TContext>(this CosmosDbSettings settings, string databaseName = null)
            where TContext : DbContext
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseCosmos(settings.Endpoint, settings.AuthKey,
                    databaseName ?? settings.Database).Options;
        }

        public static CosmosDbSettings GetConfigWithCheck(this string configGroupName)
        {
            var result = new CosmosDbSettings();

            var config = AppSettings.GetConfiguration();

            config.GetSection(configGroupName).Bind(result);

            if (result.Endpoint != null)
                return result; 
            
            config = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .Build();

            config.GetSection(configGroupName).Bind(result);

            if (result.Endpoint == null)
                throw new ArgumentException(
                    $"There isn't a group called {configGroupName} in the appsettings.json or app secrets");

            return result;
        }
    }
}