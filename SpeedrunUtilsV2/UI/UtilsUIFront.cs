using System.Text.RegularExpressions;
using static SpeedrunUtilsV2.UI.UtilsUI;
using UnityEngine;
using System;
using Reptile;

namespace SpeedrunUtilsV2
{
    internal partial class Plugin
    {
        private bool        GUI_ENABLED         = false;
        private static bool GUI_SPLITS_ENABLED  = false;
        private static bool GUI_CREDITS_ENABLED = false;

        private const float FPSUpdateRate       = 0.1f;
        private float       FPSUpdate           = FPSUpdateRate;
        private static int  FPS                 = 0;

        private static bool FPS_Limited         = false;
        private static int  FPS_LimitValue      => Mathf.Clamp(LiveSplitConfig.SETTINGS_LimitValue.Item2, 30, 9999);

        private static WindowProperties windowPropertiesMain;
        private static WindowProperties windowPropertiesSplits;
        private static WindowProperties windowPropertiesCredits;
        private static Rect fpsRect;
        private static Rect fpsRectShadow;

        internal static FPSStatus CurrentFPSStatus
        {
            get
            {
                if (LiveSplitConfig.SETTINGS_UncapLoading.Item2 && Core.Instance?.BaseModule != null && Core.Instance.BaseModule.IsLoading)
                    return FPSStatus.Uncapped;

                if (FPS_Limited)
                    return FPSStatus.Limited;

                if (LiveSplitConfig.SETTINGS_Uncap.Item2)
                    return FPSStatus.Uncapped;

                return FPSStatus.Capped;
            }
        }
        internal enum FPSStatus
        {
            Uncapped,
            Capped,
            Limited
        }

        public void Update()
        {
            if (LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.UncapFPS, out var action_uncap) && UnityEngine.Input.GetKeyDown(action_uncap))
                LiveSplitConfig.SETTINGS_Uncap.Item2 = !LiveSplitConfig.SETTINGS_Uncap.Item2;

            if (LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.LimitFPS, out var action_fps) && UnityEngine.Input.GetKeyDown(action_fps))
                FPS_Limited = !FPS_Limited;

            UpdateFPS();

            if (LiveSplitConfig.ActionKeys.TryGetValue(LiveSplitConfig.Actions.OpenGUI, out var action_gui) && UnityEngine.Input.GetKeyDown(action_gui))
            {
                if (windowPropertiesMain != null)
                    GUI_ENABLED = !GUI_ENABLED;

                if (!GUI_ENABLED)
                {
                    GUI_SPLITS_ENABLED  = false;
                    GUI_CREDITS_ENABLED = false;
                }
            }
        }

        private void UpdateFPS()
        {
            switch (CurrentFPSStatus)
            {
                case FPSStatus.Uncapped:
                    if (Application.targetFrameRate != -1)
                        SetFPS(-1);
                break;

                case FPSStatus.Capped:
                    if (Application.targetFrameRate != LiveSplitConfig.SETTINGS_DefaultFPS.Item2)
                        SetMaxFPS(LiveSplitConfig.SETTINGS_DefaultFPS.Item2);
                break;

                case FPSStatus.Limited:
                    if (Application.targetFrameRate != FPS_LimitValue)
                        SetFPS(LiveSplitConfig.SETTINGS_LimitValue.Item2 = FPS_LimitValue);
                break;
            }

            if (FPSUpdate <= 0f)
            {
                FPS = Mathf.RoundToInt(1.0f / Time.smoothDeltaTime);
                FPSUpdate = FPSUpdateRate;
            }

            FPSUpdate -= Time.deltaTime;
        }

        public void OnGUI()
        {
            Init();

            if (fpsRect != null && fpsRectShadow != null && LiveSplitConfig.SETTINGS_ShowFPS.Item2)
                GUILabelFPS(FPS.ToString(), fpsRect, fpsRectShadow);

            if (GUI_ENABLED)
            {
                GUIWindow(0, windowPropertiesMain, GUIFront);

                if (GUI_SPLITS_ENABLED)
                    GUIWindow(1, windowPropertiesSplits, GUISplits);

                if (GUI_CREDITS_ENABLED)
                    GUIWindow(2, windowPropertiesCredits, GUICredits);
            }
        }

        private     static int      _fpsInt { get { if (int.TryParse(_fps, out int result)) return result; return Application.targetFrameRate; } }
        private     static string   _fps = string.Empty;
        internal    static string   FPSfield
        {
            get => _fps;

            set
            {
                if (_fps == value)
                    return;

                string newValue = Regex.Replace(value, @"\D", "");
                if (newValue.Length > 4)
                    newValue = _fps;

                _fps = newValue;
            }
        }

        private void SetupProperties(int width, int height)
        {
            windowPropertiesMain    = new WindowProperties((width * 0.5f) - (350f * 0.5f), 350f);
            windowPropertiesSplits  = new WindowProperties(windowPropertiesMain.WindowX + 350f + 3f, 300f, 18f);
            windowPropertiesCredits = new WindowProperties(windowPropertiesMain.WindowX - 200f - 3f, 200f, 18f);
            fpsRect                 = new Rect(LiveSplitConfig.SETTINGS_FPSPos.Item2.x, LiveSplitConfig.SETTINGS_FPSPos.Item2.y, width, height);
            fpsRectShadow           = new Rect(LiveSplitConfig.SETTINGS_FPSPos.Item2.x - 2f, LiveSplitConfig.SETTINGS_FPSPos.Item2.y + 2f, width, height);
        }

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
            GUILabel("<color=#7FFFD4>Ninja</color> <color=#FA7DE5>Cookie</color>", windowPropertiesCredits);
            GUILabel("<color=#800080>Loomeh</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel("- Previous Research -", windowPropertiesCredits);
            GUILabel("<color=#9FA1A4>Jomoko</color>", windowPropertiesCredits);
            GUIEmptySpace(5f, windowPropertiesCredits);

            GUILabel("- Original Tools -", windowPropertiesCredits);
            GUILabel("<color=#45BF60>Judah Caruso</color>", windowPropertiesCredits);

            Cleanup(windowPropertiesCredits);
        }

        internal static void SetMaxFPS(int fps)
        {
            fps = Mathf.Clamp(fps, 30, 9999);

            if (CurrentFPSStatus != FPSStatus.Capped)
            {
                FPSfield = fps.ToString();
                LiveSplitConfig.UpdateFPSSetting(fps);
                return;
            }

            SetFPS(fps);

            LiveSplitConfig.UpdateFPSSetting(Application.targetFrameRate);
            FPSfield = Application.targetFrameRate.ToString();
        }

        private static void SetFPS(int fps)
        {
            if (QualitySettings.vSyncCount > 0)
                QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps;
        }

        internal static void ToggleLiveSplit()
        {
            if (!ConnectionManager.IsConnected && liveSplitManager.ConnectionStatus == LiveSplitManager.Status.Disconnected)
                liveSplitManager.ConnectToLiveSplit();
            else if (ConnectionManager.IsConnected && liveSplitManager.ConnectionStatus == LiveSplitManager.Status.Connected)
                liveSplitManager.DisconnectFromLiveSplit();
        }

        internal static void UpdateSplitGUI(LiveSplitConfig.Splits split)
        {
            bool value = GUIToggle(LiveSplitConfig.CurrentSplits[split], LiveSplitConfig.SplitDescriptions[split], windowPropertiesSplits);
            if (LiveSplitConfig.CurrentSplits.TryGetValue(split, out bool currentValue) && value != currentValue)
                LiveSplitConfig.UpdateSplitsFile(split, value);
        }
    }
}
