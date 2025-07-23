using System;
using UnityEngine;
using static SpeedrunUtilsV2.UI.UtilsUI;
using static SpeedrunUtilsV2.Localization.UI;

namespace SpeedrunUtilsV2
{
    internal partial class Plugin
    {
        internal static void GUIFront(int windowID)
        {
            string fpsHeader = CurrentFPSStatus == FPSStatus.Uncapped ? $"<color=red>{s_uncapped}</color>" : $"<color=silver>{Application.targetFrameRate}</color>";
            GUILabel($"({fpsHeader}) FPS: {FPS}", windowPropertiesMain);

            if (LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.UncapFPS, out var action_uncap) && LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.LimitFPS, out var action_fps))
                GUILabel($"{s_uncappedFramerate}: <color=orange>{action_uncap}</color> | {s_limitFramerateTo} {LiveSplitConfig.SETTINGS_LimitValue.Item2}: <color=orange>{action_fps}</color>", windowPropertiesMain, true);

            GUILabel(ConnectionManager.IsConnected ? $"<color=lime>{s_connected}</color>" : $"<color=red>{s_disconnected}</color>", windowPropertiesMain);

            if (GUIButton(ConnectionManager.IsConnected ? $"{s_disconnect}" : $"{s_connect}", ConnectionManager.IsConnected ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                ToggleLiveSplit();

            if (GUIButton($"{s_splits}", GUI_SPLITS_ENABLED ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                GUI_SPLITS_ENABLED = !GUI_SPLITS_ENABLED;

            if (GUIButton($"{s_setMax}", Setup.ButtonType.Normal, windowPropertiesMain))
                SetMaxFPS(_fpsInt);

            FPSfield = GUIField(FPSfield, windowPropertiesMain);

            GUIEmptySpace(8f, windowPropertiesMain);

            if (GUIButton($"{s_credits}", GUI_CREDITS_ENABLED ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
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
            GUILabel($"- {s_development} -", windowPropertiesCredits);
            GUILabel("<color=#FA7DE5>Ninja Cookie</color>", windowPropertiesCredits);
            GUILabel("<color=#800080>Loomeh</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel($"- {s_research} -", windowPropertiesCredits);
            GUILabel("<color=#9FA1A4>Jomoko</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel($"- {s_originalTools} -", windowPropertiesCredits);
            GUILabel("<color=#45BF60>Judah Caruso</color>", windowPropertiesCredits);

            Cleanup(windowPropertiesCredits);
        }
    }
}
