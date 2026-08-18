using System;

namespace PEAKUsageSkills.Core
{
    public static class SkillMath
    {
        public const int DefaultMaximumLevel = 999;

        public static double ExperienceToNextLevel(int level)
        {
            int safeLevel = Math.Max(1, level);
            return Math.Round(100d * Math.Pow(safeLevel, 1.21d), MidpointRounding.AwayFromZero);
        }

        public static float CappedLinearBonus(int level, float perLevel, float maximum)
        {
            if (level <= 0 || perLevel <= 0f || maximum <= 0f)
            {
                return 0f;
            }

            return Math.Min(maximum, level * perLevel);
        }

        public static float LinearBonus(int level, float perLevel)
        {
            if (level <= 0 || perLevel <= 0f)
            {
                return 0f;
            }

            return level * perLevel;
        }

        public static float ReciprocalBonusMultiplier(int level, float perLevel)
        {
            return 1f / (1f + LinearBonus(level, perLevel));
        }

        public static float AnchoredReductionMultiplier(
            int level,
            float perLevel,
            int anchorLevel = 500,
            int maximumLevel = DefaultMaximumLevel,
            float terminalMultiplier = 0.001f)
        {
            if (level <= 0 || perLevel <= 0f)
            {
                return 1f;
            }

            int safeAnchor = Math.Max(1, anchorLevel);
            int safeMaximum = Math.Max(safeAnchor + 1, maximumLevel);
            int clampedLevel = Math.Min(level, safeMaximum);
            float equivalentAtAnchor = safeMaximum;
            float anchorMultiplier = 1f / (1f + equivalentAtAnchor * perLevel);
            if (clampedLevel <= safeAnchor)
            {
                float equivalentLevel = clampedLevel * equivalentAtAnchor / safeAnchor;
                return 1f / (1f + equivalentLevel * perLevel);
            }

            float safeTerminal = Math.Max(0.000001f, Math.Min(anchorMultiplier, terminalMultiplier));
            double progress = (clampedLevel - safeAnchor) / (double)(safeMaximum - safeAnchor);
            return (float)(anchorMultiplier * Math.Pow(safeTerminal / anchorMultiplier, progress));
        }

        public static float SizeDeltaForRenderedWidth(float currentSizeDelta, float currentRenderedWidth, float targetRenderedWidth)
        {
            return currentSizeDelta + targetRenderedWidth - currentRenderedWidth;
        }

        public static float ReductionMultiplier(int level, float perLevel, float maximumReduction)
        {
            return 1f - CappedLinearBonus(level, perLevel, maximumReduction);
        }

        public static int ExperienceProgressPercent(double experience, double required)
        {
            if (experience <= 0d || required <= 0d || double.IsNaN(experience) || double.IsInfinity(experience))
            {
                return 0;
            }

            double percentage = Math.Floor(experience / required * 100d);
            return Math.Max(0, Math.Min(99, (int)percentage));
        }

        public static float ExpandedStaminaCapacity(float currentMaximum, float statusSum, float capacityBonus)
        {
            float vanillaBase = Math.Max(1f - Math.Max(0f, statusSum), 0f);
            float expandedBase = Math.Max(1f + Math.Max(0f, capacityBonus) - Math.Max(0f, statusSum), 0f);
            return Math.Max(0f, currentMaximum + expandedBase - vanillaBase);
        }

        public static int ExtraBackpackSlots(int strengthLevel)
        {
            if (strengthLevel >= 200) return 5;
            if (strengthLevel >= 120) return 4;
            if (strengthLevel >= 70) return 3;
            if (strengthLevel >= 40) return 2;
            if (strengthLevel >= 20) return 1;
            return 0;
        }

    }
}
