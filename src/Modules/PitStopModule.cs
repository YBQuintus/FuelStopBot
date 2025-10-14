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
        public async Task PitStops(float VirtualEnergyPerLap, float RaceDuration, float AverageLapTime)
        {
            var fuelStopService = new FuelStopService(VirtualEnergyPerLap, RaceDuration, AverageLapTime);
            await RespondAsync($"Calculating...{fuelStopService.LapTime}", ephemeral: true);
        }
    }
}
