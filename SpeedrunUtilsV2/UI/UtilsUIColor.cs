using UnityEngine;

namespace SpeedrunUtilsV2.UI
{
    internal static class UtilsUIColor
    {
        internal const string color_text_disconnect     = "#E74C3C";
        internal const string color_text_connect        = "#2ECC71";
        internal const string color_text_keys           = "#BDC3C7";
        internal const string color_text_red            = "#C0392B";
        internal const string color_text_fps            = "#BDC3C7";

        internal const string color_credit_ninja        = "#FA7DE5";
        internal const string color_credit_loomeh       = "#800080";
        internal const string color_credit_jomoko       = "#9FA1A4";
        internal const string color_credit_judah        = "#45BF60";

        internal static readonly Color color_text               = new Color(0.9254901960784314f, 0.9411764705882353f, 0.9450980392156862f, 1f);

        internal static readonly Color color_button_off         = new Color(0.8980392156862745f, 0.2235294117647059f, 0.20784313725490197f, 0.7f);
        internal static readonly Color color_button_off_hover   = new Color(0.8274509803921568f, 0.1843137254901961f, 0.1843137254901961f, 0.7f);
        internal static readonly Color color_button_off_active  = new Color(0.7176470588235294f, 0.10980392156862745f, 0.10980392156862745f, 0.7f);

        internal static readonly Color color_button_on          = new Color(0.2627450980392157f, 0.6274509803921569f, 0.2784313725490196f, 1f);
        internal static readonly Color color_button_on_hover    = new Color(0.2196078431372549f, 0.5568627450980392f, 0.23529411764705882f, 1f);
        internal static readonly Color color_button_on_active   = new Color(0.1803921568627451f, 0.49019607843137253f, 0.19607843137254902f, 1f);

        internal static readonly Color color_button             = new Color(0.5529411764705883f, 0.6352941176470588f, 0.7490196078431373f, 0.7f);
        internal static readonly Color color_button_hover       = new Color(0.47843137254901963f, 0.5843137254901961f, 0.6901960784313725f, 0.7f);
        internal static readonly Color color_button_active      = new Color(0.4235294117647059f, 0.5215686274509804f, 0.6313725490196078f, 0.7f);
    }
}
