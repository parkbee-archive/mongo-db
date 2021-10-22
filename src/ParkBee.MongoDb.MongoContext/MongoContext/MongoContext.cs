using System.Threading.Tasks;
using MongoDB.Driver;

namespace ParkBee.MongoDb
{
    public class MongoContext
    {
        protected readonly IMongoContextOptionsBuilder OptionsBuilder;
        public IMongoDatabase Database => OptionsBuilder.Database;

        protected MongoContext(IMongoContextOptionsBuilder optionsBuilder)
        {
            OptionsBuilder = optionsBuilder;
             optionsBuilder.Configure(this, this.OnConfiguring);
        }

        protected virtual void OnConfiguring()
        {
            
        }
    }
}