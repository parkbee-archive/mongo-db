using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ParkBee.MongoDb
{
    public class EntityTypeBuilder<T> where T : class
    {
        private readonly IMongoDatabase _database;
        public IMongoCollection<T> Collection { get; private set; }
        public Expression<Func<T, object>> KeyPropertyExpression { get; private set; }
        private Action<BsonClassMap<T>> _keyMapper;
        private readonly List<Action<BsonClassMap<T>>> _referenceMappers=new();
        private Action<BsonClassMap<T>> _definedMapper;

        public EntityTypeBuilder(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<T> ToCollection(string collectionName,
            MongoCollectionSettings? settings = null)
        {
            Collection = _database.GetCollection<T>(collectionName, settings);
            return Collection;
        }

        public EntityTypeBuilder<T> MapBson(Action<BsonClassMap<T>> classMapper)
        {
            _definedMapper = classMapper;
            return this;
        }

        public EntityTypeBuilder<T> HasKey(Expression<Func<T, object>> keyExpression)
        {
            KeyPropertyExpression = keyExpression;
            _keyMapper = cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(keyExpression);
            };
            return this;
        }

        internal void ConfigureClassMappers()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMapHelper.Unregister<T>();
            }

            if (_keyMapper != null || _referenceMappers.Any() || _definedMapper != null)
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    _keyMapper?.Invoke(cm);
                    _referenceMappers?.ForEach(rm=>rm.Invoke(cm));
                    _definedMapper?.Invoke(cm);
                });
            }
        }


        public EntityTypeBuilder<T> HasReferenceTo<TReference>(Expression<Func<TReference, object>> referenceIdSelector,
            Expression<Func<T, IEnumerable<object>>> refPropertySelector) where TReference : class, new()
        {
            var referencePropertyInfo = (refPropertySelector.Body as MemberExpression).Member as PropertyInfo;
            _referenceMappers.Add( cm =>
            {
                cm.AutoMap();
                cm.MapProperty(refPropertySelector)
                    .SetSerializer(new ReferenceClassBsonSerializer<TReference>(referencePropertyInfo.PropertyType,
                        referenceIdSelector));
            });
            return this;
        }
    }
}