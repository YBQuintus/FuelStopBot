using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelStopBot.FuelStopService
{
    internal class FuelStopService
    {
        private static readonly float secondsPerVirtualEnergyUnit = 0.4f;
        private static float virtualEnergyUnitsPerLap = 0;
        private static float raceDuration = 0;
        private static float lapTime = 0;
        private static int pitStopAddtionalTimeLoss = 25;
        private readonly static Dictionary<int, int> tireChangeTime = new Dictionary<int, int>()
        {
            { 0, 0 },
            { 1, 5 },
            { 2, 5 },
            { 3, 12 },
            { 4, 12 }
        };
        private static Dictionary<int, int> stintAndStintLength = new Dictionary<int, int>();
        public static void Main()
        {
            Console.WriteLine("Race duration? (H:MM)");
            string raceDurationInput = Console.ReadLine()!;
            string[] raceDurationParts = raceDurationInput.Split(':');
            raceDuration = (float)(TimeSpan.FromHours(int.Parse(raceDurationParts[0])) + TimeSpan.FromMinutes(int.Parse(raceDurationParts[1]))).TotalSeconds;
            Console.WriteLine("Lap time? (M:SS.sss)");
            string lapTimeInput = Console.ReadLine()!;
            string[] lapTimeParts = lapTimeInput.Split(':');
            lapTime = (float)(TimeSpan.FromMinutes(int.Parse(lapTimeParts[0])) + TimeSpan.FromSeconds(float.Parse(lapTimeParts[1]))).TotalSeconds;
            Console.WriteLine("VE per lap? (%)");
            virtualEnergyUnitsPerLap = float.Parse(Console.ReadLine()!);
            FullPush();
        }

        static void FullPush()
        {
            float VE = 100;
            int tiresRemaining = AllowedTires((int)raceDuration / 3600) - 4;
            float VERequired = 0;
            int pushStintLength = (int)(100 / virtualEnergyUnitsPerLap);
            int currentStintLength = 0;
            int currentStintNumber = 1;
            while (raceDuration > 0)
            {
                VE -= virtualEnergyUnitsPerLap;
                raceDuration -= lapTime;
                currentStintLength++;
                if (VE < virtualEnergyUnitsPerLap && raceDuration > 0)
                {
                    // Pit stop
                    raceDuration -= pitStopAddtionalTimeLoss;
                    VERequired = ((int)raceDuration / lapTime) < pushStintLength ? (int)raceDuration / (int)lapTime * virtualEnergyUnitsPerLap + virtualEnergyUnitsPerLap : 100;
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
                    currentStintLength = 0;
                    currentStintNumber++;
                }
                if (raceDuration < lapTime && raceDuration > 0)
                {
                    Console.WriteLine($"{raceDuration} seconds remaining on last lap");
                }
            }
            Console.WriteLine($"Stint {currentStintNumber} was {currentStintLength} laps.");
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
