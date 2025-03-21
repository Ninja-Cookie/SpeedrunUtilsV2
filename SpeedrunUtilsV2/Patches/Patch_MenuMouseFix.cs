using HarmonyLib;
using Reptile;
using UnityEngine;

namespace SpeedrunUtilsV2.Patches
{
    internal class Patch_MenuMouseFix : HarmonyPatch
    {
        [HarmonyPatch(typeof(TextMeshProMenuButton), "OnPointerEnter", MethodType.Normal)]
        public static class Patch_TextMeshProMenuButton_OnPointerEnter
        {
            public static bool Prefix()
            {
                if (LiveSplitConfig.SETTINGS_MouseFix.Item2)
                    return Cursor.visible;
                return true;
            }
        }

        [HarmonyPatch(typeof(TextMeshProMenuButton), "OnPointerExit", MethodType.Normal)]
        public static class Patch_TextMeshProMenuButton_OnPointerExit
        {
            public static bool Prefix()
            {
                if (LiveSplitConfig.SETTINGS_MouseFix.Item2)
                    return Cursor.visible;
                return true;
            }
        }
    }
}
