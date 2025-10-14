using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FuelStopBot.FuelStopService.FuelStopService;

namespace FuelStopBot.Modules
{
    public class PitStopModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("pitstops", "Calculate fuel strategy for a race")]
        public async Task PitStops(float veUnitsPerLap, float raceDurationIn, float lapTimeIn)
        {
            await RespondAsync("Calculating...", ephemeral: true);
        }
    }
}
