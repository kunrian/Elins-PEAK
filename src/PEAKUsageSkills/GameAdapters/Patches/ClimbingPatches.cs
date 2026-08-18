using System;
using HarmonyLib;
using UnityEngine;

namespace PEAKUsageSkills.GameAdapters.Patches
{
    [HarmonyPatch(typeof(CharacterClimbing), "GetRequestedPostition")]
    internal static class WallClimbingSpeedPatch
    {
        private struct State
        {
            public bool Applied;
            public float Original;
            public float OriginalSlippy;
        }

        private static void Prefix(CharacterClimbing __instance, Character ___character, out State __state)
        {
            __state = new State
            {
                Applied = false,
                Original = __instance.climbSpeedMod,
                OriginalSlippy = ___character?.data.slippy ?? 0f
            };

            if (___character == null
                || !Plugin.Effects.CanApply(___character)
                || ___character.data.isRopeClimbing
                || ___character.data.isVineClimbing)
            {
                return;
            }

            __instance.climbSpeedMod = __state.Original * Plugin.Effects.WallSpeedMultiplier;
            ___character.data.slippy = __state.OriginalSlippy * Plugin.Effects.WetGripPenaltyMultiplier;
            __state.Applied = true;
        }

        private static void Postfix(CharacterClimbing __instance, State __state)
        {
            Restore(__instance, __state);
        }

        private static Exception? Finalizer(CharacterClimbing __instance, State __state, Exception? __exception)
        {
            Restore(__instance, __state);
            return __exception;
        }

        private static void Restore(CharacterClimbing instance, State state)
        {
            if (state.Applied)
            {
                instance.climbSpeedMod = state.Original;
                Character? character = AccessTools.Field(typeof(CharacterClimbing), "character").GetValue(instance) as Character;
                if (character != null)
                {
                    character.data.slippy = state.OriginalSlippy;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CharacterRopeHandling), "Update")]
    internal static class RopeClimbingSpeedPatch
    {
        private struct State
        {
            public bool Applied;
            public float Original;
        }

        private static void Prefix(CharacterRopeHandling __instance, Character ___character, out State __state)
        {
            __state = new State
            {
                Applied = false,
                Original = __instance.climbSpeedMod
            };

            if (___character == null
                || !___character.data.isRopeClimbing
                || !Plugin.Effects.CanApply(___character))
            {
                return;
            }

            __instance.climbSpeedMod = __state.Original * Plugin.Effects.RopeSpeedMultiplier;
            __state.Applied = true;
        }

        private static Exception? Finalizer(CharacterRopeHandling __instance, State __state, Exception? __exception)
        {
            if (__state.Applied)
            {
                __instance.climbSpeedMod = __state.Original;
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(CharacterVineClimbing), "FixedUpdate")]
    internal static class VineClimbingSpeedPatch
    {
        private struct State
        {
            public bool Applied;
            public float Original;
            public float OriginalSlideDeceleration;
        }

        private static void Prefix(CharacterVineClimbing __instance, Character ___character, out State __state)
        {
            __state = new State
            {
                Applied = false,
                Original = __instance.climbSpeedMod,
                OriginalSlideDeceleration = __instance.slideDeceleration
            };

            if (___character == null
                || !___character.data.isVineClimbing
                || !Plugin.Effects.CanApply(___character))
            {
                return;
            }

            __instance.climbSpeedMod = __state.Original * Plugin.Effects.VineSpeedMultiplier;
            __instance.slideDeceleration = Mathf.Lerp(
                __state.OriginalSlideDeceleration,
                1f,
                Plugin.Effects.VineMomentumRetention);
            __state.Applied = true;
        }

        private static Exception? Finalizer(CharacterVineClimbing __instance, State __state, Exception? __exception)
        {
            if (__state.Applied)
            {
                __instance.climbSpeedMod = __state.Original;
                __instance.slideDeceleration = __state.OriginalSlideDeceleration;
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(WindChillZone), "ApplyStatus")]
    internal static class WetGripWindCostPatch
    {
        private static void Prefix(Character character, out float __state)
        {
            __state = character?.refs?.climbing?.climbingStamMinimumMultiplier ?? 1f;
        }

        private static void Postfix(Character character, float __state)
        {
            CharacterClimbing? climbing = character?.refs?.climbing;
            if (climbing == null || !Plugin.Effects.CanApply(character))
            {
                return;
            }

            float rawMultiplier = climbing.climbingStamMinimumMultiplier;
            if (rawMultiplier > Math.Max(1f, __state))
            {
                climbing.climbingStamMinimumMultiplier = 1f
                    + (rawMultiplier - 1f) * Plugin.Effects.WetGripPenaltyMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Character), "UpdateVariablesFixed")]
    internal static class LowStaminaClimbingControlPatch
    {
        private static void Postfix(Character __instance)
        {
            if (!Plugin.Effects.CanApply(__instance)
                || !__instance.data.isClimbing
                || __instance.GetTotalStamina() >= 0.20f)
            {
                return;
            }

            float vanillaControl = Mathf.Clamp01(__instance.data.staminaMod);
            __instance.data.staminaMod = 1f
                - (1f - vanillaControl) * Plugin.Effects.ClimbingTenacityPenaltyMultiplier;
        }
    }

    [HarmonyPatch(typeof(CharacterClimbing), "Climbing")]
    internal static class LowStaminaClimbingSlidePatch
    {
        private struct State
        {
            public bool Applied;
            public float Original;
        }

        private static void Prefix(Character ___character, out State __state)
        {
            __state = default;
            if (___character == null
                || !Plugin.Effects.CanApply(___character)
                || ___character.data.outOfStaminaClimbingFor <= 0f)
            {
                return;
            }

            __state.Original = ___character.data.outOfStaminaClimbingFor;
            ___character.data.outOfStaminaClimbingFor = __state.Original
                * Plugin.Effects.ClimbingTenacityPenaltyMultiplier;
            __state.Applied = true;
        }

        private static Exception? Finalizer(Character ___character, State __state, Exception? __exception)
        {
            if (__state.Applied && ___character != null)
            {
                ___character.data.outOfStaminaClimbingFor = __state.Original;
            }

            return __exception;
        }
    }
}
