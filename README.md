
# MongoContext
This project intended to solve these problems:
- Provide a neat and centralized way to configure MongoDb
- Provide a all in one place access to all collections
- Extend MongoDb C# Driver features
## Getting started
Install [ParkBee.MongoDb nuget package](https://www.nuget.org/packages/ParkBee.MongoDb/) in your data access / persistance layer:

    Install-Package ParkBee.MongoDb
    
Create a context class which inherits from **MongoContext**, override **OnConfiguring** method and put all your configuration logic there. 
```
public class MyApplicationContext : MongoContext  
{  
   public DbSet<User> Users { get; set; }

   protected override async Task OnConfiguring()  
   {  
        await OptionsBuilder.Entity<User>(async entity =>  
        {  
            var usersCollection = entity.ToCollection("ApplicationUsers");  
            var searchByEmail = new CreateIndexModel<Permit>(Builders<User>.IndexKeys  
                .Ascending(u => u.Email));  
  
            await permitsCollection.Indexes.CreateOneAsync(searchByEmail);  
  
            entity.HasKey(p => p.UserId);  
        });  
   }

    public MyApplicationContext(IMongoContextOptionsBuilder optionsBuilder) : base(optionsBuilder)  
    {  
    }  
}
```
Install [ParkBee.MongoDb.DependencyInjection nuget package](https://www.nuget.org/packages/ParkBee.MongoDb.DependencyInjection/) in your api / presentation layer:

    Install-Package ParkBee.MongoDb.DependencyInjection
Register your mongo context in **Startup.cs** in **ConfigureServices** method
```
services.AddMongoContext<MyApplicationContext>(options =>
{
   options.ConnectionString = "mongodb://localhost";
   options.DatabaseName = "MyApplication";
});
```
Use the context which injected in your controller:
```
public class UsersController : ControllerBase
{
    private readonly MyApplicationContext _context;

    public UsersController(MyApplicationContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery]SearchUsersRequest request, [FromQuery]QueryOptions queryOptions, CancellationToken cancellationToken)
    {
        var users = await _context.Users.ToListAsync(cancellationToken);
        
        return Ok(users);
    }
```
### Explaining the context code
**1.** We introduce a property of type `DbSet<User>`:

```public DbSet<User> Users { get; set; }```

A DbSet is a wrapper class around IMongoCollection object and adds some features to it.
You can use it as a collection object e.g. 

`await _context.Users.FindAsync(u => u.UserId == "something");`

Or you can use it as IQueryable e.g.

`await _context.Users.Where(u => u.UserId == "something").FirstOrDefaultAsync()`
>Note: for most of IQueryable operations you should reference _MongoDB.Driver.Linq_

And last but not least, DbSet provides functionality to get, update and delete documents by a key value. please refer to [HasKey section of this document](#haskey-sec)

**2.** Configuring Users collection:

We configure each collection inside `OptionsBuilder.Entity<>` method.
## Configuration Options
Inside `OptionsBuilder.Entity<>` we can use several methods to configure collections:
### ToCollection
Use this method to map an entity to a collection. By default add properties of context class with `DbSet<>` type will be mapped with the same collection name as property name.

So in our sample code if we don't use ToCollection method, this property will be mapped to a collection with name _Users_.
### MapBson
This method is the place for put mapping logic between MongoDb and entity classes.

It provides the same way that used in BsonClassMap e.g.:
```
await OptionsBuilder.Entity<InternalUser>(async b =>
{
    b.MapBson(cm =>
    {
        cm.AutoMap();
        cm.SetDiscriminator(nameof(InternalUser));
        cm.MapField("_products").SetElementName(nameof(InternalUser.Products));
        cm.SetIgnoreExtraElements(true);
    });
});
```
### <a name="haskey-sec"></a>HasKey
This method maps a property of the entity class to `_id` field in MongoDb and opens the possibility of usage of **ByKey** functionality of a DbSet:

#### FindByKey 
It's a simple way to get a single document by it's configured key e.g.
```
var user = await _context.Users.FindByKey("userid");
```
#### FindByKey
Simply get a and update a single document e.g.
```
var user = await _context.Users.UpdateByKey("userid", Builders<User>.Update.Set(p => p.FirstName, "New Name"));
```
#### ReplaceByKey
Replace a single document fetched by it's key e.g.
```
var user = await _context.Users.ReplaceByKey("userid", new User{ UserId = "userid", FirstName = "New Name"});
```
#### DeleteByKey
Delete a single document by it's key e.g.
```
var user = await _context.Users.DeleteByKey("userid");
```
### HasReferenceTo
This feature is intended to automate the reference functionality of MongoDb.Driver which should be done manually at the moment. by using this method you will have reference between collections.
> Referencing between collections is not a common use case, so please be cautious about using it widely
  
In current version  (1.1.0) using this method will save and reterive referenced data in a proper way but loading related data from other collections should be implement by the developer manually.

**Sample usage:**
Assume these entity classes:
```
public class PermissionGroup
{
    public Guid PermissionGroupId { get; set; }
    public string Name { get; set; }

    public IReadOnlyList<Permission> Permissions { get; set; }
}
```
```
public class Permission
{
    public string PermissionId { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
}
```
Context code:
```
public class PermissionsContext : MongoContext
{
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<PermissionGroup> PermissionGroups { get; set; }

    protected override async Task OnConfiguring()
    {
        await OptionsBuilder.Entity<Permission>(async b =>
        {
            b.HasKey(p => p.PermissionId);
        });
                    
        await OptionsBuilder.Entity<PermissionGroup>(async b =>
        {
            b.HasKey(p => p.PermissionGroupId)
                .HasReferenceTo<Permission>(p => p.PermissionId, pg => pg.Permissions);
        });
    }
}
```
Then, when I add a PermissionGroup to the MongoDb using code:
```
var permissionGroup = new PermissionGroup
{
    PermissionGroupId = Guid.NewGuid(),
    Name = "something",
    Permissions = new []
    {
        new Permission
        {
            PermissionId = "1",
            Name = "test",
            Description = "test"
        
        },
        new Permission
        {
            PermissionId = "2",
            Name = "test2",
            Description = "test2"
        
        }
    }
};
await _context.PermissionGroups.InsertOneAsync(permissionGroup);
```
The created document would be like this:
```
{
    "_id" : NUUID("4d42f53d-859d-4a53-a457-423d57f57744"),
    "Name" : "something",
    "Permissions" : [ 
        "1", 
        "2"
    ]
}
```
And if you want to get the document with all data loaded, you should do it like this:
```
var permissionGroup = await _context.PermissionGroups.FindByKey(Guid.Parse("4d42f53d-859d-4a53-a457-423d57f57744"));
if (permissionGroup != null && permissionGroup.Permissions != null)
{
    var permissionIds = permissionGroup.Permissions.Select(pg => pg.PermissionId);
    permissionGroup.Permissions = await _context.Permissions
        .Where(p => permissionIds.Contains(p.PermissionId))
        .ToListAsync(cancellationToken);
}
```

## Future features
We plan to add lazy-loading feature for the referenced property