using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace ParkBee.MongoDb
{
    public interface IMongoContextOptionsBuilder
    {
        IMongoDatabase Database { get; }
        
        void Configure(MongoContext context, Action configAction);

        MemberExpression GetFilterByKeyExpression<TEntity>() where TEntity : class;

        EntityTypeBuilder<TEntity> Entity<TEntity>(Action<EntityTypeBuilder<TEntity>> buildAction)
            where TEntity : class;
    }
}