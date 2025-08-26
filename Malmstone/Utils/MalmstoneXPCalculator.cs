using System;

namespace Malmstone.Utils
{
    public class MalmstoneXPCalculator
    {
        private static readonly int[] PvpLevels = {
            0, 2000, 2000, 2000, 2000, 3000, 3000, 3000, 3000, 3000, 4000, 4000, 4000, 4000, 4000, 5500, 5500, 5500, 5500, 5500,
            7500, 7500, 7500, 7500, 7500, 10000, 10000, 10000, 10000, 10000, 20000, 20000
        };

        private const int InfinityLevelExp = 20000;
        private const int FrontlineWinExp = 1500;
        private const int FrontlineLose2Exp = 1250;
        private const int FrontlineLoseExp = 1000;
        private const int FrontlineDailyWinExp = 3000;
        private const int FrontlineDailyLose2Exp = 2750;
        private const int FrontlineDailyLoseExp = 2500;
        private const int CrystallineWinExp = 900;
        private const int CrystallineLoseExp = 700;
        private const int RivalWingsWinExp = 1250;
        private const int RivalWingsLoseExp = 750;

        public struct XpCalculationResult
        {
            public int RemainingXp;
            public int TargetLevel;
            public int CrystallineConflictWin;
            public int CrystallineConflictLose;
            public int CrystallineConflictExpectedMatches;
            public int FrontlineWin;
            public int FrontlineLose2nd;
            public int FrontlineLose3rd;
            public int FrontlineExpectedMatches;
            public int FrontlineDailyWin;
            public int FrontlineDailyLose2nd;
            public int FrontlineDailyLose3rd;
            public int FrontlineDailyExpectedMatches;
            public int RivalWingsWin;
            public int RivalWingsLose;
            public int RivalWingsExpectedMatches;
        }

        public static XpCalculationResult CalculateXp(int currentLevel, int goalLevel, int currentProgress)
        {

            int remainingXp = CalculateRemainingXpForLevels(currentLevel, goalLevel, currentProgress);

            if (remainingXp <= 0)
            {
                return new XpCalculationResult { RemainingXp = 0, TargetLevel = goalLevel };
            }

            return new XpCalculationResult
            {
                RemainingXp = remainingXp,
                TargetLevel = goalLevel,
                CrystallineConflictWin = CalculateActivityCount(remainingXp, CrystallineWinExp),
                CrystallineConflictLose = CalculateActivityCount(remainingXp, CrystallineLoseExp),
                CrystallineConflictExpectedMatches = CalculateExpectedCrystallineMatches(remainingXp),
                FrontlineWin = CalculateActivityCount(remainingXp, FrontlineWinExp),
                FrontlineLose2nd = CalculateActivityCount(remainingXp, FrontlineLose2Exp),
                FrontlineLose3rd = CalculateActivityCount(remainingXp, FrontlineLoseExp),
                FrontlineExpectedMatches = CalculateExpectedFrontlineMatches(remainingXp, false),
                FrontlineDailyWin = CalculateActivityCount(remainingXp, FrontlineDailyWinExp),
                FrontlineDailyLose2nd = CalculateActivityCount(remainingXp, FrontlineDailyLose2Exp),
                FrontlineDailyLose3rd = CalculateActivityCount(remainingXp, FrontlineDailyLoseExp),
                FrontlineDailyExpectedMatches = CalculateExpectedFrontlineMatches(remainingXp, true),
                RivalWingsWin = CalculateActivityCount(remainingXp, RivalWingsWinExp),
                RivalWingsLose = CalculateActivityCount(remainingXp, RivalWingsLoseExp),
                RivalWingsExpectedMatches = CalculateExpectedRivalWingsMatches(remainingXp)
            };
        }

        public static XpCalculationResult CalculateCrystallineConflictMatches(int currentLevel, int goalLevel, int currentProgress)
        {
            int remainingXp = CalculateRemainingXpForLevels(currentLevel, goalLevel, currentProgress);

            return new XpCalculationResult
            {
                RemainingXp = remainingXp,
                TargetLevel = goalLevel,
                CrystallineConflictWin = CalculateActivityCount(remainingXp, CrystallineWinExp),
                CrystallineConflictLose = CalculateActivityCount(remainingXp, CrystallineLoseExp),
                CrystallineConflictExpectedMatches = CalculateExpectedCrystallineMatches(remainingXp)
            };
        }

        public static XpCalculationResult CalculateFrontlineMatches(int currentLevel, int goalLevel, int currentProgress)
        {
            int remainingXp = CalculateRemainingXpForLevels(currentLevel, goalLevel, currentProgress);

            return new XpCalculationResult
            {
                RemainingXp = remainingXp,
                TargetLevel = goalLevel,
                FrontlineWin = CalculateActivityCount(remainingXp, FrontlineWinExp),
                FrontlineLose2nd = CalculateActivityCount(remainingXp, FrontlineLose2Exp),
                FrontlineLose3rd = CalculateActivityCount(remainingXp, FrontlineLoseExp),
                FrontlineExpectedMatches = CalculateExpectedFrontlineMatches(remainingXp, false),
                FrontlineDailyWin = CalculateActivityCount(remainingXp, FrontlineDailyWinExp),
                FrontlineDailyLose2nd = CalculateActivityCount(remainingXp, FrontlineDailyLose2Exp),
                FrontlineDailyLose3rd = CalculateActivityCount(remainingXp, FrontlineDailyLoseExp),
                FrontlineDailyExpectedMatches = CalculateExpectedFrontlineMatches(remainingXp, true)
            };
        }

        public static XpCalculationResult CalculateRivalWingsMatches(int currentLevel, int goalLevel, int currentProgress)
        {
            int remainingXp = CalculateRemainingXpForLevels(currentLevel, goalLevel, currentProgress);

            return new XpCalculationResult
            {
                RemainingXp = remainingXp,
                TargetLevel = goalLevel,
                RivalWingsWin = CalculateActivityCount(remainingXp, RivalWingsWinExp),
                RivalWingsLose = CalculateActivityCount(remainingXp, RivalWingsLoseExp),
                RivalWingsExpectedMatches = CalculateExpectedRivalWingsMatches(remainingXp)
            };
        }

        public static int GetXPTargetForCurrentLevel(int currentLevel)
        {
            if (currentLevel >= PvpLevels.Length)
            {
                return InfinityLevelExp;
            }
            return PvpLevels[currentLevel];

        }

        private static int CalculateRemainingXpForLevels(int currentLevel, int goalLevel, int currentProgress)
        {
            int remainingXp = 0;
            if (currentLevel <= PvpLevels.Length)
            {
                int upperBoundLevel = Math.Min(goalLevel, PvpLevels.Length);
                remainingXp = CalculateRemainingXpWithinChart(currentLevel, upperBoundLevel, currentProgress);

                if (goalLevel > PvpLevels.Length)
                {
                    currentLevel = PvpLevels.Length;
                }
            }
            else
            {
                remainingXp -= currentProgress;
            }
            if (goalLevel > PvpLevels.Length)
            {
                remainingXp += CalculateRemainingXpBeyondChart(currentLevel, goalLevel);
            }
            return remainingXp;
        }

        private static int CalculateRemainingXpWithinChart(int currentLevel, int goalLevel, int currentProgress)
        {
            int remainingXp = 0;
            for (int level = currentLevel; level < goalLevel; level++)
            {
                remainingXp += PvpLevels[level];
            }
            return remainingXp - currentProgress;
        }

        private static int CalculateRemainingXpBeyondChart(int currentLevel, int goalLevel)
        {
            return (goalLevel - currentLevel) * InfinityLevelExp;
        }

        private static int CalculateActivityCount(int remainingXp, int activityXp)
        {
            // Should always be greater than 0
            return Math.Max(1, (int)Math.Ceiling((double)remainingXp / activityXp));
        }
        
        private static int CalculateExpectedCrystallineMatches(int remainingXp)
        {
            // Expected Value assuming 50% win rate
            return (int)Math.Ceiling(remainingXp / ((CrystallineWinExp + CrystallineLoseExp) / 2.0));
        }
        
        private static int CalculateExpectedFrontlineMatches(int remainingXp, bool isDaily)
        {
            if (isDaily)
            {
                // Expected Value assuming 33% win rate, 33% 2nd place, 33% 3rd place
                return (int)Math.Ceiling(remainingXp / ((FrontlineDailyWinExp + FrontlineDailyLose2Exp + FrontlineDailyLoseExp) / 3.0));
            }
            // Expected Value assuming 33% win rate, 33% 2nd place, 33% 3rd place
            return (int)Math.Ceiling(remainingXp / ((FrontlineWinExp + FrontlineLose2Exp + FrontlineLoseExp) / 3.0));
        }
        
        private static int CalculateExpectedRivalWingsMatches(int remainingXp)
        {
            // Expected Value assuming 50% win rate
            return (int)Math.Ceiling(remainingXp / ((RivalWingsWinExp + RivalWingsLoseExp) / 2.0));
        }

    }
}
