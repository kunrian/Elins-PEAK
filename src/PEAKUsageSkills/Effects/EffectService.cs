using System;
using PEAKUsageSkills.Config;
using PEAKUsageSkills.Core;
using PEAKUsageSkills.GameAdapters;

namespace PEAKUsageSkills.Effects
{
    internal sealed class EffectService
    {
        private readonly UsageSkillsConfig config;
        private readonly ProgressionService progression;
        private readonly RunStateAdapter runState;

        public EffectService(UsageSkillsConfig config, ProgressionService progression, RunStateAdapter runState)
        {
            this.config = config;
            this.progression = progression;
            this.runState = runState;
        }

        public bool CanApply(Character? character)
        {
            return config.EnableMod.Value
                && config.EnableEffects.Value
                && runState.IsGameplayScene
                && character != null
                && Character.localCharacter == character;
        }

        public int GetEffectiveLevel(SkillId skillId)
        {
            int debugLevel = config.DebugAllSkillLevelOverride.Value;
            if (debugLevel >= 0)
            {
                return Math.Max(0, Math.Min(progression.MaximumLevel, debugLevel));
            }

            return progression.GetLevel(skillId);
        }

        public float EnduranceCapacityMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.Endurance),
            config.EnduranceCapacityPerLevel.Value);

        public float EnduranceRegenMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.Endurance),
            config.EnduranceRegenPerLevel.Value);

        public float EnduranceCostMultiplier => 1f;

        public float StrengthWeightMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.Strength),
            config.StrengthReductionPerLevel.Value);

        public float WallSpeedMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.WallClimbing),
            config.WallSpeedPerLevel.Value);

        public float WallCostMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.WallClimbing),
            config.WallCostReductionPerLevel.Value);

        public float RopeSpeedMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.RopeClimbing),
            config.RopeSpeedPerLevel.Value);

        public float RopeCostMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.RopeClimbing),
            config.RopeCostEfficiencyPerLevel.Value);

        public float VineSpeedMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.VineClimbing),
            config.VineSpeedPerLevel.Value);

        public float VineCostMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.VineClimbing),
            config.VineCostEfficiencyPerLevel.Value);

        public float AthleticsMovementMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.Athletics),
            config.AthleticsMovementPerLevel.Value);

        public float AthleticsSprintMovementMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.Athletics),
            config.AthleticsSprintMovementPerLevel.Value);

        public float AthleticsSprintCostMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.Athletics),
            config.AthleticsSprintEfficiencyPerLevel.Value);

        public float AgilityJumpMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.Agility),
            config.AgilityJumpPerLevel.Value);

        public float AgilityAirControlMultiplier => 1f + SkillMath.LinearBonus(
            GetEffectiveLevel(SkillId.Agility),
            config.AgilityAirControlPerLevel.Value);

        public float AgilityJumpCostMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.Agility),
            config.AgilityJumpEfficiencyPerLevel.Value);

        public float ResilienceFallMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.Resilience),
            config.ResilienceFallReductionPerLevel.Value);

        public float ConditionGainMultiplier(SkillId skillId)
        {
            return SkillMath.AnchoredReductionMultiplier(
                GetEffectiveLevel(skillId),
                config.ConditionResistancePerLevel.Value);
        }

        public float ConditionRecoveryMultiplier(SkillId skillId)
        {
            return 1f + SkillMath.LinearBonus(
                GetEffectiveLevel(skillId),
                config.ConditionRecoveryPerLevel.Value);
        }

        public float PackRatPenaltyMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.PackRat),
            config.PackRatMitigationPerLevel.Value);

        public float PackRatWeightMultiplier(int overflowItems)
        {
            return 1f + Math.Max(0, overflowItems)
                * Math.Max(0f, config.PackRatWeightPenaltyPerItem.Value)
                * PackRatPenaltyMultiplier;
        }

        public float PackRatMovementMultiplier(int overflowItems)
        {
            float penalty = Math.Max(0, overflowItems)
                * Math.Max(0f, config.PackRatMovementPenaltyPerItem.Value)
                * PackRatPenaltyMultiplier;
            return Math.Max(0.1f, 1f - penalty);
        }

        public float PackRatStaminaMultiplier(int overflowItems)
        {
            return 1f + Math.Max(0, overflowItems)
                * Math.Max(0f, config.PackRatStaminaPenaltyPerItem.Value)
                * PackRatPenaltyMultiplier;
        }

        public float WetGripPenaltyMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.WetGrip),
            config.WetGripReductionPerLevel.Value);

        public float ClimbingTenacityPenaltyMultiplier => SkillMath.AnchoredReductionMultiplier(
            GetEffectiveLevel(SkillId.ClimbingTenacity),
            config.ClimbingTenacityReductionPerLevel.Value);

        public float VineMomentumRetention => Math.Min(
            0.75f,
            SkillMath.LinearBonus(
                GetEffectiveLevel(SkillId.VineClimbing),
                config.VineMomentumRetentionPerLevel.Value));
    }
}
