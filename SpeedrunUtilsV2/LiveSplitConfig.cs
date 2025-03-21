using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SpeedrunUtilsV2
{
    internal static class LiveSplitConfig
    {
        internal readonly static Dictionary<Splits, bool> CurrentSplits = new Dictionary<Splits, bool>()
        {
            { Splits.PrologueEnd,       false   },
            { Splits.EarlySquare,       false   },
            { Splits.VersumStart,       false   },
            { Splits.Dream1Start,       false   },
            { Splits.Chapter1End,       false   },

            { Splits.BrinkStart,        false   },
            { Splits.Dream2Start,       false   },
            { Splits.Chapter2End,       false   },

            { Splits.MallStart,         false   },
            { Splits.Dream3Start,       false   },
            { Splits.Chapter3End,       false   },

            { Splits.PrinceVersumEnd,   false   },
            { Splits.PrinceSquareEnd,   false   },
            { Splits.PrinceBrinkEnd,    false   },

            { Splits.PyramidStart,      false   },
            { Splits.Dream4Start,       false   },
            { Splits.Chapter4End,       false   },

            { Splits.FinalBossDefeated, false   }
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

            FinalBossDefeated
        }

        internal static readonly Dictionary<Splits, string> SplitDescriptions = new Dictionary<Splits, string>()
        {
            {Splits.PrologueEnd,        "Prologue Ended"                        },
            {Splits.EarlySquare,        "Entered Square Early"                  },
            {Splits.VersumStart,        "Entered Versum"                        },
            {Splits.Dream1Start,        "Dream 1 Started"                       },
            {Splits.Chapter1End,        "Chapter 1 Ended"                       },

            {Splits.BrinkStart,         "Entered Brink"                         },
            {Splits.Dream2Start,        "Dream 2 Started"                       },
            {Splits.Chapter2End,        "Chapter 2 Ended"                       },

            {Splits.MallStart,          "Entered Mall"                          },
            {Splits.Dream3Start,        "Dream 3 Started"                       },
            {Splits.Chapter3End,        "Chapter 3 Ended"                       },

            {Splits.PrinceVersumEnd,    "Talked To Frank in Versum"             },
            {Splits.PrinceSquareEnd,    "Beat the Frank Challenge in Square"    },
            {Splits.PrinceBrinkEnd,     "Talked To Frank in Brink"              },

            {Splits.PyramidStart,       "Entered Pyramid Island"                },
            {Splits.Dream4Start,        "Dream 4 Started"                       },
            {Splits.Chapter4End,        "Chapter 4 Ended"                       },

            {Splits.FinalBossDefeated,  "Defeated Final Boss"                   }
        };

        private static readonly Dictionary<Splits, bool> SplitDefault = new Dictionary<Splits, bool>()
        {
            {Splits.PrologueEnd,        true    },
            {Splits.EarlySquare,        true    },
            {Splits.VersumStart,        true    },
            {Splits.Dream1Start,        true    },
            {Splits.Chapter1End,        true    },

            {Splits.BrinkStart,         true    },
            {Splits.Dream2Start,        true    },
            {Splits.Chapter2End,        true    },

            {Splits.MallStart,          true    },
            {Splits.Dream3Start,        true    },
            {Splits.Chapter3End,        true    },

            {Splits.PrinceVersumEnd,    false   },
            {Splits.PrinceSquareEnd,    false   },
            {Splits.PrinceBrinkEnd,     false   },

            {Splits.PyramidStart,       true    },
            {Splits.Dream4Start,        true    },
            {Splits.Chapter4End,        true    },

            {Splits.FinalBossDefeated,  true    }
        };

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
            if (CurrentSplits.ContainsKey(split) && SplitDescriptions.TryGetValue(split, out var key))
            {
                CurrentSplits[split] = value;
                if (CONFIG_Splits.TryGetEntry(SECTION_Splits, key, out ConfigEntry<bool> entry))
                    entry.Value = value;
            }
        }

        private static void CreateSplitsFile()
        {
            foreach (var split in SplitDescriptions)
            {
                if (SplitDefault.TryGetValue(split.Key, out var defaultValue))
                {
                    CONFIG_Splits.Bind(SECTION_Splits, split.Value, defaultValue);
                    if (CONFIG_Splits.TryGetEntry(SECTION_Splits, split.Value, out ConfigEntry<bool> entry))
                        CurrentSplits[split.Key] = entry.Value;
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
