using PEAKUsageSkills.Core;
using Xunit;

namespace PEAKUsageSkills.Tests
{
    public sealed class SkillMathTests
    {
        [Fact]
        public void ExperienceCurveStartsAtOneHundredAndIncreases()
        {
            Assert.Equal(100d, SkillMath.ExperienceToNextLevel(1));
            Assert.True(SkillMath.ExperienceToNextLevel(10) > SkillMath.ExperienceToNextLevel(5));
            Assert.True(SkillMath.ExperienceToNextLevel(50) > SkillMath.ExperienceToNextLevel(10));
            Assert.True(SkillMath.ExperienceToNextLevel(999) > SkillMath.ExperienceToNextLevel(50));
            Assert.True(double.IsFinite(SkillMath.ExperienceToNextLevel(999)));
            Assert.Equal(999, SkillMath.DefaultMaximumLevel);
        }

        [Theory]
        [InlineData(0, 0.003f, 0.15f, 0f)]
        [InlineData(1, 0.003f, 0.15f, 0.003f)]
        [InlineData(25, 0.003f, 0.15f, 0.075f)]
        [InlineData(50, 0.003f, 0.15f, 0.15f)]
        [InlineData(100, 0.003f, 0.15f, 0.15f)]
        public void LinearBonusIsCapped(int level, float perLevel, float maximum, float expected)
        {
            Assert.Equal(expected, SkillMath.CappedLinearBonus(level, perLevel, maximum), 5);
        }

        [Fact]
        public void ReductionMultiplierNeverExceedsConfiguredReduction()
        {
            Assert.Equal(0.75f, SkillMath.ReductionMultiplier(100, 0.005f, 0.25f), 5);
            Assert.Equal(1f, SkillMath.ReductionMultiplier(0, 0.005f, 0.25f), 5);
        }

        [Theory]
        [InlineData(0, 0.003f, 0f)]
        [InlineData(50, 0.003f, 0.15f)]
        [InlineData(100, 0.003f, 0.30f)]
        [InlineData(300, 0.003f, 0.90f)]
        [InlineData(999, 0.003f, 2.997f)]
        public void LinearBonusContinuesThroughLevel999(int level, float perLevel, float expected)
        {
            Assert.Equal(expected, SkillMath.LinearBonus(level, perLevel), 5);
        }

        [Theory]
        [InlineData(0.0015f, 1.4985f)]
        [InlineData(0.002f, 1.998f)]
        [InlineData(0.003f, 2.997f)]
        [InlineData(0.005f, 4.995f)]
        public void EveryConfiguredRateRemainsFiniteAtLevel999(float perLevel, float expected)
        {
            float bonus = SkillMath.LinearBonus(999, perLevel);
            float reciprocal = SkillMath.ReciprocalBonusMultiplier(999, perLevel);
            Assert.Equal(expected, bonus, 5);
            Assert.True(float.IsFinite(bonus));
            Assert.True(float.IsFinite(reciprocal));
            Assert.True(reciprocal > 0f);
        }

        [Theory]
        [InlineData(0, 0.005f, 1f)]
        [InlineData(50, 0.005f, 0.8f)]
        [InlineData(300, 0.005f, 0.4f)]
        [InlineData(999, 0.005f, 0.1668057f)]
        public void ReciprocalMultiplierKeepsLinearHandlingBonusValid(int level, float perLevel, float expected)
        {
            Assert.Equal(expected, SkillMath.ReciprocalBonusMultiplier(level, perLevel), 5);
        }

        [Theory]
        [InlineData(0, 1f)]
        [InlineData(10, 0.94345f)]
        [InlineData(50, 0.76941f)]
        [InlineData(100, 0.62523f)]
        [InlineData(200, 0.45479f)]
        [InlineData(300, 0.35737f)]
        [InlineData(500, 0.25019f)]
        [InlineData(999, 0.001f)]
        public void AnchoredReductionUsesApprovedLevel500And999Targets(int level, float expected)
        {
            Assert.Equal(expected, SkillMath.AnchoredReductionMultiplier(level, 0.003f), 5);
        }

        [Fact]
        public void NewExperienceExponentIsOnePointTwoOne()
        {
            Assert.Equal(1622d, SkillMath.ExperienceToNextLevel(10));
            Assert.Equal(11370d, SkillMath.ExperienceToNextLevel(50));
            Assert.Equal(26303d, SkillMath.ExperienceToNextLevel(100));
        }

        [Theory]
        [InlineData(0f, 506f, 690f, 184f)]
        [InlineData(614f, 614f, 704f, 704f)]
        [InlineData(90f, 690f, 690f, 90f)]
        public void RenderedWidthCorrectionSupportsFixedAndStretchAnchors(
            float currentSizeDelta,
            float currentRenderedWidth,
            float targetRenderedWidth,
            float expectedSizeDelta)
        {
            Assert.Equal(
                expectedSizeDelta,
                SkillMath.SizeDeltaForRenderedWidth(currentSizeDelta, currentRenderedWidth, targetRenderedWidth),
                5);
        }

        [Theory]
        [InlineData(0d, 100d, 0)]
        [InlineData(1d, 100d, 1)]
        [InlineData(23.999d, 100d, 23)]
        [InlineData(99.999d, 100d, 99)]
        [InlineData(100d, 100d, 99)]
        [InlineData(25d, 0d, 0)]
        public void ExperienceProgressUsesFlooredTwoDigitPercentage(double experience, double required, int expected)
        {
            Assert.Equal(expected, SkillMath.ExperienceProgressPercent(experience, required));
        }

        [Theory]
        [InlineData(1f, 0f, 0.15f, 1.15f)]
        [InlineData(0.875f, 0.125f, 0.15f, 1.025f)]
        [InlineData(0f, 1.05f, 0.15f, 0.10f)]
        [InlineData(0f, 1.20f, 0.15f, 0f)]
        [InlineData(0.875f, 0.125f, 0f, 0.875f)]
        public void ExpandedCapacityAddsToTheBaseBeforeStatusPenalties(
            float currentMaximum,
            float statusSum,
            float capacityBonus,
            float expected)
        {
            Assert.Equal(expected, SkillMath.ExpandedStaminaCapacity(currentMaximum, statusSum, capacityBonus), 5);
        }

        [Theory]
        [InlineData(19, 0)]
        [InlineData(20, 1)]
        [InlineData(40, 2)]
        [InlineData(70, 3)]
        [InlineData(120, 4)]
        [InlineData(200, 5)]
        public void StrengthBackpackSlotMilestonesAreStable(int level, int expected)
        {
            Assert.Equal(expected, SkillMath.ExtraBackpackSlots(level));
        }
    }
}
