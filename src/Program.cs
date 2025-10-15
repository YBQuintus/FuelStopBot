using Discord.Interactions;
using Discord.WebSocket;
using FuelStopBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        DiscordSocketConfig config = new()
        {
            UseInteractionSnowflakeDate = false
        };

        DiscordSocketClient client = new(config);
        services.AddSingleton(client);       // Add the discord client to services
        services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(client);
        });
        // Add the interaction service to services
        services.AddHostedService<InteractionHandlingService>();    // Add the slash command handler
        services.AddHostedService<DiscordStartupService>();         // Add the discord startup service
    })
    .Build();

await host.RunAsync();