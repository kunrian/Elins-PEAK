using PEAKUsageSkills.Core;

namespace PEAKUsageSkills.GameAdapters
{
    internal static class ConditionSkillAdapter
    {
        public static bool TryGetResistanceSkill(CharacterAfflictions.STATUSTYPE statusType, out SkillId skillId)
        {
            switch (statusType)
            {
                case CharacterAfflictions.STATUSTYPE.Poison:
                    skillId = SkillId.Toxicology;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Cold:
                    skillId = SkillId.ColdTolerance;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Hot:
                    skillId = SkillId.HeatTolerance;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Drowsy:
                    skillId = SkillId.DrowsyTolerance;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Spores:
                    skillId = SkillId.SporeTolerance;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Hunger:
                    skillId = SkillId.HungerTolerance;
                    return true;
                default:
                    skillId = default;
                    return false;
            }
        }

        public static bool TryGetRecoverySkill(CharacterAfflictions.STATUSTYPE statusType, out SkillId skillId)
        {
            switch (statusType)
            {
                case CharacterAfflictions.STATUSTYPE.Poison:
                    skillId = SkillId.PoisonRecovery;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Cold:
                    skillId = SkillId.ColdRecovery;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Hot:
                    skillId = SkillId.HeatRecovery;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Drowsy:
                    skillId = SkillId.DrowsyRecovery;
                    return true;
                case CharacterAfflictions.STATUSTYPE.Spores:
                    skillId = SkillId.SporeRecovery;
                    return true;
                default:
                    skillId = default;
                    return false;
            }
        }
    }
}
