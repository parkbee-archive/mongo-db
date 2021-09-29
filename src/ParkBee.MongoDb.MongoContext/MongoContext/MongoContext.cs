using System.Threading.Tasks;

namespace ParkBee.MongoDb
{
    public class MongoContext
    {
        protected readonly IMongoContextOptionsBuilder OptionsBuilder;

        protected MongoContext(IMongoContextOptionsBuilder optionsBuilder)
        {
            OptionsBuilder = optionsBuilder;
            Task.Run(() => optionsBuilder.Configure(this, this.OnConfiguring)).Wait();
        }

        protected virtual Task OnConfiguring()
        {
            return Task.CompletedTask;
        }
    }
}