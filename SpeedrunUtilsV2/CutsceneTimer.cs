using Reptile;
using System;
using System.Diagnostics;

namespace SpeedrunUtilsV2
{
    internal static class CutsceneTimer
    {
        private static TimeSpan StartingTime;
        private static TimeSpan EndingTime;
        private static TimeSpan TotalDelay;
        internal static bool TimerStarted = false;
        internal async static void StartTimer()
        {
            TimerStarted = true;
            Stopwatch responseTimer = new Stopwatch();
            responseTimer.Start();
            StartingTime = await ConnectionManager.StartGettingGameTime();
            responseTimer.Stop();
            TotalDelay = TimeSpan.FromMilliseconds(responseTimer.ElapsedMilliseconds);
        }

        internal async static void StopTimer()
        {
            if (!TimerStarted)
                return;

            TimerStarted = false;

            Stopwatch responseTimer = new Stopwatch();
            responseTimer.Start();
            EndingTime = await ConnectionManager.StartGettingGameTime();
            responseTimer.Stop();
            TotalDelay += TimeSpan.FromMilliseconds(responseTimer.ElapsedMilliseconds);

            UnityEngine.Debug.Log($"----------------------------");
            UnityEngine.Debug.Log($"Final Time:     {(EndingTime - StartingTime) - TotalDelay}");
            UnityEngine.Debug.Log($"Total Delay:    {TotalDelay} (Removed from Final Time)");
            UnityEngine.Debug.Log($"----------------------------");
        }
    }
}
