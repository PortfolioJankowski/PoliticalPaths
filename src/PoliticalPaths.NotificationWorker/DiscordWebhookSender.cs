using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using PoliticalPaths.Shared.Messaging;

namespace PoliticalPaths.NotificationWorker;

public sealed class DiscordWebhookSender(HttpClient client, IOptions<DiscordOptions> options)
{
    public async Task SendAsync(ContactMessageSubmitted message, CancellationToken ct)
    {
        var payload = new
        {
            username = options.Value.Username,
            allowed_mentions = new { parse = Array.Empty<string>() },
            embeds = new[]
            {
                new
                {
                    title = Truncate($"{message.Category}: {message.Subject}", 256),
                    description = Truncate(message.Body, 4096),
                    color = 4029695,
                    fields = new[] { new { name = "Użytkownik", value = Truncate(message.UserEmail, 1024), inline = true } },
                    footer = new { text = $"ContactMessage {message.MessageId}" },
                    timestamp = message.SubmittedAtUtc.ToString("O")
                }
            }
        };
        using var response = await client.PostAsJsonAsync(options.Value.WebhookUrl, payload, ct);
        response.EnsureSuccessStatusCode();
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
