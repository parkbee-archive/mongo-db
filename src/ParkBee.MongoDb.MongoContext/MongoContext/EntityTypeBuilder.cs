using System;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ParkBee.MongoDb
{
    public class EntityTypeBuilder<T> where T : class
    {
        private readonly IMongoDatabase _database;
        public IMongoCollection<T> Collection { get; private set; }
        public Expression<Func<T, object>> KeyPropertyExpression { get; private set; }

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
            if (BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMapHelper.Unregister<T>();
            }

            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                classMapper.Invoke(cm);
                if (KeyPropertyExpression != null)
                {
                    cm.AutoMap();
                    cm.MapIdMember(KeyPropertyExpression);
                }
            });
            return this;
        }

        public EntityTypeBuilder<T> HasKey(Expression<Func<T, object>> keyExpression)
        {
            KeyPropertyExpression = keyExpression;
            return this;
        }

        internal void MapBsonIdToKey()
        {
            if (KeyPropertyExpression != null && !BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(KeyPropertyExpression);
                });
            }
        }
    }
}