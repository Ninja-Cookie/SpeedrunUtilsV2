using Reptile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpeedrunUtilsV2
{
    internal class CutsceneTimer
    {
        private class TimerInfo
        {
            internal TimeSpan StartingTime;
            internal TimeSpan EndingTime;
            internal TimeSpan TotalDelay;

            internal string name;
            internal int playable;

            internal TimerInfo(TimeSpan startingTime, string name, int playable)
            {
                this.StartingTime   = startingTime;
                this.name           = name;
                this.playable       = playable;
            }
        }

        private TimerInfo ActiveTimer;

        internal async void StartTimer(string name, int playable)
        {
            UnityEngine.Debug.Log("Starting Cutscene Timer...");
            UnityEngine.Debug.Log($"-- {name} | {playable} --");

            Stopwatch responseTimer = Stopwatch.StartNew();
            ActiveTimer = new TimerInfo(await ConnectionManager.StartGettingGameTime(), name, playable);
            responseTimer.Stop();

            ActiveTimer.TotalDelay = responseTimer.Elapsed;
        }

        internal async void StopTimer()
        {
            if (ActiveTimer == null)
                return;

            Stopwatch responseTimer = Stopwatch.StartNew();
            ActiveTimer.EndingTime = await ConnectionManager.StartGettingGameTime();
            responseTimer.Stop();

            ActiveTimer.TotalDelay += responseTimer.Elapsed;

            UnityEngine.Debug.Log($"{ActiveTimer.name} | {ActiveTimer.playable}");
            UnityEngine.Debug.Log($"----------------------------");
            UnityEngine.Debug.Log($"Final Time:     {(ActiveTimer.EndingTime - ActiveTimer.StartingTime) - ActiveTimer.TotalDelay}");
            UnityEngine.Debug.Log($"Total Delay:    {ActiveTimer.TotalDelay} (Removed from Final Time)");
            UnityEngine.Debug.Log($"----------------------------");
        }

        /*
        private static TimeSpan StartingTime;
        private static TimeSpan EndingTime;
        private static TimeSpan TotalDelay;
        internal static bool TimerStarted = false;
        internal async static void StartTimer(string name, int playable)
        {
            UnityEngine.Debug.Log($"Starting Timer ...");
            UnityEngine.Debug.Log($"{name} | {playable}");

            // Get this different, reset it properly and stuff

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
        */
    }
}
