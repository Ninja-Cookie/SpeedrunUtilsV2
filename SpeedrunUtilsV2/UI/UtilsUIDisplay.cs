using System;
using UnityEngine;
using static SpeedrunUtilsV2.UI.UtilsUI;

namespace SpeedrunUtilsV2
{
    internal partial class Plugin
    {
        internal static void GUIFront(int windowID)
        {
            string fpsHeader = CurrentFPSStatus == FPSStatus.Uncapped ? "<color=red>Uncapped</color>" : $"<color=silver>{Application.targetFrameRate}</color>";
            GUILabel($"({fpsHeader}) FPS: {FPS}", windowPropertiesMain);

            if (LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.UncapFPS, out var action_uncap) && LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.LimitFPS, out var action_fps))
                GUILabel($"Uncap Framerate: <color=orange>{action_uncap}</color> | Limit Framerate to {LiveSplitConfig.SETTINGS_LimitValue.Item2}: <color=orange>{action_fps}</color>", windowPropertiesMain, true);

            GUILabel(ConnectionManager.IsConnected ? "<color=lime>Connected to LiveSplit!</color>" : "<color=red>Disconnected from LiveSplit.</color>", windowPropertiesMain);

            if (GUIButton(ConnectionManager.IsConnected ? "Disconnect from LiveSplit" : "Connect to LiveSplit", ConnectionManager.IsConnected ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                ToggleLiveSplit();

            if (GUIButton("Splits", GUI_SPLITS_ENABLED ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                GUI_SPLITS_ENABLED = !GUI_SPLITS_ENABLED;

            if (GUIButton("Set Max FPS", Setup.ButtonType.Normal, windowPropertiesMain))
                SetMaxFPS(_fpsInt);

            FPSfield = GUIField(FPSfield, windowPropertiesMain);

            GUIEmptySpace(8f, windowPropertiesMain);

            if (GUIButton("Credits", GUI_CREDITS_ENABLED ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                GUI_CREDITS_ENABLED = !GUI_CREDITS_ENABLED;

            Cleanup(windowPropertiesMain);
        }

        internal static void GUISplits(int windowID)
        {
            foreach (var split in Enum.GetValues(typeof(LiveSplitConfig.Splits)))
                UpdateSplitGUI((LiveSplitConfig.Splits)split);

            Cleanup(windowPropertiesSplits);
        }

        internal static void GUICredits(int windowID)
        {
            GUILabel("- Development -", windowPropertiesCredits);
            GUILabel("<color=#FA7DE5>Ninja Cookie</color>", windowPropertiesCredits);
            GUILabel("<color=#800080>Loomeh</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel("- Previous Research -", windowPropertiesCredits);
            GUILabel("<color=#9FA1A4>Jomoko</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel("- Original Tools -", windowPropertiesCredits);
            GUILabel("<color=#45BF60>Judah Caruso</color>", windowPropertiesCredits);

            Cleanup(windowPropertiesCredits);
        }
    }
}
