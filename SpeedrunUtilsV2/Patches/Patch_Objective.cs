using HarmonyLib;
using Reptile;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_Objective : HarmonyPatch
    {
        internal delegate void ObjectiveChanged(Story.ObjectiveID objectiveID);
        internal static ObjectiveChanged OnObjectiveChanged;

        [HarmonyPatch(typeof(SaveSlotData), "CurrentStoryObjective", MethodType.Setter)]
        private static class Patch_SaveSlotData_CurrentStoryObjective
        {
            internal static void Prefix(Story.ObjectiveID ___currentStoryObjective, Story.ObjectiveID value)
            {
                if (value != ___currentStoryObjective)
                    OnObjectiveChanged?.Invoke(value);
            }
        }
    }
}
