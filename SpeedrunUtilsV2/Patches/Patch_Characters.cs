using HarmonyLib;
using Reptile;
using System.Linq;
using static SpeedrunUtilsV2.LiveSplitConfig;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_Characters : HarmonyPatch
    {
        [HarmonyPatch(typeof(SaveSlotData), "UnlockCharacter", MethodType.Normal)]
        private static class Patch_SaveSlotData_UnlockCharacter
        {
            private static readonly Characters[] ExcludedCharacters = new Characters[]
            {
                Characters.girl1,               // Vinyl
                Characters.dummy,               // Solace
                Characters.metalHead,           // Red
                Characters.headManNoJetpack,    // Faux (No Jetpack)
                Characters.headMan,             // Faux
                Characters.legendMetalHead,     // Felix (Metal Head)
                Characters.legendFace,          // Felix
                Characters.blockGuy,            // Tryce
                Characters.spaceGirl            // Bel
            };

            internal static void Postfix(Characters character)
            {
                if (!ExcludedCharacters.Contains(character) && CurrentSplits.TryGetValue(Splits.CharacterUnlock, out var canSplit) && canSplit.Item1)
                    GameStatus.ShouldSplit(Splits.CharacterUnlock, true);
                else if (character == Characters.oldheadPlayer)
                    GameStatus.ShouldSplit(Splits.UnlockOldHead);

                ProgressTracker.Tracking.CurrentSaveData?.UpdateCurrentCharacters();
            }
        }
    }
}
