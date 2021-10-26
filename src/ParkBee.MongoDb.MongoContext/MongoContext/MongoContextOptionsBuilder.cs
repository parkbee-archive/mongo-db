#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Driver;
using ParkBee.MongoDb.Extensions;

namespace ParkBee.MongoDb
{
    public class MongoContextOptionsBuilder : IMongoContextOptionsBuilder
    {
        private bool IsConfigured { get; set; }
        
        private readonly IDictionary<Type, object> _entityToBuilderMap =
            new Dictionary<Type, object>();

        private readonly IMongoDatabase _database;
        
        public IMongoDatabase Database => _database;

        public MongoContextOptionsBuilder(IMongoDatabase database)
        {
            _database = database;
        }

        public void Configure(MongoContext context, Action configAction)
        {
            if (!IsConfigured)
            {
                configAction.Invoke();
                ConfigureMappers();
            }

            //get collection properties in context
            var contextProperties = context.GetType().GetRuntimeProperties()
                .Where(
                    p => !(p.GetMethod ?? p.SetMethod)!.IsStatic
                         && !p.GetIndexParameters().Any()
                         && p.DeclaringType != typeof(MongoContext)
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .OrderBy(p => p.Name)
                .Select(
                    p => (
                        p.Name,
                        Type: p.PropertyType.GenericTypeArguments.Single()
                    ))
                .ToArray();

            // set value of each collection property
            foreach (var propertyInfo in contextProperties)
            {
                var dbSetType = typeof(DbSet<>).MakeGenericType(propertyInfo.Type);
                var dbSet = Activator.CreateInstance(dbSetType, this, context, GetCollectionInstance(propertyInfo));

                context.GetType().GetProperty(propertyInfo.Name).SetValue(context, dbSet);
            }

            ConfigureIndexes();

            IsConfigured = true;
        }

        public MemberExpression GetFilterByKeyExpression<TEntity>() where TEntity : class
        {
            var builder = _entityToBuilderMap[typeof(TEntity)] as EntityTypeBuilder<TEntity>;
            if (builder.KeyPropertyExpression == null)
            {
                throw new InvalidOperationException(
                    $"You should configure ket property of {typeof(TEntity)} before using ByKeyOperations. This can be done by calling HasKey method of EntityBuilder inside OnConfiguring method of your context");
            }

            return ((builder.KeyPropertyExpression as LambdaExpression).Body as UnaryExpression).Operand as
                MemberExpression;
        }

        private object GetCollectionInstance((string Name, Type Type) propertyInfo)
        {
            var builderType = typeof(EntityTypeBuilder<>).MakeGenericType(propertyInfo.Type);

            var builder = _entityToBuilderMap.ContainsKey(propertyInfo.Type)
                ? _entityToBuilderMap[propertyInfo.Type]
                : Activator.CreateInstance(builderType, Database);
            var collectionProperty = builderType.GetProperty("Collection");
            var collection = collectionProperty.GetValue(_entityToBuilderMap[propertyInfo.Type]);

            if (collection == null)
            {
                MethodInfo getCollectionMethod = Database.GetType().GetMethod("GetCollection") ??
                                                 throw new InvalidOperationException(
                                                     "IMongoDatabase don't expose GetCollection method which is expected");
                MethodInfo generic = getCollectionMethod.MakeGenericMethod(propertyInfo.Type);

                collection =
                    generic.Invoke(Database, new object?[] { propertyInfo.Name, null }) ??
                    throw new InvalidOperationException(
                        $"GetCollection<{propertyInfo.Type}>(\"{propertyInfo.Name}\") doesn't return a value");

                collectionProperty.SetValue(builder, collection);
                _entityToBuilderMap[propertyInfo.Type] = builder;
            }

            return collection;
        }

        private void ConfigureMappers()
        {
            InvokeEntityBuildersMethod("ConfigureMappers");
        }

        private void ConfigureIndexes()
        {
            InvokeEntityBuildersMethod("ConfigureIndexes");
        }

        private void InvokeEntityBuildersMethod(string methodName)
        {
            foreach (var map in _entityToBuilderMap)
            {
                var builderType = typeof(EntityTypeBuilder<>).MakeGenericType(map.Key);
                var builder = _entityToBuilderMap[map.Key];

                builderType.InvokeMember(methodName,
                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder, builder, null);
            }
        }

        public EntityTypeBuilder<TEntity> Entity<TEntity>(
            Action<EntityTypeBuilder<TEntity>> buildAction)
            where TEntity : class
        {
            var builder = _entityToBuilderMap.ContainsKey(typeof(TEntity))
                ? _entityToBuilderMap[typeof(TEntity)] as EntityTypeBuilder<TEntity>
                : new EntityTypeBuilder<TEntity>(Database);
            buildAction.Invoke(builder);
            _entityToBuilderMap[typeof(TEntity)] = builder;
            return builder;
        }

        public void ApplyConfigurationsFromAssembly(
            Assembly assembly,
            Func<Type, bool>? predicate = null)
        {
            var configureMethod = typeof(IEntityTypeConfiguration<>)
                .GetMethod("Configure");

            foreach (var type in assembly.GetConstructibleTypes().OrderBy(t => t.FullName))
            {
                // Only accept types that contain a parameterless constructor, are not abstract and satisfy a predicate if it was used.
                if (type.GetConstructor(Type.EmptyTypes) == null
                    || (!predicate?.Invoke(type) ?? false))
                {
                    continue;
                }

                var configInterface = type.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
                if (configInterface == null)
                {
                    continue;
                }

                var entityType = configInterface.GetGenericArguments().First();
                var builder = _entityToBuilderMap.ContainsKey(entityType)
                    ? _entityToBuilderMap[entityType]
                    : Activator.CreateInstance(typeof(EntityTypeBuilder<>).MakeGenericType(entityType), Database);
                var configurationClass = Activator.CreateInstance(type);

                configureMethod.Invoke(configurationClass, new[] { builder });
                _entityToBuilderMap[entityType] = builder;
            }
        }
    }
}