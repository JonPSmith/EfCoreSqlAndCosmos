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

        public static async Task<bool> CheckCosmosDbContainerExistsAsync(this string configSectionName, string databaseName = null,
            string containerName = null)
        {
            return (await configSectionName.LinkToExistingContainerAsync(databaseName, containerName)) != null;
        }

        public static Task<Container> LinkToExistingContainerAsync(this string configSectionName, string databaseName = null, string containerName = null)
        {
            var settings = configSectionName.GetConfigWithCheck();
            return settings?.LinkToExistingContainerAsync(databaseName, containerName);
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

        public static DbContextOptions<TContext> GetCosmosEfCoreOptions<TContext>(this string configSectionName, string databaseName = null)
            where TContext : DbContext
        {
            var settings = configSectionName.GetConfigWithCheck();
            return settings?.GetCosmosEfCoreOptions<TContext>(databaseName);
        }

        public static DbContextOptions<TContext> GetCosmosEfCoreOptions<TContext>(this CosmosDbSettings settings, string databaseName = null)
            where TContext : DbContext
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseCosmos(settings.Endpoint, settings.AuthKey,
                    databaseName ?? settings.Database).Options;
        }

        /// <summary>
        /// This returns the Cosmos DB setting for the given section name
        /// If no section found then returns null.
        /// </summary>
        /// <param name="configSectionName"></param>
        /// <returns></returns>
        public static CosmosDbSettings GetConfigWithCheck(this string configSectionName)
        {
            var result = new CosmosDbSettings();

            var config = AppSettings.GetConfiguration();

            config.GetSection(configSectionName).Bind(result);

            if (result.Endpoint != null)
                return result; 
            
            config = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .Build();

            config.GetSection(configSectionName).Bind(result);

            return result.Endpoint == null ? null : result;
        }
    }
}