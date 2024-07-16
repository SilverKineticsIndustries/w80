namespace SilverKinetics.w80.Application.UnitTests.Security;

[TestFixture(TestOf = typeof(Application.Security.EmailConfirmations))]
public class EmailConfirmations
{
    [Test]
    public void Decrypt_decryptToken_decryptionShouldWork()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var email = "testuser1000@silverkinetics.dev";
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var token = Convert.ToBase64String(Application.Security.EmailConfirmations.GenerateNew(config, email, now));
            Application.Security.EmailConfirmations.Decrypt(config, token, out string decryptedEmail, out DateTime decryptedDateTime);
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
            var emailConfirmationLifetime = Convert.ToInt32(config[Keys.EmailConfirmationLifetimeInHours]);
            Assert.That(Application.Security.EmailConfirmations.IsExpired(config, now, now.AddHours(emailConfirmationLifetime + 1)), Is.True);
        }
    }

    [Test]
    public void IsExpired_tokenWithDateThatIsBeforeExpired_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var emailConfirmationLifetime = Convert.ToInt32(config[Keys.EmailConfirmationLifetimeInHours]);
            Assert.That(Application.Security.EmailConfirmations.IsExpired(config, now, now.AddHours(emailConfirmationLifetime - 1)), Is.False);
        }
    }

    [Test]
    public void IsExpired_tokenWithDateThatIsExactlyOnExpirationDate_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var emailConfirmationLifetime = Convert.ToInt32(config[Keys.EmailConfirmationLifetimeInHours]);
            Assert.That(Application.Security.EmailConfirmations.IsExpired(config, now, now.AddHours(emailConfirmationLifetime)), Is.False);
        }
    }
}