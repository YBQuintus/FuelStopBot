using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public List<int> FullPush()
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
                    // Pit stop
                    raceDuration -= pitStopAddtionalTimeLoss;
                    VERequired = (int)raceDuration /   LapTime < pushStintLength ? (int)raceDuration / (int)LapTime * virtualEnergyUnitsPerLap + virtualEnergyUnitsPerLap : 100;
                    raceDuration -= (VERequired - VE) * secondsPerVirtualEnergyUnit;
                    Console.WriteLine($"Stint {currentStintNumber}: Pit stop after {currentStintLength} laps, {VERequired - VE} VE added, {TimeSpan.FromSeconds(raceDuration)} remaining.");
                    VE = VERequired;
                    if (tiresRemaining >= 4)
                    {
                        tiresRemaining -= 4;
                        raceDuration -= tireChangeTime[4];
                        Console.WriteLine($"4 tires changed, {tiresRemaining} tires remaining.");
                    }
                    else
                    {
                        raceDuration -= tireChangeTime[tiresRemaining];
                        Console.WriteLine($"{tiresRemaining} tires changed, 0 tires remaining.");
                        tiresRemaining = 0;
                    }
                    stintAndStintLength.Add(currentStintLength);
                    currentStintLength = 0;
                    currentStintNumber++;
                }
                if (raceDuration < LapTime && raceDuration > 0)
                {
                    Console.WriteLine($"{raceDuration} seconds remaining on last lap");
                }
            }
            Console.WriteLine($"Stint {currentStintNumber} was {currentStintLength} laps.");
            return stintAndStintLength;
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
