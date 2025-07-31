using Reptile;
using Reptile.Phone;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace SpeedrunUtilsV2.ProgressTracker
{
    internal class SaveData
    {
        internal Data StageData { get; private set; }

        internal SaveData(Data data = null)
        {
            StageData = data ?? new Data();
        }

        [Serializable]
        internal class Data
        {
            internal float[] Graffiti       = new float[(int)Stage.MAX];
            internal float[] Collectables   = new float[(int)Stage.MAX];
            internal float[] Characters     = new float[(int)Stage.MAX];
            internal float[] Taxis          = new float[(int)Stage.MAX];

            internal (string, bool)[] CurrentStageCollectableInfo   = Array.Empty<(string, bool)>();
            internal (string, bool)[] CurrentStageCharacterInfo     = Array.Empty<(string, bool)>();
        }

        internal string GetPercentageTotal()
        {
            if (StageData == null)
                return $"{(0f).ToString("##0.00")}%";

            float[] total = new float[4]
            {
                GetProgress(StageData.Graffiti),
                GetProgress(StageData.Collectables),
                GetProgress(StageData.Characters),
                GetProgress(StageData.Taxis)
            };

            return GetPercentage(total);
        }

        internal string GetPercentage(float[] data, Reptile.Stage? stage = null)
        {
            float progress = GetProgress(data, stage);
            return $"{progress.ToString("##0.00")}%";
        }

        internal float GetProgress(float[] data, Reptile.Stage? stage = null)
        {
            if (stage != null)
            {
                int stageIndex = (int)FromReptileStage((Reptile.Stage)stage);
                if (stageIndex == (int)Stage.NONE || data == null || data.Length < stageIndex + 1)
                    return 0f;

                return data[stageIndex];
            }

            if (data == null)
                return 0f;

            float total = 0f;
            foreach (var percent in data) { total += percent; }

            return (total / ((float)data.Length * 100f)) * 100f;
        }

        internal void UpdateAll()
        {
            UpdateTaxis(false);
            UpdateCurrentStageGraffiti(false);
            UpdateCurrentStageCollectables(false);
            UpdateCurrentCharacters(false);

            SaveSlotData saveSlot = Core.Instance?.SaveManager?.CurrentSaveSlot;
            if (saveSlot != null)
                Tracking.SaveProgressData(saveSlot.saveSlotId);
        }

        internal void UpdateTaxis(bool save = true)
        {
            SaveSlotData saveSlot = Core.Instance?.SaveManager?.CurrentSaveSlot;
            if (saveSlot == null || StageData?.Taxis == null)
                return;

            for (int i = 0; i < StageData.Taxis.Length; i++)
            {
                Reptile.Stage stage = SaveData.ToReptileStage((Stage)i);
                if (StageData.Taxis.Length >= i + 1)
                    StageData.Taxis[i] = saveSlot.GetStageProgress(stage).taxiFound ? 100f : 0f;
            }

            if (save)
                Tracking.SaveProgressData(saveSlot.saveSlotId);
        }

        internal void UpdateCurrentStageGraffiti(bool save = true)
        {
            if (WorldHandler.instance == null || Core.Instance?.SaveManager?.CurrentSaveSlot == null)
                return;

            var     grafSpots   = WorldHandler.instance.GetGrafSpots(false).Where(x => !(x is GraffitiSpotFinisher)).ToArray();
            float   percentage  = ((float)grafSpots.Where(x => x.topCrew == Crew.PLAYERS || x.topCrew == Crew.ROGUE).Count() / (float)grafSpots.Length) * 100f;

            int stageIndex = (int)FromReptileStage(Utility.GetCurrentStage());
            if (stageIndex == (int)Stage.NONE || StageData.Graffiti == null || StageData.Graffiti.Length < stageIndex + 1)
                return;

            StageData.Graffiti[stageIndex] = percentage;

            if (save)
                Tracking.SaveProgressData(Core.Instance.SaveManager.CurrentSaveSlot.saveSlotId);
        }

        private string GetLocalizedCollectableName(AUnlockable unlockable, Pickup.PickUpType type)
        {
            string itemName = string.Empty;
            var localizer = Core.Instance?.Localizer;

            if (localizer == null)
                return string.Empty;

            switch (type)
            {
                case Pickup.PickUpType.MUSIC_UNLOCKABLE:            itemName = (unlockable as MusicTrack)?.Title;                           break;
                case Pickup.PickUpType.GRAFFITI_UNLOCKABLE:         itemName = (unlockable as GraffitiAppEntry)?.Title;                     break;
                case Pickup.PickUpType.MOVESTYLE_SKIN_UNLOCKABLE:   itemName = localizer.GetSkinText((unlockable as MoveStyleSkin)?.Title); break;
                case Pickup.PickUpType.OUTFIT_UNLOCKABLE:
                    itemName = $"{localizer.GetSkinText((unlockable as OutfitUnlockable).outfitName)} ({localizer.GetCharacterName((unlockable as OutfitUnlockable).character)})";
                break;

                case Pickup.PickUpType.MAP: itemName = "Map"; break;
            }

            return itemName ?? string.Empty;
        }

        internal void UpdateCurrentStageCollectables(bool save = true)
        {
            var user = Core.Instance?.Platform?.User;

            if (WorldHandler.instance == null || Core.Instance?.SaveManager?.CurrentSaveSlot == null || user == null)
                return;

            var     pickups     = Tracking.GetCurrentStagePickups();
            float   unlocked    = 0;
            List<(string, bool)> info = new List<(string, bool)>();

            foreach (var pickup in pickups)
            {
                switch (pickup.Item2)
                {
                    case Tracking.PickupType.Pickup:
                        var     pickupObj       = (pickup.Item1 as Pickup);
                        var     unlock          = pickupObj?.unlock;
                        bool    isUnlocked      = false;

                        if ((pickupObj.pickupType == Pickup.PickUpType.MAP && Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().mapFound) || (pickupObj.pickupType != Pickup.PickUpType.MAP && unlock != null && user.GetUnlockableSaveDataFor(unlock).IsUnlocked))
                        {
                            unlocked++;
                            isUnlocked = true;
                        }

                        info.Add((GetLocalizedCollectableName(unlock, pickupObj.pickupType), isUnlocked));
                    break;

                    case Tracking.PickupType.Collectable:
                        var     collectableObj          = (pickup.Item1 as Collectable);
                        var     unlockCollectable       = collectableObj?.unlock;
                        bool    isCollectableUnlocked   = false;

                        if (unlockCollectable != null && user.GetUnlockableSaveDataFor(unlockCollectable).IsUnlocked)
                        {
                            unlocked++;
                            isCollectableUnlocked = true;
                        }

                        info.Add((GetLocalizedCollectableName(unlockCollectable, collectableObj.GetValue<Pickup.PickUpType>("pickUpType")), isCollectableUnlocked));
                    break;
                }
            }

            float percentage = (unlocked / (float)pickups.Length) * 100f;

            int stageIndex = (int)FromReptileStage(Utility.GetCurrentStage());
            if (stageIndex == (int)Stage.NONE || StageData?.Collectables == null || StageData.Collectables.Length < stageIndex + 1)
                return;

            StageData.CurrentStageCollectableInfo = info.ToArray();
            StageData.Collectables[stageIndex] = percentage;

            if (save)
                Tracking.SaveProgressData(Core.Instance.SaveManager.CurrentSaveSlot.saveSlotId);
        }

        internal void UpdateCurrentCharacters(bool save = true)
        {
            if (Core.Instance?.SaveManager?.CurrentSaveSlot == null || StageData?.Characters == null)
                return;

            var localizer = Core.Instance?.Localizer;
            List<(string, bool)> info = new List<(string, bool)>();

            for (int i = 0; i < StageData.Characters.Length; i++)
            {
                Reptile.Stage stage     = SaveData.ToReptileStage((Stage)i);
                Characters[] characters = CharacterList.Where(x => x.Value == stage)?.Select(x => x.Key)?.ToArray();
                if (characters == null || characters.Length == 0)
                    continue;

                List<Characters> unlockedCharacters = new List<Characters>();

                foreach (Characters character in characters)
                {
                    CharacterProgress currentProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character);
                    if (currentProgress == null)
                        continue;

                    bool unlocked = false;

                    if (currentProgress.unlocked)
                    {
                        unlockedCharacters.Add(character);
                        unlocked = true;
                    }

                    if (stage == Utility.GetCurrentStage())
                        info.Add((localizer?.GetCharacterName(character) ?? string.Empty, unlocked));
                }

                float percentage = ((float)unlockedCharacters.Count / (float)characters.Length) * 100f;
                if (StageData.Characters.Length < i + 1)
                    continue;

                StageData.Characters[i] = percentage;
            }

            StageData.CurrentStageCharacterInfo = info.ToArray();

            if (save)
                Tracking.SaveProgressData(Core.Instance.SaveManager.CurrentSaveSlot.saveSlotId);
        }

        internal static Stage FromReptileStage(Reptile.Stage stage)
        {
            switch (stage)
            {
                case Reptile.Stage.hideout:     return Stage.Hideout;
                case Reptile.Stage.downhill:    return Stage.Versum;
                case Reptile.Stage.square:      return Stage.Square;
                case Reptile.Stage.tower:       return Stage.Brink;
                case Reptile.Stage.Mall:        return Stage.Mall;
                case Reptile.Stage.pyramid:     return Stage.Pyramid;
                case Reptile.Stage.osaka:       return Stage.Mataan;

                default: return Stage.NONE;
            }
        }

        internal static Reptile.Stage ToReptileStage(Stage stage)
        {
            switch (stage)
            {
                case Stage.Hideout: return Reptile.Stage.hideout;
                case Stage.Versum:  return Reptile.Stage.downhill;
                case Stage.Square:  return Reptile.Stage.square;
                case Stage.Brink:   return Reptile.Stage.tower;
                case Stage.Mall:    return Reptile.Stage.Mall;
                case Stage.Pyramid: return Reptile.Stage.pyramid;
                case Stage.Mataan:  return Reptile.Stage.osaka;

                default: return Reptile.Stage.NONE;
            }
        }

        internal enum Stage
        {
            NONE = -1,
            Hideout,
            Versum,
            Square,
            Brink,
            Mall,
            Pyramid,
            Mataan,
            MAX
        }

        internal static readonly Dictionary<Characters, Reptile.Stage> CharacterList = new Dictionary<Characters, Reptile.Stage>()
        {
            { Characters.dummy,                 Reptile.Stage.hideout   },
            { Characters.girl1,                 Reptile.Stage.hideout   },

            { Characters.angel,                 Reptile.Stage.downhill  },
            { Characters.frank,                 Reptile.Stage.downhill  },
            { Characters.jetpackBossPlayer,     Reptile.Stage.downhill  },

            { Characters.dj,                    Reptile.Stage.square    },

            { Characters.wideKid,               Reptile.Stage.tower     },
            { Characters.medusa,                Reptile.Stage.tower     },

            { Characters.eightBallBoss,         Reptile.Stage.Mall      },
            { Characters.bunGirl,               Reptile.Stage.Mall      },

            { Characters.pufferGirl,            Reptile.Stage.pyramid   },
            { Characters.boarder,               Reptile.Stage.pyramid   },

            { Characters.ringdude,              Reptile.Stage.osaka     },
            { Characters.futureGirl,            Reptile.Stage.osaka     },
            { Characters.prince,                Reptile.Stage.osaka     }
        };
    }
}
