
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
                float raceDurationCopy = raceDuration;
                float raceDurationEnd = 0;
                stintLengths.Clear();

                Console.WriteLine(raceDurationCopy);

                while (raceDurationCopy > 0)
                {
                    VE -= virtualEnergyUnitsPerLap;
                    raceDurationCopy -= LapTime;
                    currentStintLength++;

                    Console.WriteLine(raceDurationCopy);

                    if (VE < 0.95f * virtualEnergyUnitsPerLap && raceDurationCopy > 0)
                    {
                        VE = Math.Max(0, VE);
                        Console.WriteLine($"Pit VE: {VE}");
                        raceDurationCopy -= pitStopAddtionalTimeLoss;

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

                        VERequired = (int)(raceDurationCopy / LapTime) < pushStintLength
                            ? (int)(raceDurationCopy / LapTime) * virtualEnergyUnitsPerLap + virtualEnergyUnitsPerLap
                            : 100;
                        raceDurationCopy -= (VERequired - VE) * secondsPerVirtualEnergyUnit;

                        Console.WriteLine(raceDurationCopy);

                        VE = VERequired;

                        stintLengths.Add(currentStintLength);
                        currentStintLength = 0;
                    }

                    if (raceDurationCopy < LapTime && raceDurationCopy > 0)
                    {
                        stintLengths.Add(currentStintLength + 1);
                        raceDurationEnd = raceDurationCopy;
                        raceDurationCopy = 0;
                    }
                }
                return (stintLengths, raceDurationEnd);
            });
        }

        public async Task<(List<int>, float)> OptimiseAsync()
        {
            return await Task.Run(() =>
            {
                /* if (stintLengths[^1] < stintLengths.Count - 1)
                {
                    for (int i = 0; i <= stintLengths[^1]; i++)
                    {
                        stintLengths[i] = stintLengths[i] + 1;
                    }
                    stintLengths.RemoveAt(stintLengths.Count - 1);
                } */

                float VE = 100;
                int tiresRemaining = AllowedTires((int)raceDuration / 3600) - 4;
                float VERequired = 0;
                int pushStintLength = (int)(100 / virtualEnergyUnitsPerLap);
                int currentStintNumber = 0;
                int currentStintLength = 0;
                float raceDurationCopy = raceDuration;
                float raceDurationEnd = 0;
                float virtualEnergyUsedPerLapOptimised = 0;
                List<int> stintLengthsShadow = new List<int>();
                Console.WriteLine(raceDurationCopy);
                while (raceDurationCopy > 0)
                {

                    virtualEnergyUsedPerLapOptimised = 100f / stintLengths[currentStintNumber];
                    float VEMin = Math.Min(virtualEnergyUnitsPerLap, virtualEnergyUsedPerLapOptimised);
                    VE -= VEMin;

                    Console.WriteLine(VEMin);

                    float newLapTime = LapTime * (1 + Math.Max(0, stintLengths[currentStintNumber] - pushStintLength) * 0.008f);
                    raceDurationCopy -= newLapTime;
                    currentStintLength++;

                    Console.WriteLine(raceDurationCopy);

                    double VEDiff = Math.Round(VE - VEMin, 3);
                    if (VEDiff <= -0.05f * VEMin && raceDurationCopy > 0)
                    {
                        VE = Math.Max(0, VE);
                        Console.WriteLine($"Pit VE: {VE}, Min VE: {VEMin}");
                        raceDurationCopy -= pitStopAddtionalTimeLoss;

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

                        VERequired = (int)(raceDurationCopy / LapTime) < pushStintLength
                            ? (int)(raceDurationCopy / LapTime) * virtualEnergyUnitsPerLap + virtualEnergyUnitsPerLap
                            : 100;
                        raceDurationCopy -= (VERequired - VE) * secondsPerVirtualEnergyUnit;

                        Console.WriteLine(raceDurationCopy);
                        VE = VERequired;
                        stintLengthsShadow.Add(currentStintLength);
                        currentStintLength = 0;
                        currentStintNumber++;
                    }

                    if (raceDurationCopy < LapTime && raceDurationCopy > 0)
                    {
                        raceDurationEnd = raceDurationCopy;
                        stintLengths[currentStintNumber] = currentStintLength + 1;
                        stintLengthsShadow.Add(currentStintLength + 1);
                        raceDurationCopy = 0;
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
                return 38;
            }
        }
    }
}
