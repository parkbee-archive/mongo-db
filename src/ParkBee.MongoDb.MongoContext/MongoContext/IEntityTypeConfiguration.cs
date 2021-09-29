using System.Threading.Tasks;

namespace ParkBee.MongoDb
{
    public interface IEntityTypeConfiguration<TEntity>
        where TEntity : class
    {
        Task Configure(EntityTypeBuilder<TEntity> builder);
    }
}