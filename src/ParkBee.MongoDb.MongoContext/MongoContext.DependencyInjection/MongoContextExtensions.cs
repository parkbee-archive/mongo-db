using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace ParkBee.MongoDb.DependencyInjection
{
    public static class MongoContextExtensions
    {
        public static void AddMongoContext<TContextService, TContextImplementation>(this IServiceCollection services,
            Action<MongoContextOptions> mongoContextOptionsAction, IEventSubscriber mongoEventSubscriber = null)
            where TContextImplementation : MongoContext, TContextService
            where TContextService : class
        {
            AddRequiredServices(services, mongoContextOptionsAction, mongoEventSubscriber);

            services.AddScoped<TContextService, TContextImplementation>();
        }

        public static void AddMongoContext<TContextImplementation>(this IServiceCollection services,
            Action<MongoContextOptions> mongoContextOptionsAction, IEventSubscriber mongoEventSubscriber = null)
            where TContextImplementation : MongoContext
        {
            AddRequiredServices(services, mongoContextOptionsAction, mongoEventSubscriber);

            services.AddScoped<TContextImplementation>();
        }

        private static void AddRequiredServices(IServiceCollection services,
            Action<MongoContextOptions> mongoContextOptionsAction, IEventSubscriber mongoEventSubscriber)
        {
            services.Configure(mongoContextOptionsAction);

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MongoContextOptions>>();
                var settings = MongoClientSettings.FromConnectionString(options.Value.ConnectionString);
                if (mongoEventSubscriber != null)
                    settings.ClusterConfigurator = builder => builder.Subscribe(mongoEventSubscriber);
                var client = new MongoClient(settings);

                return client.GetDatabase(options.Value.DatabaseName);
            });
            services.AddSingleton<IMongoContextOptionsBuilder>(provider =>
            {
                var database = provider.GetRequiredService<IMongoDatabase>();
                return new MongoContextOptionsBuilder(database);
            });
        }
    }
}