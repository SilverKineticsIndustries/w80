namespace SilverKinetics.w80.Application.UnitTests.Security;

[TestFixture(TestOf = typeof(Application.Security.Invitations))]
public class Invitations
{
    [Test]
    public void Decrypt_decryptToken_decryptionShouldWork()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var email = "testuser1000@silverkinetics.dev";
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var token = Convert.ToBase64String(Application.Security.Invitations.GenerateNew(config, email, now));
            Application.Security.Invitations.Decrypt(config, token, out string decryptedEmail, out DateTime decryptedDateTime);
            Assert.That(email, Is.EqualTo(decryptedEmail));
            Assert.That(now, Is.EqualTo(decryptedDateTime));
        }
    }

    [Test]
    public void IsExpired_tokenWithDateThatIsAfterExpired_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var invitationLifetime = Convert.ToInt32(config[Keys.InvitationCodeLifetimeInHours]);
            Assert.That(Application.Security.Invitations.IsExpired(config, now, now.AddHours(invitationLifetime + 1)), Is.True);
        }
    }

    [Test]
    public void IsExpired_tokenWithDateThatIsBeforeExpired_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var invitationLifetime = Convert.ToInt32(config[Keys.InvitationCodeLifetimeInHours]);
            Assert.That(Application.Security.Invitations.IsExpired(config, now, now.AddHours(invitationLifetime - 1)), Is.False);
        }
    }

    [Test]
    public void IsExpired_tokenWithDateThatIsExactlyExpired_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var invitationLifetime = Convert.ToInt32(config[Keys.InvitationCodeLifetimeInHours]);
            Assert.That(Application.Security.Invitations.IsExpired(config, now, now.AddHours(invitationLifetime)), Is.False);
        }
    }
}