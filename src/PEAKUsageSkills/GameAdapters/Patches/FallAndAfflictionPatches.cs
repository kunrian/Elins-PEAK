using System;
using HarmonyLib;
using PEAKUsageSkills.Core;
using UnityEngine;

namespace PEAKUsageSkills.GameAdapters.Patches
{
    internal static class ExplicitItemStatusScope
    {
        [ThreadStatic]
        private static int depth;

        public static bool Active => depth > 0;

        public static void Enter()
        {
            depth++;
        }

        public static void Exit()
        {
            depth = Math.Max(0, depth - 1);
        }
    }

    internal static class EnvironmentalColdRecoveryScope
    {
        [ThreadStatic]
        private static int depth;

        public static bool Active => depth > 0;

        public static void Enter()
        {
            depth++;
        }

        public static void Exit()
        {
            depth = Math.Max(0, depth - 1);
        }
    }

    [HarmonyPatch(typeof(Action_ModifyStatus), "RunAction")]
    internal static class ExplicitItemStatusScopePatch
    {
        private static void Prefix()
        {
            ExplicitItemStatusScope.Enter();
        }

        private static Exception? Finalizer(Exception? __exception)
        {
            ExplicitItemStatusScope.Exit();
            return __exception;
        }
    }

    internal static class FallScope
    {
        [ThreadStatic]
        private static string? source;

        public static string? Source => source;

        public static void Enter(string value)
        {
            source = value;
        }

        public static void Exit()
        {
            source = null;
        }
    }

    [HarmonyPatch(typeof(CharacterMovement), "CheckFallDamage")]
    internal static class NormalFallScopePatch
    {
        private static void Prefix()
        {
            FallScope.Enter("CharacterMovement");
        }

        private static Exception? Finalizer(Exception? __exception)
        {
            FallScope.Exit();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(CharacterClimbing), "CheckFallDamage")]
    internal static class WallFallScopePatch
    {
        private static void Prefix()
        {
            FallScope.Enter("CharacterClimbing");
        }

        private static Exception? Finalizer(Exception? __exception)
        {
            FallScope.Exit();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(CharacterAfflictions), "AddStatus")]
    internal static class AddStatusPatch
    {
        private struct State
        {
            public bool Local;
            public float Before;
            public float RawRequested;
            public float AppliedRequested;
            public CharacterAfflictions.STATUSTYPE StatusType;
            public string Source;
            public bool ConditionExposure;
            public SkillId ConditionSkill;
            public bool EnvironmentalColdRecovery;
        }

        private static void Prefix(
            CharacterAfflictions __instance,
            CharacterAfflictions.STATUSTYPE statusType,
            ref float amount,
            bool fromRPC,
            out State __state)
        {
            bool local = __instance != null
                && __instance.character != null
                && Character.localCharacter == __instance.character;
            __state = new State
            {
                Local = local,
                Before = local ? __instance!.GetCurrentStatus(statusType) : 0f,
                RawRequested = amount,
                AppliedRequested = amount,
                StatusType = statusType,
                Source = fromRPC ? "AddRPC" : "AddLocal",
                ConditionExposure = false,
                ConditionSkill = default,
                EnvironmentalColdRecovery = false
            };

            string? fallSource = FallScope.Source;
            if (!local || amount <= 0f)
            {
                return;
            }

            bool warmingExistingCold = statusType == CharacterAfflictions.STATUSTYPE.Hot
                && __instance!.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) > 0f;
            if (warmingExistingCold && !ExplicitItemStatusScope.Active)
            {
                EnvironmentalColdRecoveryScope.Enter();
                __state.EnvironmentalColdRecovery = true;
            }

            if (statusType == CharacterAfflictions.STATUSTYPE.Injury && fallSource != null)
            {
                float rawAmount = amount;
                Plugin.Progression.AwardWork(
                    SkillId.Resilience,
                    rawAmount,
                    Plugin.Settings.ResilienceXpPerInjury.Value,
                    fallSource);

                if (Plugin.Effects.CanApply(__instance!.character))
                {
                    amount *= Plugin.Effects.ResilienceFallMultiplier;
                }

                __state.AppliedRequested = amount;
                __state.Source = "Fall:" + fallSource;
                Plugin.Diagnostics?.RecordFall(rawAmount, amount, fallSource);
                return;
            }

            if (!ConditionSkillAdapter.TryGetResistanceSkill(statusType, out SkillId conditionSkill))
            {
                return;
            }

            __state.ConditionExposure = true;
            __state.ConditionSkill = conditionSkill;
            __state.Source = "Exposure:" + statusType;
            if (!warmingExistingCold && Plugin.Effects.CanApply(__instance!.character))
            {
                amount *= Plugin.Effects.ConditionGainMultiplier(conditionSkill);
            }

            __state.AppliedRequested = amount;
        }

        private static void Postfix(CharacterAfflictions __instance, bool __result, State __state)
        {
            if (!__state.Local)
            {
                return;
            }

            float after = __instance.GetCurrentStatus(__state.StatusType);
            float actual = after - __state.Before;
            if (__state.ConditionExposure
                && __state.ConditionSkill != SkillId.HungerTolerance
                && __result
                && actual > 0f)
            {
                Plugin.Progression.AwardWork(
                    __state.ConditionSkill,
                    actual,
                    Plugin.Settings.ConditionXpPerStatus.Value,
                    __state.Source);
            }

            Plugin.Diagnostics?.RecordStatusChange(
                __state.StatusType.ToString(),
                __state.RawRequested,
                actual,
                after,
                __state.Source);
        }

        private static Exception? Finalizer(Exception? __exception, State __state)
        {
            if (__state.EnvironmentalColdRecovery)
            {
                EnvironmentalColdRecoveryScope.Exit();
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(CharacterAfflictions), "SubtractStatus")]
    internal static class SubtractStatusPatch
    {
        private struct State
        {
            public bool Local;
            public float Before;
            public float Requested;
            public CharacterAfflictions.STATUSTYPE StatusType;
            public string Source;
            public bool NaturalRecovery;
            public bool HasRecoverySkill;
            public SkillId RecoverySkill;
        }

        private static void Prefix(
            CharacterAfflictions __instance,
            CharacterAfflictions.STATUSTYPE statusType,
            ref float amount,
            bool fromRPC,
            bool decreasedNaturally,
            out State __state)
        {
            bool local = __instance != null
                && __instance.character != null
                && Character.localCharacter == __instance.character;
            bool environmentalColdRecovery = statusType == CharacterAfflictions.STATUSTYPE.Cold
                && EnvironmentalColdRecoveryScope.Active;
            bool naturalRecovery = decreasedNaturally || environmentalColdRecovery;
            __state = new State
            {
                Local = local,
                Before = local ? __instance!.GetCurrentStatus(statusType) : 0f,
                Requested = amount,
                StatusType = statusType,
                Source = environmentalColdRecovery
                    ? "NaturalRecovery:ColdByWarmth"
                    : decreasedNaturally ? "NaturalRecovery" : fromRPC ? "SubtractRPC" : "SubtractLocal",
                NaturalRecovery = naturalRecovery,
                HasRecoverySkill = ConditionSkillAdapter.TryGetRecoverySkill(statusType, out SkillId recoverySkill),
                RecoverySkill = recoverySkill
            };

            if (local
                && naturalRecovery
                && amount > 0f
                && __state.HasRecoverySkill
                && Plugin.Effects.CanApply(__instance!.character))
            {
                amount *= Plugin.Effects.ConditionRecoveryMultiplier(__state.RecoverySkill);
                if (!environmentalColdRecovery)
                {
                    __state.Source = "NaturalRecovery:" + statusType;
                }
            }
        }

        private static void Postfix(CharacterAfflictions __instance, State __state)
        {
            if (!__state.Local)
            {
                return;
            }

            float after = __instance.GetCurrentStatus(__state.StatusType);
            float actual = after - __state.Before;
            float actualRecovered = Mathf.Max(0f, -actual);
            if (__state.NaturalRecovery && __state.HasRecoverySkill && actualRecovered > 0f)
            {
                Plugin.Progression.AwardWork(
                    __state.RecoverySkill,
                    actualRecovered,
                    Plugin.Settings.ConditionRecoveryXpPerStatus.Value,
                    __state.Source);
            }

            Plugin.Diagnostics?.RecordStatusChange(
                __state.StatusType.ToString(),
                -__state.Requested,
                actual,
                after,
                __state.Source);
        }
    }
}
