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
            { ("ch1s4",                 631),   TimeSpan.ParseExact("1:09.737", Format, null) },
            { ("ch1s5b",                270),   TimeSpan.ParseExact("1:35.315", Format, null) },
            { ("ch1s5c",                290),   TimeSpan.ParseExact("0:40.687", Format, null) },
            { ("ch1s7",                 171),   TimeSpan.ParseExact("0:28.243", Format, null) },
            { ("ch1s10",                894),   TimeSpan.ParseExact("1:26.239", Format, null) },
            { ("ch1s12",                224),   TimeSpan.ParseExact("0:36.994", Format, null) },
            { ("ch2s1",                 416),   TimeSpan.ParseExact("1:41.264", Format, null) },
            { ("Sequence_LeaveSquare",  11),    TimeSpan.ParseExact("0:08.957", Format, null) },
            { ("Sequence_Talk_1",       71),    TimeSpan.ParseExact("0:06.462", Format, null) },
            { ("ch3s1",                 216),   TimeSpan.ParseExact("0:56.397", Format, null) }
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

        internal static void DebugRunTimer(PlayableDirector sequence)
        {
            UnityEngine.Debug.Log($"Starting Timer ...");
            UnityEngine.Debug.Log($"{sequence.name} | {sequence.playableGraph.GetPlayableCount()}");
            CutsceneTimer.StartTimer();
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
                        CutsceneTimer.StopTimer();
                        return;
                    }
                }
            }
        }
    }
}