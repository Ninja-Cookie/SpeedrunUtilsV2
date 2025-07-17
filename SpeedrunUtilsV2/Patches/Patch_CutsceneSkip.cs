using HarmonyLib;
using Reptile;
using UnityEngine.Playables;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace SpeedrunUtilsV2.Patches
{
    internal class Patch_CutsceneSkip : HarmonyPatch
    {
        internal delegate void SkippedCutscene(TimeSpan time);
        internal static SkippedCutscene OnSkippedCutscene;

        private const string Format = @"m\:ss\.fff";
        private static readonly Dictionary<(string, int), TimeSpan> CutsceneIDs = new Dictionary<(string, int), TimeSpan>()
        {
            { ("ch1s4",                 631),   TimeSpan.ParseExact("1:09.714", Format, null) },    // Prologue Ending
            { ("ch1s5b",                270),   TimeSpan.ParseExact("1:35.298", Format, null) },    // Chapter 1 Hideout Intro
            { ("ch1s5c",                290),   TimeSpan.ParseExact("0:40.713", Format, null) },    // Chapter 1 Hideout Exit
            { ("ch1s7",                 171),   TimeSpan.ParseExact("0:28.212", Format, null) },    // Versum Frank Spray
            { ("ch1s10",                894),   TimeSpan.ParseExact("1:25.915", Format, null) },    // Versum Dream Intro
            { ("ch1s12",                224),   TimeSpan.ParseExact("0:36.930", Format, null) },    // Versum Dream Outro
            { ("ch2s1",                 416),   TimeSpan.ParseExact("1:41.202", Format, null) },    // Chapter 2 Hideout Intro
            { ("Sequence_LeaveSquare",  11),    TimeSpan.ParseExact("0:08.913", Format, null) },    // Square Talk 1
            { ("Sequence_Talk_1",       71),    TimeSpan.ParseExact("0:06.412", Format, null) },    // Square Talk 2
            { ("ch3s1",                 216),   TimeSpan.ParseExact("0:56.348", Format, null) }     // Chapter 3 Hideout Intro
        };

        [HarmonyPatch(typeof(SequenceHandler), "EnterSequenceRoutine", MethodType.Enumerator)]
        internal static class Patch_SequenceHandler_EnterSequenceRoutine
        {
            internal static void Postfix(bool __result)
            {
                if (__result)
                    return;

                FullyEnteredSequence();
            }
        }

        [HarmonyPatch(typeof(SequenceHandler), "SetInSequenceImmediate", MethodType.Normal)]
        internal static class Patch_SequenceHandler_SetInSequenceImmediate
        {
            internal static void Postfix(SequenceHandler __instance, PlayableDirector ___sequence)
            {
                EnteredSequence(__instance, ___sequence);
            }
        }

        private static void FullyEnteredSequence()
        {
            SequenceHandler sequenceHandler = SequenceHandler.instance;
            if (sequenceHandler != null)
                EnteredSequence(sequenceHandler, sequenceHandler.GetValue<PlayableDirector>("sequence"));
        }

        private static void EnteredSequence(SequenceHandler instance, PlayableDirector sequence)
        {
            if (instance?.GetValue<Enum>("skipTextActiveState")?.ToString() != "NOT_SKIPPABLE")
                return;

            if      (LiveSplitConfig.SETTINGS_DebugMode.Item2) { DebugRunTimer(sequence); return; }
            else if (LiveSplitConfig.SETTINGS_Skip.Item2 && CutsceneIDs.TryGetValue((sequence.name, sequence.playableGraph.GetPlayableCount()), out var time))
            {
                instance.ExitCurrentSequence();
                OnSkippedCutscene?.Invoke(time);
            }
        }



        // Debug -----------------------------------------------------------------------------------------------

        private static CutsceneTimer cutsceneTimer;

        internal static void DebugRunTimer(PlayableDirector sequence)
        {
            if (cutsceneTimer != null)
                return;

            cutsceneTimer = new CutsceneTimer();
            cutsceneTimer.StartTimer(sequence.name, sequence.playableGraph.GetPlayableCount());
        }

        [HarmonyPatch(typeof(SequenceHandler), "UpdateSequenceHandler", MethodType.Normal)]
        internal static class Patch_SequenceHandler_UpdateSequenceHandler
        {
            internal static void Prefix(Player ___player, Coroutine ___exitSequenceRoutine, bool ___disabledExit, PlayableDirector ___sequence, float ___fadeDuration, float ___skipTimer, float ___skipThreshold)
            {
                if (LiveSplitConfig.SETTINGS_DebugMode.Item2 && ___player.GetValue<SequenceState>("sequenceState") == SequenceState.IN_SEQUENCE && SequenceHandler.instance?.GetValue<Enum>("skipTextActiveState").ToString() == "NOT_SKIPPABLE")
                {
                    if (___exitSequenceRoutine == null && !___disabledExit && (___sequence.time >= ___sequence.duration - (double)___fadeDuration || ___skipTimer >= ___skipThreshold))
                    {
                        cutsceneTimer.StopTimer();
                        cutsceneTimer = null;
                        return;
                    }
                }
            }
        }
    }
}