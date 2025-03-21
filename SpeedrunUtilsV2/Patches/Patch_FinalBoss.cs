using HarmonyLib;
using Reptile;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_FinalBoss : HarmonyPatch
    {
        internal delegate void FinalBossDead();
        internal static FinalBossDead OnFinalBossDead;

        [HarmonyPatch(typeof(SnakeBossNEW), "CheckIfDied", MethodType.Normal)]
        private static class Patch_SnakeBossNEW_CheckIfDiedn
        {
            internal static void Prefix(int ___HP)
            {
                if (___HP <= 0)
                    OnFinalBossDead?.Invoke();
            }
        }
    }
}
