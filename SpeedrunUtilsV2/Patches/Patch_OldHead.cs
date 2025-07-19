using HarmonyLib;
using Reptile;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_OldHead : HarmonyPatch
    {
        [HarmonyPatch(typeof(SaveSlotData), "UnlockCharacter", MethodType.Normal)]
        private static class Patch_SaveSlotData_UnlockCharacter
        {
            internal static void Postfix(Characters character)
            {
                if (character == Characters.oldheadPlayer)
                    GameStatus.ShouldSplit(LiveSplitConfig.Splits.UnlockOldHead);
            }
        }
    }
}
