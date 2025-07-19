using HarmonyLib;
using Reptile;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_FinalBoss : HarmonyPatch
    {
        [HarmonyPatch(typeof(SnakeBossNEW), "CheckIfDied", MethodType.Normal)]
        private static class Patch_SnakeBossNEW_CheckIfDiedn
        {
            internal static void Prefix(int ___HP)
            {
                if (___HP <= 0)
                    GameStatus.ShouldSplit(LiveSplitConfig.Splits.FinalBossDefeated);
            }
        }
    }
}
