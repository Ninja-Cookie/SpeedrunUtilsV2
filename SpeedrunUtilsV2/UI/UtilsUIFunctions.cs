using BepInEx.Configuration;
using Reptile;
using SpeedrunUtilsV2.ProgressTracker;
using SpeedrunUtilsV2.UI;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static SpeedrunUtilsV2.ProgressTracker.SaveData;
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

        private static bool GUI_TRACKER_ENABLED = false;
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
        internal static WindowProperties windowPropertiesTracker;
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

        public void Start()
        {
            GUI_TRACKER_ENABLED = LiveSplitConfig.SETTINGS_Tracker.Item2;
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

            if (GetActionKeyDown(LiveSplitConfig.Actions.OpenTracker) && windowPropertiesMain != null)
                ToggleProgressTracker();
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

        private ScreenInfo CurrentScreenInfo;
        private class ScreenInfo
        {
            private const float ReferenceResolution = 1080f;

            internal ScreenInfo(int width, int height)
            {
                float newWidth  = ReferenceResolution * ((float)width / (float)height);
                float scale     = Mathf.Min((float)width / newWidth, (float)height / ReferenceResolution);

                float offsetX   = ((float)width - newWidth * scale) * 0.5f;
                float offsetY   = ((float)height - ReferenceResolution * scale) * 0.5f;

                this.Resolution = new Vector2Int((int)newWidth, (int)ReferenceResolution);
                this.Matrix     = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0), Quaternion.identity, new Vector3(scale, scale, 1f));
            }

            internal Vector2Int Resolution  { get; private set; }
            internal Matrix4x4  Matrix      { get; private set; }
        }

        public void OnGUI()
        {
            if (CurrentScreenInfo?.Matrix != null)
                GUI.matrix = CurrentScreenInfo.Matrix;

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

            if (GUI_TRACKER_ENABLED)
                GUIWindow(3, windowPropertiesTracker, GUITracker);
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
            CurrentScreenInfo = new ScreenInfo(width, height);
            width   = CurrentScreenInfo.Resolution.x;
            height  = CurrentScreenInfo.Resolution.y;

            windowPropertiesMain    = new WindowProperties((width * 0.5f) - (350f * 0.5f), 350f);
            windowPropertiesSplits  = new WindowProperties(windowPropertiesMain.WindowX + 350f + 3f, 300f, 18f);
            windowPropertiesCredits = new WindowProperties(windowPropertiesMain.WindowX - 200f - 3f, 200f, 18f);
            windowPropertiesTracker = new WindowProperties(width - 300f - 18f, 300f, 18f);
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

        internal static void ToggleProgressTracker()
        {
            GUI_TRACKER_ENABLED = !GUI_TRACKER_ENABLED;
            LiveSplitConfig.UpdateProgressTrackerState(LiveSplitConfig.SETTINGS_Tracker.Item2 = GUI_TRACKER_ENABLED);
        }
    }
}
