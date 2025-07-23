using Reptile;
using System.Text.RegularExpressions;
using UnityEngine;
using static SpeedrunUtilsV2.UI.UtilsUI;

namespace SpeedrunUtilsV2
{
    internal partial class Plugin
    {
        private bool _GUI_ENABLED        = false;
        private bool GUI_ENABLED
        {
            get => _GUI_ENABLED;
            set
            {
                _GUI_ENABLED = value;
                if (!GUI_ENABLED)
                {
                    GUI_SPLITS_ENABLED  = false;
                    GUI_CREDITS_ENABLED = false;
                }
            }
        }


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
            if (GetActionKeyDown(LiveSplitConfig.Actions.UncapFPS))
                LiveSplitConfig.SETTINGS_Uncap.Item2 = !LiveSplitConfig.SETTINGS_Uncap.Item2;

            if (GetActionKeyDown(LiveSplitConfig.Actions.LimitFPS))
                FPS_Limited = !FPS_Limited;

            UpdateFPS();

            if (GetActionKeyDown(LiveSplitConfig.Actions.OpenGUI) && windowPropertiesMain != null)
                GUI_ENABLED = !GUI_ENABLED;
        }

        private bool GetActionKeyDown(LiveSplitConfig.Actions action)
        {
            return LiveSplitConfig.ActionKeys.TryGetValue(action, out var actionKeyCode) && UnityEngine.Input.GetKeyDown(actionKeyCode);
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
            bool value = GUIToggle(LiveSplitConfig.CurrentSplits[split].Item1, LiveSplitConfig.CurrentSplits[split].Item3, windowPropertiesSplits);
            if (LiveSplitConfig.CurrentSplits.TryGetValue(split, out var currentValue) && value != currentValue.Item1)
                LiveSplitConfig.UpdateSplitsFile(split, value);
        }
    }
}
