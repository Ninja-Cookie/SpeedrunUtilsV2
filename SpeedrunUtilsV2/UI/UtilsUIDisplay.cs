using System;
using UnityEngine;
using static SpeedrunUtilsV2.UI.UtilsUI;
using static SpeedrunUtilsV2.Localization.UI;
using static SpeedrunUtilsV2.UI.UtilsUIColor;

namespace SpeedrunUtilsV2
{
    internal partial class Plugin
    {
        internal static void GUIFront(int windowID)
        {
            string fpsHeader = CurrentFPSStatus == FPSStatus.Uncapped ? $"<color={color_text_red}>{s_uncapped}</color>" : $"<color={color_text_fps}>{Application.targetFrameRate}</color>";
            GUILabel($"({fpsHeader}) FPS: {FPS}", windowPropertiesMain);

            if (LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.UncapFPS, out var action_uncap) && LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.LimitFPS, out var action_fps))
                GUILabel($"{s_uncappedFramerate}: <color={color_text_keys}>{action_uncap}</color> | {s_limitFramerateTo} {LiveSplitConfig.SETTINGS_LimitValue.Item2}: <color={color_text_keys}>{action_fps}</color>", windowPropertiesMain, true);

            GUILabel(ConnectionManager.IsConnected ? $"<color={color_text_connect}>{s_connected}</color>" : $"<color={color_text_disconnect}>{s_disconnected}</color>", windowPropertiesMain);

            if (GUIButton(ConnectionManager.IsConnected ? $"{s_disconnect}" : $"{s_connect}", ConnectionManager.IsConnected ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                ToggleLiveSplit();

            if (GUIButton($"{s_splits}", GUI_SPLITS_ENABLED ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                GUI_SPLITS_ENABLED = !GUI_SPLITS_ENABLED;

            if (GUIButton($"{s_credits}", GUI_CREDITS_ENABLED ? Setup.ButtonType.On : Setup.ButtonType.Off, windowPropertiesMain))
                GUI_CREDITS_ENABLED = !GUI_CREDITS_ENABLED;

            GUIEmptySpace(8f, windowPropertiesMain);

            if (GUIButton($"{s_setMax}", Setup.ButtonType.Normal, windowPropertiesMain))
                SetMaxFPS(_fpsInt);

            FPSfield = GUIField(FPSfield, windowPropertiesMain);

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
            GUILabel($"<color={color_credit_ninja}>Ninja Cookie</color>", windowPropertiesCredits);
            GUILabel($"<color={color_credit_loomeh}>Loomeh</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel($"- {s_research} -", windowPropertiesCredits);
            GUILabel($"<color={color_credit_jomoko}>Jomoko</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel($"- {s_originalTools} -", windowPropertiesCredits);
            GUILabel($"<color={color_credit_judah}>Judah Caruso</color>", windowPropertiesCredits);

            Cleanup(windowPropertiesCredits);
        }
    }
}
