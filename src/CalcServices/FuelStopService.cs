
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FuelStopBot.CalcServices
{
    public class FuelStopService(float veUnitsPerLap, float raceDurationIn, float lapTimeIn)
    {
        private static readonly float secondsPerVirtualEnergyUnit = 0.4f;
        private float virtualEnergyUnitsPerLap = veUnitsPerLap;
        private float raceDuration = raceDurationIn;
        public float LapTime { get; private set; } = lapTimeIn;
        private static int pitStopAddtionalTimeLoss = 27;
        private List<int> stintLengths = new List<int>();

        private readonly static Dictionary<int, int> tireChangeTime = new Dictionary<int, int>()
        {
            { 0, 0 },
            { 1, 5 },
            { 2, 5 },
            { 3, 12 },
            { 4, 12 }
        };

        public async Task<(List<int>, float)> FullPushAsync()
        {
            return await Task.Run(() =>
            {
                float VE = 100;
                int tiresRemaining = AllowedTires((int)raceDuration / 3600) - 4;
                float VERequired = 0;
                int pushStintLength = (int)(100 / virtualEnergyUnitsPerLap);
                int currentStintLength = 0;
                int currentStintNumber = 1;
                float raceDurationCopy = raceDuration;
                float raceDurationEnd = 0;
                stintLengths.Clear();

                while (raceDurationCopy > 0)
                {
                    VE -= virtualEnergyUnitsPerLap;
                    raceDurationCopy -= LapTime;
                    currentStintLength++;

                    if (VE < virtualEnergyUnitsPerLap && raceDurationCopy > 0)
                    {
                        raceDurationCopy -= pitStopAddtionalTimeLoss;
                        VERequired = (int)raceDurationCopy / LapTime < pushStintLength
                            ? (int)raceDurationCopy / (int)LapTime * virtualEnergyUnitsPerLap + virtualEnergyUnitsPerLap
                            : 100;
                        raceDurationCopy -= (VERequired - VE) * secondsPerVirtualEnergyUnit;
                        
                        if (tiresRemaining >= 4)
                        {
                            raceDurationCopy -= tireChangeTime[4];
                            tiresRemaining -= 4;
                            
                        }
                        else
                        {
                            raceDurationCopy -= tireChangeTime[tiresRemaining];
                            
                            tiresRemaining = 0;
                        }
                        VE = VERequired;

                        stintLengths.Add(currentStintLength);
                        currentStintLength = 0;
                        currentStintNumber++;
                    }

                    if (raceDurationCopy < LapTime && raceDurationCopy > 0)
                    {
                        stintLengths.Add(currentStintLength + 1);
                        raceDurationEnd = raceDurationCopy;
                    }
                }
                Console.WriteLine(string.Join(", ", stintLengths));
                return (stintLengths, raceDurationEnd);
            });
        }

        public async Task<(List<int>, float)> OptimiseAsync()
        {
            return await Task.Run(() =>
            {
                int nominalStintLength = (int)(100 / virtualEnergyUnitsPerLap);
                int tiresRemaining = AllowedTires((int)raceDuration / 3600) - 4;
                float raceDurationCopy = raceDuration;
                float raceDurationEnd = 0;
                if (stintLengths[^1] < stintLengths.Count - 1)
                {
                    for (int i = 0; i < stintLengths[^1]; i++)
                    {
                        stintLengths[i] = stintLengths[i] + 1;
                    }
                    stintLengths.RemoveAt(stintLengths.Count - 1);
                }
                for (int i = 0; i < stintLengths.Count; i++)
                {
                    float racePace = LapTime * (1 + (stintLengths[i] - nominalStintLength) * 0.01f);
                    Console.WriteLine(racePace);
                    if (racePace * stintLengths[i] > raceDurationCopy)
                    {
                        raceDurationCopy -= (int)(raceDurationCopy / racePace) * racePace;
                        
                        raceDurationEnd = raceDurationCopy;
                    }
                    else
                    {
                        raceDurationCopy -= racePace * stintLengths[i];
                        raceDurationCopy -= pitStopAddtionalTimeLoss;
                        raceDurationCopy -= 40; // refuel time
                        if (tiresRemaining >= 4)
                        {
                            tiresRemaining -= 4;
                            raceDurationCopy -= tireChangeTime[4];
                        }
                        else
                        {
                            raceDurationCopy -= tireChangeTime[tiresRemaining];
                            tiresRemaining = 0;
                        }
                    }
                }
                return (stintLengths, raceDurationEnd);
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
