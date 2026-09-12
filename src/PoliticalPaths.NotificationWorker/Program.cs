using PoliticalPaths.Infrastructure;
using PoliticalPaths.Infrastructure.Messaging;
using PoliticalPaths.NotificationWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "rabbitmq.topology.json"), optional: false, reloadOnChange: true);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRabbitMqMessaging(builder.Configuration, publishOutbox: false);
builder.Services.AddOptions<DiscordOptions>().Bind(builder.Configuration.GetSection(DiscordOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddHttpClient<DiscordWebhookSender>();
builder.Services.AddHostedService<DiscordContactConsumer>();
if (builder.Configuration.GetValue<bool>("SendGrid:Enabled"))
{
    builder.Services.AddOptions<SendGridOptions>().Bind(builder.Configuration.GetSection(SendGridOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
    builder.Services.AddHttpClient<SendGridEmailSender>();
    builder.Services.AddHostedService<SendGridEmailConsumer>();
}
await builder.Build().RunAsync();
