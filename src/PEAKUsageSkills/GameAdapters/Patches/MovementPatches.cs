using System;
using System.Reflection;
using HarmonyLib;
using PEAKUsageSkills.Core;

namespace PEAKUsageSkills.GameAdapters.Patches
{
    [HarmonyPatch(typeof(CharacterMovement), "GetMovementForce")]
    internal static class AthleticsMovementPatch
    {
        private static void Postfix(Character ___character, ref float __result)
        {
            if (__result <= 0f || ___character == null || !Plugin.Effects.CanApply(___character))
            {
                return;
            }

            if (___character.data.isGrounded && !___character.data.isClimbingAnything)
            {
                __result *= Plugin.Effects.AthleticsMovementMultiplier;
                if (___character.data.isSprinting)
                {
                    __result *= Plugin.Effects.AthleticsSprintMovementMultiplier;
                }
            }

        }
    }

    [HarmonyPatch(typeof(Character), "CalculateWorldMovementDir")]
    internal static class AgilityAirControlPatch
    {
        private struct State
        {
            public bool Applied;
            public float Original;
        }

        private static void Prefix(Character __instance, out State __state)
        {
            __state = default;
            CharacterMovement? movement = __instance.refs?.movement;
            if (movement == null
                || __instance.data.isGrounded
                || __instance.data.isClimbingAnything
                || !Plugin.Effects.CanApply(__instance))
            {
                return;
            }

            __state.Original = movement.airMovementTurnSpeed;
            movement.airMovementTurnSpeed = __state.Original * Plugin.Effects.AgilityAirControlMultiplier;
            __state.Applied = true;
        }

        private static Exception? Finalizer(Character __instance, State __state, Exception? __exception)
        {
            if (__state.Applied && __instance?.refs?.movement != null)
            {
                __instance.refs.movement.airMovementTurnSpeed = __state.Original;
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(CharacterMovement), "JumpRpc")]
    internal static class AgilityJumpAwardPatch
    {
        private static void Prefix(Character ___character, bool isPalJump)
        {
            if (___character == null
                || Character.localCharacter != ___character
                || isPalJump)
            {
                return;
            }

            Plugin.Progression.AwardWork(
                SkillId.Agility,
                1d,
                Plugin.Settings.AgilityXpPerJump.Value,
                "ExecutedJump");
        }
    }

    internal static class JumpEffectScope
    {
        [ThreadStatic]
        private static Character? character;

        [ThreadStatic]
        private static int depth;

        public static bool Matches(Character candidate)
        {
            return depth > 0 && character == candidate;
        }

        public static void Enter(Character value)
        {
            character = value;
            depth++;
        }

        public static void Exit()
        {
            depth = Math.Max(0, depth - 1);
            if (depth == 0)
            {
                character = null;
            }
        }
    }

    [HarmonyPatch]
    internal static class AgilityJumpRoutinePatch
    {
        private static readonly FieldInfo MovementCharacterField = AccessTools.Field(typeof(CharacterMovement), "character");
        private static readonly FieldInfo JumpImpulseField = AccessTools.Field(typeof(CharacterMovement), "jumpImpulse");

        private struct State
        {
            public CharacterMovement? Movement;
            public float OriginalJumpImpulse;
            public bool Applied;
            public bool ScopeEntered;
        }

        private static MethodBase TargetMethod()
        {
            Type displayClass = typeof(CharacterMovement).GetNestedType(
                "<>c__DisplayClass69_0",
                BindingFlags.NonPublic) ?? throw new MissingMemberException("CharacterMovement jump display class");
            Type iteratorType = displayClass.GetNestedType(
                "<<JumpRpc>g__IDoJump|0>d",
                BindingFlags.NonPublic) ?? throw new MissingMemberException("CharacterMovement jump iterator");
            return AccessTools.Method(iteratorType, "MoveNext")
                ?? throw new MissingMethodException(iteratorType.FullName, "MoveNext");
        }

        private static void Prefix(object __instance, out State __state)
        {
            __state = default;
            FieldInfo? closureField = AccessTools.Field(__instance.GetType(), "<>4__this");
            object? closure = closureField?.GetValue(__instance);
            FieldInfo? movementField = closure == null ? null : AccessTools.Field(closure.GetType(), "<>4__this");
            CharacterMovement? movement = movementField?.GetValue(closure) as CharacterMovement;
            Character? character = movement == null ? null : MovementCharacterField.GetValue(movement) as Character;
            if (movement == null || character == null || Character.localCharacter != character)
            {
                return;
            }

            __state.Movement = movement;
            JumpEffectScope.Enter(character);
            __state.ScopeEntered = true;

            if (!Plugin.Effects.CanApply(character))
            {
                return;
            }

            __state.OriginalJumpImpulse = (float)JumpImpulseField.GetValue(movement);
            JumpImpulseField.SetValue(
                movement,
                __state.OriginalJumpImpulse * Plugin.Effects.AgilityJumpMultiplier);
            __state.Applied = true;
        }

        private static Exception? Finalizer(State __state, Exception? __exception)
        {
            if (__state.Applied && __state.Movement != null)
            {
                JumpImpulseField.SetValue(__state.Movement, __state.OriginalJumpImpulse);
            }

            if (__state.ScopeEntered)
            {
                JumpEffectScope.Exit();
            }

            return __exception;
        }
    }
}
