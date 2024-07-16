using Microsoft.AspNetCore.Http;

namespace SilverKinetics.w80.Application.Contracts;

public interface IReCaptchaApplicationService
{
    Task VerifyRequestNotFromBotAsync(string response, CancellationToken cancellationToken);
}