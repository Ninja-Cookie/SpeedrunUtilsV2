using HarmonyLib;
using Reptile;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_NewGame : HarmonyPatch
    {
        [HarmonyPatch(typeof(BaseModule), "StartNewGame", MethodType.Normal)]
        private static class Patch_BaseModule_StartNewGame
        {
            internal static void Prefix()
            {
                SaveSlotData saveSlotData = Core.Instance?.SaveManager?.CurrentSaveSlot;
                if (saveSlotData != null)
                    GameStatus.SetupNewGame(saveSlotData);
            }
        }
    }
}
