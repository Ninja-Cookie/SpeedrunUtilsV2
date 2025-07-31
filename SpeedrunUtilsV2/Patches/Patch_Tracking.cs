using HarmonyLib;
using Reptile;
using SpeedrunUtilsV2.ProgressTracker;
using static SpeedrunUtilsV2.ProgressTracker.SaveData;

namespace SpeedrunUtilsV2.Patches
{
    internal class Patch_Tracking
    {
        [HarmonyPatch(typeof(Player), "EndGraffitiMode", MethodType.Normal)]
        private static class Patch_Player_EndGraffitiMode
        {
            internal static void Postfix(GraffitiSpot graffitiSpot)
            {
                if (graffitiSpot.topCrew == Crew.PLAYERS || graffitiSpot.topCrew == Crew.ROGUE)
                    ProgressTracker.Tracking.CurrentSaveData?.UpdateCurrentStageGraffiti();
            }
        }

        [HarmonyPatch(typeof(Pickup), "ApplyPickupType", MethodType.Normal)]
        private static class Patch_Pickup_ApplyPickupType
        {
            internal static void Postfix(Pickup.PickUpType pickupType)
            {
                switch (pickupType)
                {
                    case Pickup.PickUpType.MAP:
                    case Pickup.PickUpType.MUSIC_UNLOCKABLE:
                    case Pickup.PickUpType.GRAFFITI_UNLOCKABLE:
                    case Pickup.PickUpType.MOVESTYLE_SKIN_UNLOCKABLE:
                    case Pickup.PickUpType.OUTFIT_UNLOCKABLE:
                        ProgressTracker.Tracking.CurrentSaveData?.UpdateCurrentStageCollectables();
                    break;
                }
            }
        }

        [HarmonyPatch(typeof(BaseModule), "LoadStage", MethodType.Normal)]
        private static class Patch_BaseModule_LoadStage
        {
            internal static void Postfix()
            {
                if (Core.Instance?.SaveManager?.CurrentSaveSlot != null)
                    LoadProgressData(Core.Instance.SaveManager.CurrentSaveSlot.saveSlotId);
            }
        }

        private static void LoadProgressData(int slotId)
        {
            SaveData data = ProgressTracker.Tracking.LoadProgressData(slotId);

            for (int i = 0; i < data.StageData.Graffiti.Length; i++)
            {
                SaveData.Stage stage = (SaveData.Stage)i;
            }
        }

        [HarmonyPatch(typeof(TaxiUI), "TaxiFound", MethodType.Normal)]
        private static class Patch_TaxiUI_TaxiFound
        {
            internal static void Postfix()
            {
                ProgressTracker.Tracking.CurrentSaveData?.UpdateTaxis();
            }
        }

        [HarmonyPatch(typeof(NPC), "TaxiFound", MethodType.Normal)]
        private static class Patch_NPC_TaxiFound
        {
            internal static void Postfix()
            {
                ProgressTracker.Tracking.CurrentSaveData?.UpdateTaxis();
            }
        }
    }
}
