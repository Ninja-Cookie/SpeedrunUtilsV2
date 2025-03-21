using HarmonyLib;
using Reptile;
using System;

namespace SpeedrunUtilsV2.Patches
{
    class Patch_CutsceneMash : HarmonyPatch
    {
        [HarmonyPatch(typeof(GameInput), "GetButtonNew", typeof(int), typeof(int))]
        private static class Patch_GameInput_GetButtonNew
        {
            internal static void Postfix(GameInput __instance, int actionId, ref bool __result)
            {
                if (!LiveSplitConfig.SETTINGS_MashEnabled.Item2 || actionId != 2 || SequenceHandler.instance == null || !SequenceHandler.instance.IsInSequence())
                    return;

                DialogueUI dialogue = Core.Instance?.UIManager?.GetValue<DialogueUI>("dialogueUI");
                if (SequenceHandler.instance.GetValue<Enum>("skipTextActiveState").ToString() == "NOT_SKIPPABLE" && dialogue != null && dialogue.IsDialogueUIActive() && dialogue.CanBeSkipped && !dialogue.isYesNoPromptEnabled && (dialogue.ReadyToResume || !dialogue.GetValue<bool>("fastForwardTypewriter")))
                    __result = true;
            }
        }
    }
}
