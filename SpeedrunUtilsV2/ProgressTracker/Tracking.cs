using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SpeedrunUtilsV2.ProgressTracker
{
    internal static class Tracking
    {
        internal static SaveData CurrentSaveData = new SaveData();

        internal static void CreateProgressData(int id)
        {
            CurrentSaveData = new SaveData();
            SaveProgressData(id);
        }

        internal static void SaveProgressData(int id)
        {
            if (TryGetDirectory(out var dir))
            {
                string path = Path.Combine(dir, $"data_{id}{LiveSplitConfig.EXTENSION_SaveData}");
                using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)))
                    Write(writer);
            }
        }

        internal static SaveData LoadProgressData(int id)
        {
            return CurrentSaveData = new SaveData(GetProgressData(id));
        }

        private static SaveData.Data GetProgressData(int id)
        {
            SaveData.Data data = new SaveData.Data();

            string path = Path.Combine(LiveSplitConfig.PATH_SaveData, $"data_{id}{LiveSplitConfig.EXTENSION_SaveData}");
            if (!File.Exists(path))
                return data;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)))
                    data = Read(reader);
            }
            catch
            {
                return data = new SaveData.Data();
            }

            return data;
        }

        private static void Write(BinaryWriter writer)
        {
            foreach (var graffiti in CurrentSaveData.StageData.Graffiti)
                writer.Write(graffiti);

            foreach (var collectable in CurrentSaveData.StageData.Collectables)
                writer.Write(collectable);

            foreach (var character in CurrentSaveData.StageData.Characters)
                writer.Write(character);

            foreach (var taxi in CurrentSaveData.StageData.Taxis)
                writer.Write(taxi);
        }

        private static SaveData.Data Read(BinaryReader reader)
        {
            SaveData.Data data = new SaveData.Data();

            for (int i = 0; i < data.Graffiti.Length; i++)
                data.Graffiti[i] = reader.ReadSingle();

            for (int i = 0; i < data.Collectables.Length; i++)
                data.Collectables[i] = reader.ReadSingle();

            for (int i = 0; i < data.Characters.Length; i++)
                data.Characters[i] = reader.ReadSingle();

            for (int i = 0; i < data.Taxis.Length; i++)
                data.Taxis[i] = reader.ReadSingle();

            return data;
        }

        private static bool TryGetDirectory(out string directory)
        {
            if (Directory.Exists(directory = LiveSplitConfig.PATH_SaveData))
                return true;

            try
            {
                if (Directory.CreateDirectory(LiveSplitConfig.PATH_SaveData).Exists)
                    return true;
            } catch {}
            return false;
        }

        internal static (UnityEngine.Object, PickupType)[] GetCurrentStagePickups()
        {
            List<Pickup>                            pickups         = new List<Pickup>();
            Collectable[]                           collectables    = Array.Empty<Collectable>();
            List<(UnityEngine.Object, PickupType)>  finalObjects    = new List<(UnityEngine.Object, PickupType)>();

            Stage stage = Utility.GetCurrentStage();

            if (Core.Instance?.SaveManager?.CurrentSaveSlot == null || stage == Stage.NONE || stage == Stage.MAX)
                return finalObjects.ToArray();

            StageProgress stageProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetStageProgress(stage);
            if (stageProgress == null)
                return finalObjects.ToArray();

            collectables = WorldHandler.instance?.SceneObjectsRegister?.gameplayEvents?.Where(x => x.GetType() == typeof(Collectable))?.Cast<Collectable>()?.ToArray();
            if (collectables == null)
                return finalObjects.ToArray();

            VendingMachine[] vendors = UnityEngine.Object.FindObjectsOfType<VendingMachine>()?.Where(x => x.rewards != null && x.rewards.Contains(VendingMachine.Reward.UNLOCKABLE_DROP))?.ToArray();
            if (vendors != null)
            {
                foreach (var vendor in vendors)
                {
                    if (vendor.unlockableDrop != null && vendor.unlockableDrop.TryGetComponent<Pickup>(out var pickup))
                        pickups.Add(pickup);
                }
            }

            Pickup map = WorldHandler.instance.SceneObjectsRegister.pickups?.FirstOrDefault(x => x.pickupType == Pickup.PickUpType.MAP);
            if (map != null)
                pickups.Add(map);

            foreach (var pickup in pickups)
                finalObjects.Add((pickup, PickupType.Pickup));

            foreach (var collectable in collectables)
                finalObjects.Add((collectable, PickupType.Collectable));

            return finalObjects.ToArray();
        }

        internal enum PickupType
        {
            Collectable,
            Pickup
        }
    }
}
