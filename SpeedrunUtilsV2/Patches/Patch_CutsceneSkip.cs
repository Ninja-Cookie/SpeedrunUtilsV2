using HarmonyLib;
using Reptile;
using UnityEngine.Playables;
using System.Collections.Generic;
using System;

namespace SpeedrunUtilsV2.Patches
{
    internal class Patch_CutsceneSkip : HarmonyPatch
    {
        internal delegate void SkippedCutscene(TimeSpan time);
        internal static SkippedCutscene OnSkippedCutscene;

        [HarmonyPatch(typeof(SequenceHandler), "SetFullyInSequence", MethodType.Normal)]
        internal static class Patch_SequenceHandler_SetFullyInSequence
        {
            private const string Format = @"m\:ss\.fff";
            private static readonly Dictionary<(string, int), TimeSpan> CutsceneIDs = new Dictionary<(string, int), TimeSpan>()
            {
                { ("ch1s4",                 641200198),     TimeSpan.ParseExact("1:09.944", Format, null) },
                { ("ch1s5b",                72321221),      TimeSpan.ParseExact("1:35.334", Format, null) },
                { ("ch1s5c",                -1364565889),   TimeSpan.ParseExact("0:40.713", Format, null) },
                { ("ch1s10",                -1652014990),   TimeSpan.ParseExact("1:26.200", Format, null) },
                { ("ch1s12",                577310854),     TimeSpan.ParseExact("0:37.204", Format, null) },
                { ("ch2s1",                 925761599),     TimeSpan.ParseExact("1:41.443", Format, null) },
                { ("Sequence_LeaveSquare",  1644233353),    TimeSpan.ParseExact("0:09.141", Format, null) },
                { ("Sequence_Talk_1",       72877330),      TimeSpan.ParseExact("0:06.669", Format, null) },
                { ("ch3s1",                 -1078755188),   TimeSpan.ParseExact("0:56.600", Format, null) },
            };

            private static void Postfix(SequenceHandler __instance, PlayableDirector ___sequence)
            {
                if (LiveSplitConfig.SETTINGS_Skip.Item2 && CutsceneIDs.TryGetValue((___sequence.name, ___sequence.duration.GetHashCode()), out var time))
                {
                    __instance.ExitCurrentSequence();
                    OnSkippedCutscene?.Invoke(time);
                }
            }
        }
    }
}