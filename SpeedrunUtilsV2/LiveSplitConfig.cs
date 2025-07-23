using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static SpeedrunUtilsV2.Localization.Splits;

namespace SpeedrunUtilsV2
{
    internal static class LiveSplitConfig
    {
        internal readonly static Dictionary<Splits, (bool, bool, string)> CurrentSplits = new Dictionary<Splits, (bool, bool, string)>()
        {
            { Splits.PrologueEnd,       (false, true,   $"{s_PrologueEnd}")         },
            { Splits.EarlySquare,       (false, true,   $"{s_EarlySquare}")         },
            { Splits.VersumStart,       (false, true,   $"{s_VersumStart}")         },
            { Splits.Dream1Start,       (false, true,   $"{s_Dream1Start}")         },
            { Splits.Chapter1End,       (false, true,   $"{s_Chapter1End}")         },

            { Splits.BrinkStart,        (false, true,   $"{s_BrinkStart}")          },
            { Splits.Dream2Start,       (false, true,   $"{s_Dream2Start}")         },
            { Splits.Chapter2End,       (false, true,   $"{s_Chapter2End}")         },

            { Splits.MallStart,         (false, true,   $"{s_MallStart}")           },
            { Splits.Dream3Start,       (false, true,   $"{s_Dream3Start}")         },
            { Splits.Chapter3End,       (false, true,   $"{s_Chapter3End}")         },

            { Splits.PrinceVersumEnd,   (false, false,  $"{s_PrinceVersumEnd}")     },
            { Splits.PrinceSquareEnd,   (false, false,  $"{s_PrinceSquareEnd}")     },
            { Splits.PrinceBrinkEnd,    (false, false,  $"{s_PrinceBrinkEnd}")      },

            { Splits.PyramidStart,      (false, true,   $"{s_PyramidStart}")        },
            { Splits.Dream4Start,       (false, true,   $"{s_Dream4Start}")         },
            { Splits.Chapter4End,       (false, true,   $"{s_Chapter4End}")         },

            { Splits.Dream5Start,       (false, false,  $"{s_Dream5Start}")         },

            { Splits.UnlockOldHead,     (false, false,  $"{s_UnlockOldHead}")       },
            { Splits.CharacterUnlock,   (false, false,  $"{s_CharacterUnlock}")     },
            { Splits.FinalBossDefeated, (false, true,   $"{s_FinalBossDefeated}")   }
        };
        internal readonly static Dictionary<Splits, bool> CurrentSplitStates = new Dictionary<Splits, bool>();

        internal enum Splits
        {
            PrologueEnd,
            EarlySquare,
            VersumStart,
            Dream1Start,
            Chapter1End,

            BrinkStart,
            Dream2Start,
            Chapter2End,

            MallStart,
            Dream3Start,
            Chapter3End,

            PrinceVersumEnd,
            PrinceSquareEnd,
            PrinceBrinkEnd,

            PyramidStart,
            Dream4Start,
            Chapter4End,

            Dream5Start,
            UnlockOldHead,
            CharacterUnlock,

            FinalBossDefeated
        }

        internal enum Actions
        {
            OpenGUI,
            LimitFPS,
            UncapFPS
        }

        internal static readonly Dictionary<Actions, KeyCode> ActionKeys = new Dictionary<Actions, KeyCode>()
        {
            { Actions.OpenGUI,      KeyCode.F2  },
            { Actions.LimitFPS,     KeyCode.L   },
            { Actions.UncapFPS,     KeyCode.O   }
        };

        internal static readonly Dictionary<Actions, KeyCode> ActionKeysDefault = new Dictionary<Actions, KeyCode>()
        {
            { Actions.OpenGUI,      KeyCode.F2  },
            { Actions.LimitFPS,     KeyCode.L   },
            { Actions.UncapFPS,     KeyCode.O   }
        };

        internal static (string, int, int)          SETTINGS_DefaultFPS     = ("Default FPS",                   200,    200     );
        internal static (string, int, int)          SETTINGS_LimitValue     = ("Limit FPS Value",               30,     30      );
        internal static (string, bool,bool)         SETTINGS_Uncap          = ("Start Uncapped",                false,  false   );
        internal static (string, bool,bool)         SETTINGS_Skip           = ("Skip Unskippable Cutscenes",    true,   true    );
        internal static (string, bool,bool)         SETTINGS_MashEnabled    = ("AutoMash Unskippable Cutscenes",true,   true    );
        internal static (string, bool,bool)         SETTINGS_ShowFPS        = ("Show FPS",                      true,   true    );
        internal static (string, int, int)          SETTINGS_FPSSize        = ("FPS Size",                      33,     33      );
        internal static (string, Vector2, Vector2)  SETTINGS_FPSPos         = ("FPS Screen Position",           new Vector2(8f, 4f), new Vector2(8f, 4f));
        internal static (string, bool, bool)        SETTINGS_UncapLoading   = ("Uncap FPS During Loading",      true, true);
        internal static (string, bool, bool)        SETTINGS_MouseFix       = ("Enable Fix to Menu Mouse",      true, true);
        internal static (string, bool, bool)        SETTINGS_DebugMode      = ("Debug Mode",                    false, false);

        private const   string          EXTENSION           = ".cfg";
        private const   string          FOLDER_UtilsFolder  = "SpeedrunUtilsV2";
        private static  readonly string FILE_Splits         = $"Splits{EXTENSION}";
        private static  readonly string FILE_Keys           = $"Keys{EXTENSION}";
        private static  readonly string FILE_Settings       = $"Settings{EXTENSION}";
        private static  readonly string PATH_Config         = BepInEx.Paths.ConfigPath;
        private static  readonly string PATH_UtilsConfig    = Path.Combine(PATH_Config,         FOLDER_UtilsFolder);
        private static  readonly string PATH_Splits         = Path.Combine(PATH_UtilsConfig,    FILE_Splits);
        private static  readonly string PATH_Keys           = Path.Combine(PATH_UtilsConfig,    FILE_Keys);
        private static  readonly string PATH_Settings       = Path.Combine(PATH_UtilsConfig,    FILE_Settings);

        private const string SECTION_Splits     = "Splits";
        private const string SECTION_Keys       = "Keys";
        private const string SECTION_Settings   = "Settings";

        private static  readonly ConfigFile CONFIG_Splits   = new ConfigFile(PATH_Splits,   true);
        private static  readonly ConfigFile CONFIG_Keys     = new ConfigFile(PATH_Keys,     true);
        private static  readonly ConfigFile CONFIG_Settings = new ConfigFile(PATH_Settings, true);

        internal static void ManageConfigFiles()
        {
            CreateSplitsFile();
            CreateKeysFile();
            CreateSettingsFile();
            SetDefaultSplitStates();
        }

        internal static void SetDefaultSplitStates()
        {
            CurrentSplitStates.Clear();
            foreach (var split in Enum.GetValues(typeof(Splits)))
                CurrentSplitStates.Add((Splits)split, false);
        }

        internal static void RefreshSplitsFile()
        {
            CreateSplitsFile();
        }

        internal static void UpdateSplitsFile(Splits split, bool value)
        {
            if (CurrentSplits.ContainsKey(split))
            {
                CurrentSplits[split] = (value, CurrentSplits[split].Item2, CurrentSplits[split].Item3);
                if (CONFIG_Splits.TryGetEntry(SECTION_Splits, CurrentSplits[split].Item3, out ConfigEntry<bool> entry))
                    entry.Value = value;
            }
        }

        private static void CreateSplitsFile()
        {
            for (int i = 0; i < CurrentSplits.Count; i++)
            {
                var key = CurrentSplits.ElementAt(i).Key;
                if (CurrentSplits.TryGetValue(key, out var value))
                {
                    CONFIG_Splits.Bind(SECTION_Splits, value.Item3, value.Item2);
                    if (CONFIG_Splits.TryGetEntry(SECTION_Splits, value.Item3, out ConfigEntry<bool> entry))
                        CurrentSplits[key] = (entry.Value, value.Item2, value.Item3);
                }
            }
        }

        private static void CreateKeysFile()
        {
            foreach (Actions action in Enum.GetValues(typeof(Actions)))
            {
                if (ActionKeysDefault.TryGetValue(action, out var defaultValue))
                {
                    string value = Regex.Replace(action.ToString(), "([a-z])([A-Z])", "$1 $2");
                    CONFIG_Keys.Bind(SECTION_Keys, value, defaultValue);
                    if (CONFIG_Keys.TryGetEntry(SECTION_Keys, value, out ConfigEntry<KeyCode> entry))
                        ActionKeys[action] = entry.Value;
                }
            }
        }

        private static void CreateSettingsFile()
        {
            Plugin.SetMaxFPS(BindSetting(ref SETTINGS_DefaultFPS));

            BindSetting(ref SETTINGS_LimitValue);
            BindSetting(ref SETTINGS_Uncap);
            BindSetting(ref SETTINGS_Skip);
            BindSetting(ref SETTINGS_MashEnabled);
            BindSetting(ref SETTINGS_ShowFPS);
            BindSetting(ref SETTINGS_FPSSize);
            BindSetting(ref SETTINGS_FPSPos);
            BindSetting(ref SETTINGS_UncapLoading);
            BindSetting(ref SETTINGS_MouseFix);
            BindSetting(ref SETTINGS_DebugMode);
        }

        private static T BindSetting<T>(ref (string, T, T) setting)
        {
            CONFIG_Settings.Bind(SECTION_Settings, setting.Item1, setting.Item3);
            if (CONFIG_Settings.TryGetEntry(SECTION_Settings, setting.Item1, out ConfigEntry<T> entry))
                return setting.Item2 = entry.Value;
            return default(T);
        }

        internal static void UpdateFPSSetting(int fps)
        {
            SETTINGS_DefaultFPS.Item2 = fps;
            if (CONFIG_Settings.TryGetEntry(SECTION_Settings, SETTINGS_DefaultFPS.Item1, out ConfigEntry<int> entry))
                entry.Value = fps;
        }
    }
}
