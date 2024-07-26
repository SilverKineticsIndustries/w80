using System.Text.Encodings.Web;
using SilverKinetics.w80.Notifications;

namespace SilverKinetics.w80.Domain.UnitTests.Services;

[TestFixture(TestOf = typeof(Domain.Services.EmailMessageGenerator))]
public class EmailMessageGenerator
{
    [Test]
    public void GetEmailAccountOwnershipVerificationEmailMessage_generateForUser_emailTokenExpirationDateShouldBeAccordingToConfig()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var service = ctx.Services.GetRequiredService<IEmailMessageGenerator>();
            var configuration = ctx.Services.GetRequiredService<IConfiguration>();
            var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(configuration, "testuser1000@silverkinetics.dev", DateTime.UtcNow));
            var emailMessage = service.GetEmailAccountOwnershipVerificationEmailMessage(ctx.CreateUser(), token);

            Assert.That(emailMessage.Parameters["confirmationExpirationLength"],
                Is.EqualTo(configuration[Keys.EmailConfirmationLifetimeInHours]));
        }
    }

    [Test]
    public void GetEmailAccountOwnershipVerificationEmailMessage_generateForUser_confirmationUrlShouldBeCorrect()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var service = ctx.Services.GetRequiredService<IEmailMessageGenerator>();
            var configuration = ctx.Services.GetRequiredService<IConfiguration>();

            var user = ctx.CreateUser();
            var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(configuration, user.Email, DateTime.UtcNow));
            var emailMessage = service.GetEmailAccountOwnershipVerificationEmailMessage(user, token);

            var domain = configuration[Keys.Domain];
            var encodedToken = UrlEncoder.Default.Encode(token);
            var confirmationUrl = $"https://{domain}/#?code={encodedToken}";

            Assert.That(emailMessage.Parameters["confirmationUrl"], Is.EqualTo(confirmationUrl));
        }
    }

    [Test]
    public void GetEmailAccountOwnershipVerificationEmailMessage_generateForUser_userEmailShouldBePresent()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var service = ctx.Services.GetRequiredService<IEmailMessageGenerator>();
            var configuration = ctx.Services.GetRequiredService<IConfiguration>();
            var user = ctx.CreateUser();
            var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(configuration, user.Email, DateTime.UtcNow));

            var emailMessage = service.GetEmailAccountOwnershipVerificationEmailMessage(user, token);

            Assert.Multiple(() =>
            {
                Assert.That(emailMessage.EmailAddresses.Count, Is.EqualTo(1));
                Assert.That(emailMessage.EmailAddresses.Contains(user.Email), Is.True);
            });
        }
    }

    [Test]
    public void GetEmailAccountOwnershipVerificationEmailMessage_generateForUser_userNameShouldBePresent()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var service = ctx.Services.GetRequiredService<IEmailMessageGenerator>();
            var configuration = ctx.Services.GetRequiredService<IConfiguration>();
            var user = ctx.CreateUser();
            var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(configuration, user.Email, DateTime.UtcNow));

            var emailMessage = service.GetEmailAccountOwnershipVerificationEmailMessage(user, token);

            Assert.That(emailMessage.Parameters["name"].Contains(
                !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Email)
            );
        }
    }

    [Test]
    public void GetEmailAccountOwnershipVerificationEmailMessage_generateForUser_correntTemplateTypeShouldBePresent()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var service = ctx.Services.GetRequiredService<IEmailMessageGenerator>();
            var configuration = ctx.Services.GetRequiredService<IConfiguration>();
            var user = ctx.CreateUser();
            var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(configuration, user.Email, DateTime.UtcNow));
            var emailMessage = service.GetEmailAccountOwnershipVerificationEmailMessage(user, token);

            Assert.That(emailMessage.Template, Is.EqualTo(TemplateType.EmailConfirmation));
        }
    }

    [Test]
    public void GetEmailAccountOwnershipVerificationEmailMessage_generateForUser_correctCultureShoulddBePresent()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var service = ctx.Services.GetRequiredService<IEmailMessageGenerator>();
            var configuration = ctx.Services.GetRequiredService<IConfiguration>();
            var user = ctx.CreateUser();
            var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(configuration, user.Email, DateTime.UtcNow));
            var emailMessage = service.GetEmailAccountOwnershipVerificationEmailMessage(user, token);

            Assert.That(emailMessage.Culture, Is.EqualTo(user.Culture));
        }
    }

    [Test]
    public void GetEmailAccountOwnershipVerificationEmailMessage_generateForUser_allParametersInMessageShouldBeInTemplateMetadata()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var service = ctx.Services.GetRequiredService<IEmailMessageGenerator>();
            var configuration = ctx.Services.GetRequiredService<IConfiguration>();
            var user = ctx.CreateUser();
            var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(configuration, user.Email, DateTime.UtcNow));
            var emailMessage = service.GetEmailAccountOwnershipVerificationEmailMessage(user, token);

            Assert.That(emailMessage.Parameters.Keys, Is.SubsetOf(Templates.Meta[TemplateType.EmailConfirmation].ReplaceableFields));
        }
    }
}