using Discord;
using Discord.Interactions;
using FuelStopBot.CalcServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FuelStopBot.Modules
{
    public class PitStopModule(ILogger<PitStopModule> logger) : InteractionModuleBase<SocketInteractionContext>
    {

        private readonly ILogger<PitStopModule> _logger = logger;

        [SlashCommand("fuelstrat", "Calculate fuel strategy for a racetest")]
        public async Task PitStops(string VirtualEnergyPerLap, string RaceDurationInHours, string AverageLapTime)
        {
            await DeferAsync(ephemeral: true);

            if (!float.TryParse(VirtualEnergyPerLap, out float veFloat) ||
                !int.TryParse(RaceDurationInHours, out int raceHours) ||
                !int.TryParse(AverageLapTime, out int averageLapTimeInt))
            {
                await FollowupAsync("❌ Please ensure all inputs are valid numbers.");
                return;
            }

            var fuelStopService = new FuelStopService(veFloat, raceHours * 3600, averageLapTimeInt);
            var embed = (await BuildResponse(await fuelStopService.FullPushAsync())).Build();

            await FollowupAsync($"✅ Fuel strategy for a {RaceDurationInHours} hour race:", embed: embed);
        }

        private async Task<EmbedBuilder> BuildResponse(List<int> stintAndStintLength)
        {
            return await Task.Run(() =>
            {
                var embedBuilder = new EmbedBuilder()
                    .WithTitle("Fuel Stop Calculation Results")
                    .WithColor(Color.DarkGreen);

                StringBuilder stints = new($"## Stint Strategy{Environment.NewLine}");
                for (int i = 0; i < stintAndStintLength.Count; i++)
                {
                    stints.AppendLine($"- Stint {i + 1}: {stintAndStintLength[i]} laps");
                }

                embedBuilder.WithDescription(stints.ToString());
                return embedBuilder;
            });
        }
    }
}
