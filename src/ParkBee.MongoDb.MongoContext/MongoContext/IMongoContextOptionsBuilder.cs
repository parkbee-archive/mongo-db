using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ParkBee.MongoDb
{
    public interface IMongoContextOptionsBuilder
    {
        Task Configure(MongoContext context, Func<Task> configAction);

        MemberExpression GetFilterByKeyExpression<TEntity>() where TEntity : class;

        Task<EntityTypeBuilder<TEntity>> Entity<TEntity>(Func<EntityTypeBuilder<TEntity>, Task> buildAction)
            where TEntity : class;
    }
}