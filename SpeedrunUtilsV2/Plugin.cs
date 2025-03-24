using BepInEx;
using HarmonyLib;
using Reptile;
using UnityEngine;

namespace SpeedrunUtilsV2
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    internal partial class Plugin : BaseUnityPlugin
    {
        private const string pluginGuid     = "SpeedrunUtilsV2";
        private const string pluginName     = "SpeedrunUtilsV2";
        private const string pluginVersion  = "0.1.0";

        internal static readonly LiveSplitManager liveSplitManager = new LiveSplitManager();

        public void Awake()
        {
            SetupProperties(Screen.width, Screen.height);
            Core.OnScreenSizeChanged += SetupProperties;

            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }
    }
}
