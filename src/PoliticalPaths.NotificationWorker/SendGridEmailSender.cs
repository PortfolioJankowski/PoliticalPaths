using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using PoliticalPaths.Shared.Messaging;

namespace PoliticalPaths.NotificationWorker;

public sealed class SendGridEmailSender(HttpClient client, IOptions<SendGridOptions> options)
{
    public async Task<string?> SendAsync(EmailNotificationRequested message, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
        request.Content = JsonContent.Create(new
        {
            personalizations = new[] { new { to = new[] { new { email = message.RecipientEmail } } } },
            from = new { email = options.Value.FromEmail, name = options.Value.FromName },
            subject = message.Subject,
            content = new[] { new { type = "text/plain", value = message.Body } }
        });
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return response.Headers.TryGetValues("X-Message-Id", out var values) ? values.FirstOrDefault() : null;
    }
}
