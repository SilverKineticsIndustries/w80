namespace SilverKinetics.w80.TestHelper.Fakes;

public class SecurityContextFake
    : ISecurityContext
{
    public ObjectId UserId { get; private set; }
    public Role Role { get; private set; }
    public string Language { get; } = SupportedCultures.DefaultCulture.Split("-")[0];
    public string Region { get; } = SupportedCultures.DefaultCulture.Split("-")[1];

    public void SetToAdmin(TestContext ctx)
    {
        UserId = ctx.GetAdminUserID();
        Role = Role.Administrator;
    }

    public void SetToUser(TestContext ctx)
    {
        UserId = ctx.GetTestUserID();
        Role = Role.User;
    }

    public bool CanAccess(ObjectId userId)
    {
        return UserId == userId || Role == Role.Administrator || Role == Role.ServiceWorker;
    }
}