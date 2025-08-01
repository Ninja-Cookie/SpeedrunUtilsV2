using UnityEngine;
using Color = UnityEngine.Color;
using FontStyle = UnityEngine.FontStyle;
using static SpeedrunUtilsV2.UI.UtilsUIColor;

namespace SpeedrunUtilsV2.UI
{
    internal static class UtilsUI
    {
        internal static bool setup = false;

        private const string WINDOWNAME = "";

        private static GUIStyle TEXT;
        private static GUIStyle TEXT_LEFT;
        private static GUIStyle TEXT_SMALL;
        private static GUIStyle TEXT_FPS;
        private static GUIStyle TEXT_FPS_SHADOW;
        private static GUIStyle WINDOW;
        private static GUIStyle BUTTON;
        private static GUIStyle BUTTON_OFF;
        private static GUIStyle BUTTON_ON;
        private static GUIStyle FIELD;
        private static GUIStyle TOGGLE;

        internal class WindowProperties
        {
            internal float  WindowMarginW   = 20f;
            internal float  WindowMarginH   = 20f;
            internal float  WindowMarginIW  = 5f;
            internal float  WindowMarginIH  = 3f;

            internal float  WindowX;
            internal float  WindowY;
            internal float  WindowW         = 350f;
            internal float  WindowH         => FinalIndex + (WindowMarginIH * 2);

            internal float  ElementH        = 20f;
            internal float  Seperation      = 3f;

            internal float  IndexLine       = 0;
            internal float  FinalIndex      = 0;

            internal Rect   ElementRect;
            private float   Spacing         => IndexLine != 0f ? Seperation : WindowMarginIH;
            internal float  ElementY        => (IndexLine + Spacing);
            internal float  ElementW        => WindowW - (WindowMarginIW * 2);
            internal Rect   ElementRectFinal
            {
                get
                {
                    ElementRect.y       =   ElementY;
                    ElementRect.width   =   ElementW;
                    IndexLine           +=  ElementRect.height + Spacing;
                    return ElementRect;
                }
            }

            internal WindowProperties(float WindowX = 0, float WindowW = 350f, float ElementH = 20f, float WindowY = -1, Rect? customRect = null)
            {
                this.WindowX        = WindowX;
                this.WindowW        = WindowW;
                this.WindowY        = WindowY != -1 ? WindowY : WindowMarginH;
                ElementRect         = customRect ?? new Rect(WindowMarginIW, 0f, ElementW, this.ElementH = ElementH);
            }
        }

        internal static void Init()
        {
            if (setup)
                return;

            TEXT            = Setup.Text();
            TEXT_LEFT       = Setup.TextLeft();
            TEXT_SMALL      = Setup.TextSmall();
            TEXT_FPS        = Setup.TextFPS();
            TEXT_FPS_SHADOW = Setup.TextFPSShadow();
            WINDOW          = Setup.Window();
            BUTTON          = Setup.Button(Setup.ButtonType.Normal);
            BUTTON_OFF      = Setup.Button(Setup.ButtonType.Off);
            BUTTON_ON       = Setup.Button(Setup.ButtonType.On);
            FIELD           = Setup.TextField();
            TOGGLE          = Setup.Toggle();

            setup = true;
        }

        internal static class Setup
        {
            private static readonly Color TextColor         = color_text;
            private static readonly Color TextColorShadow   = new Color(0f, 0f, 0f, 0.5f);
            private static readonly Color TextColorFps      = new Color(1f, 1f, 1f, 0.8f);
            private static int FontSize = 16;

            private static readonly Texture2D Normal_BT     = new Texture2D(1, 1);
            private static readonly Texture2D Hover_BT      = new Texture2D(1, 1);
            private static readonly Texture2D Active_BT     = new Texture2D(1, 1);

            private static readonly Texture2D Normal_BToff  = new Texture2D(1, 1);
            private static readonly Texture2D Hover_BToff   = new Texture2D(1, 1);
            private static readonly Texture2D Active_BToff  = new Texture2D(1, 1);

            private static readonly Texture2D Normal_BTon   = new Texture2D(1, 1);
            private static readonly Texture2D Hover_BTon    = new Texture2D(1, 1);
            private static readonly Texture2D Active_BTon   = new Texture2D(1, 1);

            private static readonly Texture2D WindowTexture = new Texture2D(1, 1);

            internal enum ButtonType
            {
                Normal,
                Off,
                On
            }

            internal static GUIStyle Text()
            {
                GUIStyle text = new GUIStyle();

                text.alignment          = TextAnchor.MiddleCenter;
                text.wordWrap           = false;
                text.fontSize           = FontSize;
                text.fontStyle          = FontStyle.Bold;

                text.normal.textColor   = TextColor;
                text.hover.textColor    = TextColor;
                text.active.textColor   = TextColor;

                return text;
            }

            internal static GUIStyle TextLeft()
            {
                GUIStyle text = new GUIStyle();

                text.alignment          = TextAnchor.MiddleLeft;
                text.wordWrap           = false;
                text.fontSize           = (int)(FontSize * 0.9f);
                text.fontStyle          = FontStyle.Bold;

                text.normal.textColor   = TextColor;
                text.hover.textColor    = TextColor;
                text.active.textColor   = TextColor;

                return text;
            }

            internal static GUIStyle TextSmall()
            {
                GUIStyle text = new GUIStyle();

                text.alignment          = TextAnchor.MiddleCenter;
                text.wordWrap           = false;
                text.fontSize           = Mathf.RoundToInt(FontSize * 0.7f);
                text.fontStyle          = FontStyle.Bold;

                text.normal.textColor   = TextColor;
                text.hover.textColor    = TextColor;
                text.active.textColor   = TextColor;

                return text;
            }

            internal static GUIStyle TextFPS()
            {
                GUIStyle text = new GUIStyle();

                text.alignment  = TextAnchor.UpperLeft;
                text.wordWrap   = false;
                text.fontSize   = Mathf.Max(LiveSplitConfig.SETTINGS_FPSSize.Item2, 1);
                text.fontStyle  = FontStyle.Bold;

                text.normal .textColor = TextColorFps;
                text.hover  .textColor = TextColorFps;
                text.active .textColor = TextColorFps;

                return text;
            }

            internal static GUIStyle TextFPSShadow()
            {
                GUIStyle text = new GUIStyle();

                text.alignment          = TextAnchor.UpperLeft;
                text.wordWrap           = false;
                text.fontSize           = Mathf.Max(LiveSplitConfig.SETTINGS_FPSSize.Item2, 1);
                text.fontStyle          = FontStyle.Bold;

                text.normal .textColor  = TextColorShadow;
                text.hover  .textColor  = TextColorShadow;
                text.active .textColor  = TextColorShadow;

                return text;
            }

            internal static GUIStyle TextField()
            {
                GUIStyle textfield = new GUIStyle(GUI.skin.textField);

                textfield.alignment = TextAnchor.MiddleCenter;
                textfield.wordWrap  = false;
                textfield.fontSize  = Mathf.RoundToInt(FontSize * 0.8f);
                textfield.fontStyle = FontStyle.Bold;

                return textfield;
            }

            internal static GUIStyle Window()
            {
                GUIStyle window = new GUIStyle(GUI.skin.box);

                WindowTexture.SetPixel(0, 0, new Color(0.073f, 0.123f, 0.155f, 0.75f));
                WindowTexture.Apply();

                window.normal.background = WindowTexture;
                return window;
            }

            internal static GUIStyle Button(ButtonType buttonType)
            {
                GUIStyle button = new GUIStyle(GUI.skin.button);

                Texture2D normal    = Normal_BT;
                Texture2D hover     = Hover_BT;
                Texture2D active    = Active_BT;

                switch (buttonType)
                {
                    case ButtonType.Normal:
                        normal  .SetPixel(0, 0, color_button);
                        hover   .SetPixel(0, 0, color_button_hover);
                        active  .SetPixel(0, 0, color_button_active);
                    break;

                    case ButtonType.Off:
                        normal  = Normal_BToff;
                        hover   = Hover_BToff;
                        active  = Active_BToff;

                        normal  .SetPixel(0, 0, color_button_off);
                        hover   .SetPixel(0, 0, color_button_off_hover);
                        active  .SetPixel(0, 0, color_button_off_active);
                    break;

                    case ButtonType.On:
                        normal  = Normal_BTon;
                        hover   = Hover_BTon;
                        active  = Active_BTon;

                        normal  .SetPixel(0, 0, color_button_on);
                        hover   .SetPixel(0, 0, color_button_on_hover);
                        active  .SetPixel(0, 0, color_button_on_active);
                    break;
                }

                normal  .Apply();
                hover   .Apply();
                active  .Apply();

                button.normal.background    = normal;
                button.normal.textColor     = TextColor;

                button.hover.background     = hover;
                button.hover.textColor      = TextColor;

                button.active.background    = active;
                button.active.textColor     = TextColor;

                button.alignment    = TextAnchor.MiddleCenter;
                button.wordWrap     = false;
                button.fontSize     = Mathf.RoundToInt(FontSize * 0.8f);
                button.fontStyle    = FontStyle.Bold;

                return button;
            }

            internal static GUIStyle Toggle()
            {
                GUIStyle toggle = new GUIStyle(GUI.skin.toggle);

                toggle.alignment    = TextAnchor.MiddleCenter;
                toggle.wordWrap     = false;
                toggle.fontSize     = Mathf.RoundToInt(FontSize * 0.8f);
                toggle.fontStyle    = FontStyle.Bold;

                return toggle;
            }
        }

        internal static void GUIWindow(int ID, WindowProperties windowProperties, GUI.WindowFunction func)
        {
            GUI.Window(ID, new Rect(windowProperties.WindowX, windowProperties.WindowY, windowProperties.WindowW, windowProperties.WindowH), func, WINDOWNAME, WINDOW);
        }

        internal static void GUILabel(string text, WindowProperties windowProperties, bool small = false, bool fps = false)
        {
            GUI.Label(windowProperties.ElementRectFinal, text, fps ? TEXT_FPS : small ? TEXT_SMALL : TEXT);
        }

        internal static void GUILabelLeft(string text, WindowProperties windowProperties)
        {
            GUI.Label(windowProperties.ElementRectFinal, text, TEXT_LEFT);
        }

        internal static void GUILabelTracker(WindowProperties windowProperties, params string[] texts)
        {
            foreach (var text in texts)
                GUI.Label(windowProperties.ElementRectFinal, text, TEXT_LEFT);
        }

        internal static void GUILabelFPS(string text, Rect rect, Rect shadow)
        {
            GUI.Label(shadow,   text, TEXT_FPS_SHADOW);
            GUI.Label(rect,     text, TEXT_FPS);
        }

        internal static void GUIEmptySpace(float space, WindowProperties windowProperties)
        {
            windowProperties.ElementRect.height = space;
            GUI.Label(windowProperties.ElementRectFinal, "", TEXT);
            windowProperties.ElementRect.height = windowProperties.ElementH;
        }

        internal static bool GUIButton(string text, Setup.ButtonType buttonType, WindowProperties windowProperties)
        {
            GUIStyle style = BUTTON;
            switch (buttonType)
            {
                case Setup.ButtonType.Normal:   style = BUTTON;     break;
                case Setup.ButtonType.Off:      style = BUTTON_OFF; break;
                case Setup.ButtonType.On:       style = BUTTON_ON;  break;
            }

            return GUI.Button(windowProperties.ElementRectFinal, text, style);
        }

        internal static string GUIField(string text, WindowProperties windowProperties)
        {
            return GUI.TextField(windowProperties.ElementRectFinal, text, FIELD);
        }

        internal static bool GUIToggle(bool value, string text, WindowProperties windowProperties)
        {
            return GUI.Toggle(windowProperties.ElementRectFinal, value, text, TOGGLE);
        }

        internal static void Cleanup(WindowProperties windowProperties)
        {
            if (windowProperties.FinalIndex == 0f)
                windowProperties.FinalIndex = windowProperties.IndexLine;

            windowProperties.IndexLine = 0f;
        }
    }
}
