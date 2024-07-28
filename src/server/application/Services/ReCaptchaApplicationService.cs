using System.Web;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Application.Exceptions;

namespace SilverKinetics.w80.Application.Services;

public class ReCaptchaApplicationService(
    IConfiguration configuration,
    ILogger<ReCaptchaApplicationService> logger
) : IReCaptchaApplicationService
{
    public async Task VerifyRequestNotFromBotAsync(string response, CancellationToken cancellationToken)
    {
        if (Convert.ToBoolean(configuration[Keys.ReCaptchaEnabled]) == false)
            return;

        var key = configuration[Keys.Secrets.CaptchaKey];
        if (string.IsNullOrWhiteSpace(key))
            throw new Exception("ReCaptcha key is missing.");

        var url = configuration[Keys.ReCaptchaValidationEndpointURL];
        if (string.IsNullOrWhiteSpace(url))
            throw new Exception("ReCaptcha validation endpoint URL is missing.");

        if (string.IsNullOrWhiteSpace(response))
            throw new Exception("ReCaptcha response is missing.");

        try
        {
            using(var httpClient = new HttpClient())
            {
                url = url + '?' + HttpUtility.UrlPathEncode($"secret={key}&response={response}");
                using (var verificationReponse = await httpClient.PostAsync(url, null, cancellationToken))
                {
                    verificationReponse.EnsureSuccessStatusCode();
                    var body = await verificationReponse.Content.ReadAsStringAsync(cancellationToken);
                    JObject data = JObject.Parse(body);
                    if (!data.TryGetValue("success", out JToken? value) || value.ToString() != bool.TrueString)
                        throw new ReCaptchaFailedException();
                }
            }
        }
        catch (HttpRequestException _) {
            logger.LogError(_, "ReCaptcha verification call failed.");
            throw;
        }
        catch (Exception _) {
            logger.LogError(_, "Error has occured while trying to verify ReCaptcha.");
            throw;
        }
    }

    private const string CaptchaResponseFieldName = "g-recaptcha-response";
}