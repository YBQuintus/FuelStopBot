
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelStopBot.CalcServices
{
    public class FuelStopService
    {
        private static readonly float secondsPerVirtualEnergyUnit = 0.4f;
        private float virtualEnergyUnitsPerLap = 0;
        private float raceDuration = 0;
        public float LapTime { get; private set; } = 0;
        private static int pitStopAddtionalTimeLoss = 27;

        private readonly static Dictionary<int, int> tireChangeTime = new Dictionary<int, int>()
        {
            { 0, 0 },
            { 1, 5 },
            { 2, 5 },
            { 3, 12 },
            { 4, 12 }
        };

        public FuelStopService(float veUnitsPerLap, float raceDurationIn, float lapTimeIn)
        {
            virtualEnergyUnitsPerLap = veUnitsPerLap;
            raceDuration = raceDurationIn;
            LapTime = lapTimeIn;
        }

        public async Task<List<int>> FullPushAsync()
        {
            return await Task.Run(() =>
            {
                float VE = 100;
                int tiresRemaining = AllowedTires((int)raceDuration / 3600) - 4;
                float VERequired = 0;
                int pushStintLength = (int)(100 / virtualEnergyUnitsPerLap);
                int currentStintLength = 0;
                int currentStintNumber = 1;
                List<int> stintAndStintLength = new List<int>();

                while (raceDuration > 0)
                {
                    VE -= virtualEnergyUnitsPerLap;
                    raceDuration -= LapTime;
                    currentStintLength++;

                    if (VE < virtualEnergyUnitsPerLap && raceDuration > 0)
                    {
                        raceDuration -= pitStopAddtionalTimeLoss;
                        VERequired = (int)raceDuration / LapTime < pushStintLength
                            ? (int)raceDuration / (int)LapTime * virtualEnergyUnitsPerLap + virtualEnergyUnitsPerLap
                            : 100;
                        raceDuration -= (VERequired - VE) * secondsPerVirtualEnergyUnit;
                        VE = VERequired;

                        if (tiresRemaining >= 4)
                        {
                            tiresRemaining -= 4;
                            raceDuration -= tireChangeTime[4];
                        }
                        else
                        {
                            raceDuration -= tireChangeTime[tiresRemaining];
                            tiresRemaining = 0;
                        }

                        stintAndStintLength.Add(currentStintLength);
                        currentStintLength = 0;
                        currentStintNumber++;
                    }

                    if (raceDuration < LapTime && raceDuration > 0)
                    {
                        // Final lap logic (optional)
                    }
                }

                return stintAndStintLength;
            });
        }

        static int AllowedTires(int durationHour)
        {
            if (durationHour <= 6)
            {
                return 18;
            }
            else if (durationHour <= 8)
            {
                return 26;
            }
            else
            {
                return 32;
            }
        }
    }
}
