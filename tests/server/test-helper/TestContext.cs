using Microsoft.AspNetCore.Mvc;

namespace SilverKinetics.w80.TestHelper;

public class TestContext
    : IDisposable
{
    public IServiceProvider Services { get; private set; }

    public TestContext(IServiceProvider serviceProvider, string databaseName)
    {
        Services = serviceProvider;
        _databaseName = databaseName;
        _mongoClient = Services.GetRequiredService<IMongoClient>();
    }

    public async Task<TestContext> SeedDatabaseAsync()
    {
        await DataSeeder.SeedTestDataAsync(Services, _databaseName);
        ((SecurityContextFake)Services.GetRequiredService<ISecurityContext>()).SetToUser(this);
        return this;
    }

    public ObjectId GetCurrentUserId()
    {
        var securityContext = Services.GetRequiredService<ISecurityContext>();
        return securityContext.UserId;
    }

    public IConfiguration Config()
    {
        return Services.GetRequiredService<IConfiguration>();
    }

    public void SetUserToAdmin()
    {
        var securityContext = (SecurityContextFake)Services.GetRequiredService<ISecurityContext>();
        securityContext.SetToAdmin(this);
    }

    public void SetUserToUser()
    {
        var securityContext = (SecurityContextFake)Services.GetRequiredService<ISecurityContext>();
        securityContext.SetToUser(this);
    }

    public string GetTestUserEmail()
    {
        VerifyUsersLoaded();
        return _testUser.Email;
    }

    public ObjectId GetTestUserID()
    {
        VerifyUsersLoaded();
        return _testUser.Id;
    }

    public string GetAdminUserEmail()
    {
        VerifyUsersLoaded();
        return _adminUser.Email;
    }

    public ObjectId GetAdminUserID()
    {
        VerifyUsersLoaded();
        return _adminUser.Id;
    }

    public string GetTestUserPassword()
    {
        return Config()["Tests_User_Password"] ?? throw new Exception("Missing test user password");
    }

    public string GetAdminUserPassword()
    {
        return Config()["Tests_Admin_Password"] ?? throw new Exception("Missing test admin password");
    }

    public async Task<string> ProfileAsync(Func<Task> action)
    {
        EnableDatabaseProfiling();
        await action();
        DisableDatabaseProfiling();
        return GetProfileInformation();
    }

    public void EnableDatabaseProfiling()
    {
        var database = Services.GetRequiredService<IMongoDatabase>();
        var profileCommand = new BsonDocument("profile", 2);
        database.RunCommand<BsonDocument>(profileCommand);
    }

    public void DisableDatabaseProfiling()
    {
        var database = Services.GetRequiredService<IMongoDatabase>();
        var profileCommand = new BsonDocument("profile", 0);
        database.RunCommand<BsonDocument>(profileCommand);
    }

    public string GetProfileInformation()
    {
        var database = Services.GetRequiredService<IMongoDatabase>();
        var collection = database.GetCollection<BsonDocument>("system.profile");
        var doc = collection.Find(new BsonDocument()).FirstOrDefault();
        var dotNetObj = BsonTypeMapper.MapToDotNetValue(doc);
        return Newtonsoft.Json.JsonConvert.SerializeObject(dotNetObj);
    }

    public async void Dispose()
    {
        if (_mongoClient != null)
        {
            //TODO: Do we need to hide this?
            try
            {
                await _mongoClient.DropDatabaseAsync(_databaseName);
            } catch {}
        }

        if (Services is IDisposable disposable)
            disposable.Dispose();
    }

    private void VerifyUsersLoaded()
    {
        _testUser ??= Services.GetRequiredService<IMongoCollection<User>>().AsQueryable().First(x => x.Role == Role.User);
        _adminUser ??= Services.GetRequiredService<IMongoCollection<User>>().AsQueryable().First(x => x.Role == Role.Administrator);
    }

    private string _databaseName;
    private IMongoClient _mongoClient;
    private static User _testUser = null!;
    private static User _adminUser = null!;
}
