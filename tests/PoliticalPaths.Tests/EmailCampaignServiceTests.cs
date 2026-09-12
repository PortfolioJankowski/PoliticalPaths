using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Dashboard.Services;
using PoliticalPaths.Infrastructure.Identity;
using PoliticalPaths.Infrastructure.Persistence;
using Xunit;

namespace PoliticalPaths.Tests;

public sealed class EmailCampaignServiceTests
{
    [Fact]
    public async Task Campaign_queues_only_active_confirmed_subscribers()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(options);
        db.Users.AddRange(
            User("subscribed@example.local", active: true, confirmed: true, consent: true),
            User("without-consent@example.local", active: true, confirmed: true, consent: false),
            User("inactive@example.local", active: false, confirmed: true, consent: true),
            User("unconfirmed@example.local", active: true, confirmed: false, consent: true));
        await db.SaveChangesAsync();
        var service = new EmailCampaignService(db);

        await service.CreateAsync(Guid.NewGuid(), "Ważna zmiana", "Treść wiadomości testowej");

        var campaign = Assert.Single(await db.EmailCampaigns.ToListAsync());
        Assert.Equal(1, campaign.RecipientCount);
        Assert.Equal("subscribed@example.local", Assert.Single(await db.EmailDeliveries.ToListAsync()).RecipientEmail);
        Assert.Equal("notification.email.broadcast", Assert.Single(await db.OutboxMessages.ToListAsync()).RoutingKey);
    }

    private static ApplicationUser User(string email, bool active, bool confirmed, bool consent) => new()
    {
        Id = Guid.NewGuid(), UserName = email, NormalizedUserName = email.ToUpperInvariant(),
        Email = email, NormalizedEmail = email.ToUpperInvariant(), IsActive = active,
        EmailConfirmed = confirmed, EmailNotificationsEnabled = consent
    };
}
