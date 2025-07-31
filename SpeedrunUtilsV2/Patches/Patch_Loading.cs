using HarmonyLib;
using Reptile;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_Loading : HarmonyPatch
    {
        internal delegate void Loading();
        internal static Loading OnEnteredLoading;
        internal static Loading OnExitedLoading;

        [HarmonyPatch(typeof(BaseModule), "ShowLoadingScreen", MethodType.Normal)]
        private static class Patch_BaseModule_ShowLoadingScreen
        {
            internal static void Prefix()
            {
                OnEnteredLoading?.Invoke();
            }
        }

        [HarmonyPatch(typeof(BaseModule), "HideLoadingScreen", MethodType.Normal)]
        private static class Patch_BaseModule_HideLoadingScreen
        {
            internal static void Prefix()
            {
                ProgressTracker.Tracking.CurrentSaveData?.UpdateAll();

                OnExitedLoading?.Invoke();
                if (!ConnectionManager.IsConnected)
                    ConnectionManager.StartingNewGame = false;
            }
        }

        [HarmonyPatch(typeof(Core), "OnApplicationQuit", MethodType.Normal)]
        private static class Patch_Core_OnApplicationQuit
        {
            internal static void Prefix()
            {
                OnEnteredLoading?.Invoke();
            }
        }
    }
}
