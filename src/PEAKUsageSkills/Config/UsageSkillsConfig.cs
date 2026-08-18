using BepInEx.Configuration;
using PEAKUsageSkills.Core;

namespace PEAKUsageSkills.Config
{
    internal sealed class UsageSkillsConfig
    {
        public UsageSkillsConfig(ConfigFile config)
        {
            EnableMod = config.Bind("General", "EnableMod", true, "Master switch for PEAK Usage Skills.");
            EnableExperience = config.Bind("General", "EnableExperience", true, "Allow eligible gameplay to grant usage-skill XP.");
            EnableEffects = config.Bind("General", "EnableEffects", true, "Apply gameplay effects from skill levels.");
            EnableXpInCustomRuns = config.Bind("General", "EnableXpInCustomRuns", false, "Allow XP in custom runs. Airport/lobby XP is always disabled.");
            ShowDebugOverlay = config.Bind("UI", "ShowDebugOverlay", false, "Show the compact diagnostics overlay. Also toggleable from the ESC menu.");
            ExtendStaminaBar = config.Bind("UI", "ExtendStaminaBar", true, "Extend stamina backing and outline for Endurance capacity above 100.");

            AutomaticDiagnostics = config.Bind("Diagnostics", "AutomaticDiagnostics", true, "Write automatic rate-limited gameplay diagnostics into the BepInEx log.");
            DiagnosticIntervalSeconds = config.Bind("Diagnostics", "IntervalSeconds", 2f, "Interval for aggregate runtime snapshots.");
            LogStatusChanges = config.Bind("Diagnostics", "LogStatusChanges", true, "Aggregate and log status/affliction changes.");

            MaximumLevel = config.Bind("Progression", "MaximumLevel", SkillMath.DefaultMaximumLevel, "Shared local save/schema level maximum. This is not selected by a multiplayer host.");
            SaveIntervalSeconds = config.Bind("Progression", "SaveIntervalSeconds", 30f, "Minimum interval between dirty progression saves.");
            DebugAllSkillLevelOverride = config.Bind("Progression", "DebugAllSkillLevelOverride", -1, "-1 uses saved levels. Set 0..MaximumLevel to test effects without altering progression.");

            EnduranceXpPerStamina = config.Bind("XP", "EnduranceXpPerStamina", 6f, "XP per normalized point of raw stamina requested (0.06 XP per displayed stamina point).");
            StrengthXpPerWork = config.Bind("XP", "StrengthXpPerWork", 2f, "XP per raw Weight x movement meter.");
            WallClimbingXpPerMeter = config.Bind("XP", "WallClimbingXpPerMeter", 4f, "XP per intentional wall-climbing meter in any direction.");
            RopeClimbingXpPerMeter = config.Bind("XP", "RopeClimbingXpPerMeter", 4f, "XP per intentional rope-climbing meter in any direction.");
            VineClimbingXpPerMeter = config.Bind("XP", "VineClimbingXpPerMeter", 4f, "XP per intentional vine-climbing meter in any direction.");
            AthleticsXpPerMeter = config.Bind("XP", "AthleticsXpPerMeter", 0.28f, "XP per qualifying grounded walking meter.");
            AthleticsSprintXpPerMeter = config.Bind("XP", "AthleticsSprintXpPerMeter", 1.12f, "XP per qualifying grounded sprinting meter.");
            AgilityXpPerJump = config.Bind("XP", "AgilityXpPerJump", 3.2f, "XP per successfully executed local jump.");
            ConfigEntry<float> legacyResilienceXpPerInjury = config.Bind("XP", "ResilienceXpPerInjury", 100f, "Legacy Vitality XP setting retained only for 0.4.1 migration.");
            VitalityXpPerInjury = config.Bind("XP", "VitalityXpPerInjury", 100f, "XP per normalized point of raw fall Injury.");
            ConditionXpPerStatus = config.Bind("XP", "ConditionXpPerStatus", 100f, "XP per normalized point of actual incoming Resiliency affliction.");
            WetGripXpPerMeter = config.Bind("XP", "WetGripXpPerMeter", 6f, "XP per slippery wall-climbing meter, weighted by current slipperiness.");
            ClimbingTenacityXpPerMeter = config.Bind("XP", "ClimbingTenacityXpPerMeter", 6f, "XP per intentional wall-climbing meter while regular stamina is below 20%.");

            StrengthReductionPerLevel = config.Bind("Effects", "StrengthWeightReductionPerLevel", 0.003f, "Anchored carry-Weight reduction rate per Strength level.");
            MaximumStrengthReduction = config.Bind("Effects", "MaximumStrengthWeightReduction", 0.25f, "Legacy cap retained for configuration compatibility; linear level scaling does not use it.");
            EnduranceCapacityPerLevel = config.Bind("Effects", "EnduranceCapacityPerLevel", 0.005f, "True base stamina capacity increase per Endurance level.");
            MaximumEnduranceCapacityBonus = config.Bind("Effects", "MaximumEnduranceCapacityBonus", 0.15f, "Legacy cap retained for configuration compatibility; linear level scaling does not use it.");
            EnduranceRegenPerLevel = config.Bind("Effects", "EnduranceRegenPerLevel", 0.001f, "Stamina regeneration increase per Endurance level.");
            MaximumEnduranceRegenBonus = config.Bind("Effects", "MaximumEnduranceRegenBonus", 0.25f, "Legacy cap retained for configuration compatibility; linear level scaling does not use it.");
            EnduranceCostReductionPerLevel = config.Bind("Effects", "EnduranceCostReductionPerLevel", 0f, "Legacy option. Endurance no longer reduces general stamina costs.");
            MaximumEnduranceCostReduction = config.Bind("Effects", "MaximumEnduranceCostReduction", 0.10f, "Legacy cap retained for configuration compatibility; linear level scaling does not use it.");
            WallSpeedPerLevel = config.Bind("Effects", "WallClimbingSpeedPerLevel", 0.003f, "Wall-climbing speed increase per level.");
            MaximumWallSpeedBonus = config.Bind("Effects", "MaximumWallClimbingSpeedBonus", 0.15f, "Legacy cap retained for configuration compatibility; linear level scaling does not use it.");
            WallCostReductionPerLevel = config.Bind("Effects", "WallClimbingCostReductionPerLevel", 0.003f, "Anchored wall-climbing stamina cost reduction rate per level.");
            MaximumWallCostReduction = config.Bind("Effects", "MaximumWallClimbingCostReduction", 0.10f, "Legacy cap retained for configuration compatibility; linear level scaling does not use it.");
            RopeSpeedPerLevel = config.Bind("Effects", "RopeClimbingSpeedPerLevel", 0.003f, "Rope-climbing speed increase per level.");
            RopeCostEfficiencyPerLevel = config.Bind("Effects", "RopeClimbingCostEfficiencyPerLevel", 0.003f, "Anchored rope-climbing stamina cost reduction rate per level.");
            VineSpeedPerLevel = config.Bind("Effects", "VineClimbingSpeedPerLevel", 0.003f, "Vine-climbing speed increase per level.");
            VineCostEfficiencyPerLevel = config.Bind("Effects", "VineClimbingCostEfficiencyPerLevel", 0.003f, "Anchored vine-climbing stamina cost reduction rate per level.");
            VineMomentumRetentionPerLevel = config.Bind("Effects", "VineMomentumRetentionPerLevel", 0.0005f, "Fraction of vine velocity damping removed per Vine Climbing level.");
            AthleticsMovementPerLevel = config.Bind("Effects", "AthleticsMovementSpeedPerLevel", 0.001f, "Ground movement-force increase per Athletics level.");
            AthleticsSprintMovementPerLevel = config.Bind("Effects", "AthleticsSprintSpeedPerLevel", 0.002f, "Additional sprint movement-force increase per Athletics level.");
            AthleticsSprintEfficiencyPerLevel = config.Bind("Effects", "AthleticsSprintEfficiencyPerLevel", 0.003f, "Anchored sprint stamina cost reduction rate per Athletics level.");
            AgilityJumpPerLevel = config.Bind("Effects", "AgilityJumpPerformancePerLevel", 0.0015f, "Jump impulse increase per Agility level.");
            AgilityAirControlPerLevel = config.Bind("Effects", "AgilityAirControlPerLevel", 0.00025f, "Light airborne turning responsiveness increase per Agility level.");
            AgilityJumpEfficiencyPerLevel = config.Bind("Effects", "AgilityJumpEfficiencyPerLevel", 0.003f, "Anchored jump stamina cost reduction rate per Agility level.");
            ConfigEntry<float> legacyResilienceFallReductionPerLevel = config.Bind("Effects", "ResilienceFallReductionPerLevel", 0.003f, "Legacy Vitality effect setting retained only for 0.4.1 migration.");
            VitalityFallReductionPerLevel = config.Bind("Effects", "VitalityFallReductionPerLevel", 0.003f, "Fall Injury reduction per Vitality level.");
            ConditionResistancePerLevel = config.Bind("Effects", "ConditionResistancePerLevel", 0.0015f, "Anchored incoming-condition reduction rate per matching Resiliency level.");
            ConditionRecoveryPerLevel = config.Bind("Effects", "ConditionRecoveryPerLevel", 0.0015f, "Natural condition-recovery increase per matching tolerance level.");
            WetGripReductionPerLevel = config.Bind("Effects", "WetGripReductionPerLevel", 0.003f, "Anchored reduction rate for slippery downward pull and wind climbing drain.");
            ClimbingTenacityReductionPerLevel = config.Bind("Effects", "ClimbingTenacityReductionPerLevel", 0.003f, "Anchored reduction rate for the below-20%-stamina climbing penalty.");

            ConfigEntry<int> configSchema = config.Bind("Internal", "ConfigSchema", 0, "Internal configuration migration marker.");
            if (configSchema.Value < 1)
            {
                // Version 0.1.2 and earlier wrote 50 as the default. Preserve
                // later user choices while migrating that legacy default once.
                if (MaximumLevel.Value == 50)
                {
                    MaximumLevel.Value = SkillMath.DefaultMaximumLevel;
                }

                configSchema.Value = 1;
                config.Save();
            }

            if (configSchema.Value < 2)
            {
                ShowDebugOverlay.Value = false;
                DebugAllSkillLevelOverride.Value = -1;
                MigrateExact(EnduranceXpPerStamina, 20f, 2f);
                MigrateExact(StrengthXpPerWork, 10f, 2f);
                MigrateExact(WallClimbingXpPerMeter, 8f, 2f);
                MigrateExact(RopeClimbingXpPerMeter, 8f, 2f);
                MigrateExact(VineClimbingXpPerMeter, 8f, 2f);
                MigrateExact(AthleticsXpPerMeter, 2f, 0.5f);
                MigrateExact(AgilityXpPerJump, 20f, 4f);
                MigrateExact(StrengthReductionPerLevel, 0.005f, 0.003f);
                MigrateExact(EnduranceCapacityPerLevel, 0.003f, 0.005f);
                MigrateExact(EnduranceRegenPerLevel, 0.005f, 0.001f);
                MigrateExact(EnduranceCostReductionPerLevel, 0.002f, 0f);
                MigrateExact(WallCostReductionPerLevel, 0.002f, 0.003f);
                MigrateExact(RopeCostEfficiencyPerLevel, 0.002f, 0.003f);
                MigrateExact(VineCostEfficiencyPerLevel, 0.002f, 0.003f);
                MigrateExact(AthleticsMovementPerLevel, 0.0015f, 0.001f);
                MigrateExact(AthleticsSprintEfficiencyPerLevel, 0.0015f, 0.003f);
                MigrateExact(AgilityJumpEfficiencyPerLevel, 0.0015f, 0.003f);
                MigrateExact(ConditionResistancePerLevel, 0.005f, 0.003f);
                MigrateExact(ConditionRecoveryPerLevel, 0.005f, 0.003f);
                configSchema.Value = 2;
                config.Save();
            }

            if (configSchema.Value < 3)
            {
                MigrateExact(AthleticsXpPerMeter, 0.5f, 0.35f);
                MigrateExact(AthleticsSprintXpPerMeter, 2f, 1.4f);
                configSchema.Value = 3;
                config.Save();
            }

            bool migratedToSchema4 = configSchema.Value < 4;
            if (migratedToSchema4)
            {
                MigrateExact(EnduranceXpPerStamina, 2f, 6f);
                MigrateExact(WallClimbingXpPerMeter, 2f, 4f);
                MigrateExact(RopeClimbingXpPerMeter, 2f, 4f);
                MigrateExact(VineClimbingXpPerMeter, 2f, 4f);
                MigrateExact(AthleticsXpPerMeter, 0.35f, 0.28f);
                MigrateExact(AthleticsSprintXpPerMeter, 1.4f, 1.12f);
                MigrateExact(AgilityXpPerJump, 4f, 3.2f);
                MigrateExact(WetGripXpPerMeter, 2f, 6f);
                MigrateExact(ClimbingTenacityXpPerMeter, 2f, 6f);
                MigrateExact(ConditionResistancePerLevel, 0.003f, 0.0015f);
                MigrateExact(ConditionRecoveryPerLevel, 0.003f, 0.0015f);
                configSchema.Value = 4;
            }

            bool migratedToSchema5 = configSchema.Value < 5;
            if (migratedToSchema5)
            {
                VitalityXpPerInjury.Value = legacyResilienceXpPerInjury.Value;
                VitalityFallReductionPerLevel.Value = legacyResilienceFallReductionPerLevel.Value;
                configSchema.Value = 5;
            }

            bool removedObsoleteEntries = false;
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("XP", "ResilienceXpPerInjury"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("Effects", "ResilienceFallReductionPerLevel"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("Effects", "MaximumResilienceFallReduction"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("XP", "ConditionRecoveryXpPerStatus"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("XP", "HungerMovementXpPerWork"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("XP", "PackRatXpPerWork"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("Effects", "HungerTrainingThreshold"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("Effects", "PackRatMitigationPerLevel"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("Effects", "PackRatWeightPenaltyPerItem"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("Effects", "PackRatMovementPenaltyPerItem"));
            removedObsoleteEntries |= config.Remove(new ConfigDefinition("Effects", "PackRatStaminaPenaltyPerItem"));
            if (removedObsoleteEntries || migratedToSchema4 || migratedToSchema5)
            {
                config.Save();
            }
        }

        private static void MigrateExact(ConfigEntry<float> entry, float oldDefault, float newDefault)
        {
            if (System.Math.Abs(entry.Value - oldDefault) < 0.000001f)
            {
                entry.Value = newDefault;
            }
        }

        public ConfigEntry<bool> EnableMod { get; }
        public ConfigEntry<bool> EnableExperience { get; }
        public ConfigEntry<bool> EnableEffects { get; }
        public ConfigEntry<bool> EnableXpInCustomRuns { get; }
        public ConfigEntry<bool> ShowDebugOverlay { get; }
        public ConfigEntry<bool> ExtendStaminaBar { get; }
        public ConfigEntry<bool> AutomaticDiagnostics { get; }
        public ConfigEntry<float> DiagnosticIntervalSeconds { get; }
        public ConfigEntry<bool> LogStatusChanges { get; }
        public ConfigEntry<int> MaximumLevel { get; }
        public ConfigEntry<float> SaveIntervalSeconds { get; }
        public ConfigEntry<int> DebugAllSkillLevelOverride { get; }
        public ConfigEntry<float> EnduranceXpPerStamina { get; }
        public ConfigEntry<float> StrengthXpPerWork { get; }
        public ConfigEntry<float> WallClimbingXpPerMeter { get; }
        public ConfigEntry<float> RopeClimbingXpPerMeter { get; }
        public ConfigEntry<float> VineClimbingXpPerMeter { get; }
        public ConfigEntry<float> AthleticsXpPerMeter { get; }
        public ConfigEntry<float> AthleticsSprintXpPerMeter { get; }
        public ConfigEntry<float> AgilityXpPerJump { get; }
        public ConfigEntry<float> VitalityXpPerInjury { get; }
        public ConfigEntry<float> ConditionXpPerStatus { get; }
        public ConfigEntry<float> WetGripXpPerMeter { get; }
        public ConfigEntry<float> ClimbingTenacityXpPerMeter { get; }
        public ConfigEntry<float> StrengthReductionPerLevel { get; }
        public ConfigEntry<float> MaximumStrengthReduction { get; }
        public ConfigEntry<float> EnduranceCapacityPerLevel { get; }
        public ConfigEntry<float> MaximumEnduranceCapacityBonus { get; }
        public ConfigEntry<float> EnduranceRegenPerLevel { get; }
        public ConfigEntry<float> MaximumEnduranceRegenBonus { get; }
        public ConfigEntry<float> EnduranceCostReductionPerLevel { get; }
        public ConfigEntry<float> MaximumEnduranceCostReduction { get; }
        public ConfigEntry<float> WallSpeedPerLevel { get; }
        public ConfigEntry<float> MaximumWallSpeedBonus { get; }
        public ConfigEntry<float> WallCostReductionPerLevel { get; }
        public ConfigEntry<float> MaximumWallCostReduction { get; }
        public ConfigEntry<float> RopeSpeedPerLevel { get; }
        public ConfigEntry<float> RopeCostEfficiencyPerLevel { get; }
        public ConfigEntry<float> VineSpeedPerLevel { get; }
        public ConfigEntry<float> VineCostEfficiencyPerLevel { get; }
        public ConfigEntry<float> VineMomentumRetentionPerLevel { get; }
        public ConfigEntry<float> AthleticsMovementPerLevel { get; }
        public ConfigEntry<float> AthleticsSprintMovementPerLevel { get; }
        public ConfigEntry<float> AthleticsSprintEfficiencyPerLevel { get; }
        public ConfigEntry<float> AgilityJumpPerLevel { get; }
        public ConfigEntry<float> AgilityAirControlPerLevel { get; }
        public ConfigEntry<float> AgilityJumpEfficiencyPerLevel { get; }
        public ConfigEntry<float> VitalityFallReductionPerLevel { get; }
        public ConfigEntry<float> ConditionResistancePerLevel { get; }
        public ConfigEntry<float> ConditionRecoveryPerLevel { get; }
        public ConfigEntry<float> WetGripReductionPerLevel { get; }
        public ConfigEntry<float> ClimbingTenacityReductionPerLevel { get; }
    }
}
