using Discord;
using Discord.Interactions;
using FuelStopBot.CalcServices;
using System;
using System.Threading.Tasks;
using static FuelStopBot.CalcServices.FuelStopService;

namespace FuelStopBot.Modules
{
    public class PitStopModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("pitstops", "Calculate fuel strategy for a race")]
        public async Task PitStops(float VirtualEnergyPerLap, float RaceDurationInHours, float AverageLapTime)
        {
            var fuelStopService = new FuelStopService(VirtualEnergyPerLap, RaceDurationInHours * 3600, AverageLapTime);

            MessageComponent? embed = (await BuildResponse()).Build();
            await RespondAsync($"Calculating fuel strategy for a {RaceDurationInHours} hour race", components: embed);
        }

        private async Task<ComponentBuilder> BuildResponse()
        {
            var builder = new ComponentBuilder();
            builder.WithButton("Full Push", "full_push", ButtonStyle.Primary);
            return builder;
        }
    }
}
