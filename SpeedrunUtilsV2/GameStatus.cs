using Reptile;
using System;
using static Reptile.Story;
using static SpeedrunUtilsV2.LiveSplitConfig;

namespace SpeedrunUtilsV2
{
    internal static class GameStatus
    {
        private static Story.ObjectiveID _currentStoryObjective = Story.ObjectiveID.NONE;
        private static Story.ObjectiveID PreviousStoryObjective = Story.ObjectiveID.NONE;

        internal static Story.ObjectiveID CurrentStoryObjective
        {
            get => _currentStoryObjective;

            set
            {
                if (value != _currentStoryObjective)
                    PreviousStoryObjective = _currentStoryObjective;

                _currentStoryObjective = value;
            }
        }

        private static Stage _currentStage = Stage.NONE;
        private static Stage PreviousStage = Stage.NONE;

        internal static Stage CurrentStage
        {
            get => _currentStage;

            set
            {
                if (value != _currentStage)
                    PreviousStage = _currentStage;

                _currentStage = value;
            }
        }

        internal static void SetupNewGame(SaveSlotData saveSlotData)
        {
            RefreshSplitsFile();

            CurrentSplitStates.Clear();
            foreach (var split in Enum.GetValues(typeof(Splits)))
                CurrentSplitStates.Add((Splits)split, false);

            ConnectionManager.StartingNewGame = true;
        }

        internal static void UpdateStage()
        {
            SaveSlotData saveSlotData = Core.Instance?.SaveManager?.CurrentSaveSlot;
            Stage currentStage = Utility.GetCurrentStage();
            if (saveSlotData != null && currentStage != Stage.NONE)
            {
                CurrentStoryObjective = saveSlotData.CurrentStoryObjective;
                CurrentStage = currentStage;
                CheckSplitsForStageChange(CurrentStage, PreviousStage);
            }
        }

        internal static void UpdateObjective(ObjectiveID objectiveID)
        {
            if (objectiveID != Story.ObjectiveID.NONE)
            {
                CurrentStoryObjective = objectiveID;
                CheckSplitsForObjectiveChange(CurrentStoryObjective, PreviousStoryObjective);
            }
        }

        internal static void FinalBossDead()
        {
            ShouldSplit(Splits.FinalBossDefeated);
        }

        private static void CheckSplitsForStageChange(Stage currentStage, Stage previousStage)
        {
            SaveSlotData saveSlotData = Core.Instance?.SaveManager?.CurrentSaveSlot;
            if (saveSlotData == null)
                return;

            ObjectiveID currentObjective = saveSlotData.CurrentStoryObjective;
            bool startObjectives    = currentObjective == ObjectiveID.EscapePoliceStation || currentObjective == ObjectiveID.JoinTheCrew;
            bool pyramidObjectives  = currentObjective == ObjectiveID.SearchForPrince || currentObjective == ObjectiveID.SearchForPrince2 || currentObjective == ObjectiveID.SearchForPrince3 || currentObjective == ObjectiveID.SearchForPrince4 || currentObjective == ObjectiveID.BeatSamurai;

            if (previousStage == Stage.Prelude && currentStage == Stage.hideout && startObjectives)
                ShouldSplit(Splits.PrologueEnd);

            else if (previousStage == Stage.osaka && currentStage == Stage.square && startObjectives)
                ShouldSplit(Splits.EarlySquare);

            else if (((previousStage == Stage.square && currentStage == Stage.downhill) || (previousStage == Stage.hideout && currentStage == Stage.downhill)) && (startObjectives || currentObjective == ObjectiveID.BeatFranks))
                ShouldSplit(Splits.VersumStart);

            else if (previousStage == Stage.downhill && currentStage == Stage.hideout && currentObjective == ObjectiveID.GoToSquare)
                ShouldSplit(Splits.Chapter1End);

            else if (previousStage == Stage.square && currentStage == Stage.tower && currentObjective == ObjectiveID.BeatEclipse)
                ShouldSplit(Splits.BrinkStart);

            else if (previousStage == Stage.tower && currentStage == Stage.hideout && currentObjective == ObjectiveID.BeatDotExe)
                ShouldSplit(Splits.Chapter2End);

            else if (previousStage == Stage.square && currentStage == Stage.Mall && currentObjective == ObjectiveID.BeatDotExe)
                ShouldSplit(Splits.MallStart);

            else if (previousStage == Stage.Mall && currentStage == Stage.hideout && currentObjective == ObjectiveID.SearchForPrince)
                ShouldSplit(Splits.Chapter3End);

            else if (previousStage == Stage.square && currentStage == Stage.pyramid && pyramidObjectives)
                ShouldSplit(Splits.PyramidStart);

            else if (previousStage == Stage.pyramid && currentStage == Stage.hideout && currentObjective == ObjectiveID.BeatOsaka)
                ShouldSplit(Splits.Chapter4End);
        }

        private static void CheckSplitsForObjectiveChange(ObjectiveID currentObjective, ObjectiveID previousObjective)
        {
            switch (currentObjective)
            {
                case ObjectiveID.DJChallenge1: ShouldSplit(Splits.Dream1Start); break;
                case ObjectiveID.DJChallenge2: ShouldSplit(Splits.Dream2Start); break;
                case ObjectiveID.DJChallenge3: ShouldSplit(Splits.Dream3Start); break;
                case ObjectiveID.DJChallenge4: ShouldSplit(Splits.Dream4Start); break;

                case ObjectiveID.SearchForPrince2:
                    if (previousObjective == ObjectiveID.SearchForPrince)
                        ShouldSplit(Splits.PrinceVersumEnd);
                break;

                case ObjectiveID.SearchForPrince3:
                    if (previousObjective == ObjectiveID.SearchForPrince2)
                        ShouldSplit(Splits.PrinceSquareEnd);
                break;

                case ObjectiveID.SearchForPrince4:
                    if (previousObjective == ObjectiveID.SearchForPrince3)
                        ShouldSplit(Splits.PrinceBrinkEnd);
                break;
            }
        }

        private static void ShouldSplit(Splits split)
        {
            RefreshSplitsFile();

            if (CurrentSplits.TryGetValue(split, out var canSplit) && canSplit && CurrentSplitStates.TryGetValue(split, out var hasBeenSplit) && !hasBeenSplit)
            {
                CurrentSplitStates[split] = true;
                ConnectionManager.StartSplit(split);
            }
        }
    }
}
