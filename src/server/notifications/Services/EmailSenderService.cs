using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Domain;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.Notifications.Services;

public class EmailSenderService(
    IConfiguration configuration,
    ILogger<EmailSenderService> logger) : IEmailSenderService
{
    public async Task<bool> SendAsync(TemplateType templateType, string subjectText, string bodyText, string[] addresses)
    {
        var key = configuration[Keys.Secrets.EmailSenderKey];
        var url = configuration[Keys.NotificationsEmailBaseAPI];
        var from = configuration[Keys.NotificationsFromEmailAddress];
        var fromName = configuration[Keys.NotificationsFromEmailAddress];

        if (string.IsNullOrWhiteSpace(key))
            throw new Exception("Unable to send emails because email sender key is missing.");

        if (string.IsNullOrWhiteSpace(url))
            throw new Exception("Unable to send emails because email base URL is missing.");

        if (string.IsNullOrWhiteSpace(from))
            throw new Exception("Unable to send emails because from email address missing.");

        var tos = string.Join(',', addresses);
        try
        {
            using(var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");

                var json =
                    JsonSerializer.Serialize(new
                    {
                        from = new {
                            email = from,
                            name = fromName
                        },
                        to = addresses.Select(addr => new { email = addr }).ToArray(),
                        subject = subjectText,
                        html = bodyText
                    });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync(url, content))
                {
                    response.EnsureSuccessStatusCode();

                    logger.LogInformation("An {templateType} email was successfully sent to {addresses} (MessageID: {messageId})",
                        templateType, tos,
                        response.Headers.Contains("x-message-id") ? response.Headers.GetValues("x-message-id").First() : "Unknown");

                    return true;
                }
            }
        }
        catch (HttpRequestException _) {
            logger.LogError(_, "Sending email failed (Template: {templateType}, Addresses: {addresses})", templateType, tos);
            throw;
        }
        catch (Exception _) {
            logger.LogError(_, "Error has occured while trying to email (Template: {templateType}, Addresses: {addresses})", templateType, tos);
            throw;
        }
    }
}