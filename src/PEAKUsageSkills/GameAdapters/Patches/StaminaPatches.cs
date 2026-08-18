using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using HarmonyLib;
using PEAKUsageSkills.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PEAKUsageSkills.GameAdapters.Patches
{
    [HarmonyPatch(typeof(Character), "GetMaxStamina")]
    internal static class StaminaCapacityPatch
    {
        private static void Postfix(Character __instance, ref float __result)
        {
            if (Plugin.Effects.CanApply(__instance))
            {
                float capacityBonus = Plugin.Effects.EnduranceCapacityMultiplier - 1f;
                float statusSum = __instance.refs?.afflictions?.statusSum ?? 0f;
                __result = SkillMath.ExpandedStaminaCapacity(__result, statusSum, capacityBonus);
            }
        }
    }

    [HarmonyPatch(typeof(Character), "UseStamina")]
    internal static class StaminaCostPatch
    {
        private static void Prefix(Character __instance, ref float usage)
        {
            if (__instance == null || Character.localCharacter != __instance || usage <= 0f)
            {
                return;
            }

            float rawUsage = usage;
            bool jumping = JumpEffectScope.Matches(__instance);
            bool wallClimbing = __instance.data.isClimbing
                && !__instance.data.isRopeClimbing
                && !__instance.data.isVineClimbing;
            string source = jumping
                ? "Jumping"
                : wallClimbing
                    ? "WallClimbing"
                    : __instance.data.isRopeClimbing
                        ? "RopeClimbing"
                        : __instance.data.isVineClimbing
                            ? "VineClimbing"
                            : __instance.data.isSprinting
                                ? "Sprinting"
                                : "OtherExertion";

            if (!__instance.infiniteStam)
            {
                Plugin.Progression.AwardWork(
                    SkillId.Endurance,
                    rawUsage,
                    Plugin.Settings.EnduranceXpPerStamina.Value,
                    source);
            }

            if (Plugin.Effects.CanApply(__instance))
            {
                int overflowItems = InventorySkillService.GetOverflowItemCount(__instance);
                usage *= Plugin.Effects.PackRatStaminaMultiplier(overflowItems);
                if (wallClimbing)
                {
                    usage *= Plugin.Effects.WallCostMultiplier;
                    if (__instance.GetTotalStamina() < 0.20f)
                    {
                        usage *= Plugin.Effects.ClimbingTenacityPenaltyMultiplier;
                    }
                }
                else if (__instance.data.isRopeClimbing)
                {
                    usage *= Plugin.Effects.RopeCostMultiplier;
                }
                else if (__instance.data.isVineClimbing)
                {
                    usage *= Plugin.Effects.VineCostMultiplier;
                }
                else if (jumping)
                {
                    usage *= Plugin.Effects.AgilityJumpCostMultiplier;
                }
                else if (__instance.data.isSprinting)
                {
                    usage *= Plugin.Effects.AthleticsSprintCostMultiplier;
                }
            }

            Plugin.Diagnostics?.RecordStaminaRequest(rawUsage, usage, source);
        }
    }

    [HarmonyPatch(typeof(Character), "OutOfRegularStamina")]
    internal static class BonusStaminaSprintPatch
    {
        private static void Postfix(Character __instance, ref bool __result)
        {
            if (__result
                && Plugin.Effects.CanApply(__instance)
                && __instance.data.extraStamina >= 0.001f)
            {
                // Vanilla sprinting stops on the regular pool even though
                // UseStamina supports spending bonus stamina. Keep sprinting
                // long enough for the normal spender to roll into that pool.
                __result = false;
            }
        }
    }

    internal static class RegenScope
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

    [HarmonyPatch(typeof(Character), "UpdateVariablesFixed")]
    internal static class StaminaRegenScopePatch
    {
        private static void Prefix(Character __instance, out bool __state)
        {
            __state = Character.localCharacter == __instance;
            if (__state)
            {
                RegenScope.Enter();
            }
        }

        private static Exception? Finalizer(bool __state, Exception? __exception)
        {
            if (__state)
            {
                RegenScope.Exit();
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(Character), "AddStamina")]
    internal static class StaminaRegenAmountPatch
    {
        private static void Prefix(Character __instance, [HarmonyArgument(0)] ref float amount)
        {
            if (RegenScope.Active && amount > 0f && Plugin.Effects.CanApply(__instance))
            {
                amount *= Plugin.Effects.EnduranceRegenMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(StaminaBar), "Update")]
    [HarmonyAfter("com.github.LengSword.PeakStatsEx")]
    [HarmonyPriority(Priority.Last)]
    internal static class StaminaBarVisualPatch
    {
        private const float VanillaOutlinePadding = 14f;

        private sealed class Baseline
        {
            public float BackingWidth { get; set; }
            public float OutlineWidth { get; set; }
            public float BackingLeft { get; set; }
            public float OutlineLeft { get; set; }
            public float FullBarLeft { get; set; }
            public float StaminaBarLeft { get; set; }
            public float MaxStaminaBarLeft { get; set; }
            public float ExtraBarLeft { get; set; }
            public RectTransform? StatusLayout { get; set; }
            public float StatusLayoutWidth { get; set; }
            public float StatusLayoutLeft { get; set; }
            public float LastLoggedCapacity { get; set; } = float.NaN;
            public int LastLoggedScreenWidth { get; set; } = -1;
            public int LastLoggedScreenHeight { get; set; } = -1;
        }

        private static readonly ConditionalWeakTable<StaminaBar, Baseline> Baselines = new ConditionalWeakTable<StaminaBar, Baseline>();
        private static readonly Vector3[] WorldCorners = new Vector3[4];

        private static void Postfix(StaminaBar __instance)
        {
            if (__instance == null
                || __instance.fullBar == null
                || __instance.staminaBar == null
                || __instance.maxStaminaBar == null
                || __instance.backing == null
                || __instance.staminaBarOutline == null)
            {
                return;
            }

            RectTransform? reference = __instance.transform.parent as RectTransform;
            if (reference == null)
            {
                return;
            }

            float unitWidth = __instance.fullBar.sizeDelta.x;
            if (unitWidth <= 1f)
            {
                return;
            }

            if (!Baselines.TryGetValue(__instance, out Baseline baseline))
            {
                RectTransform? statusLayout = __instance.maxStaminaBar.parent as RectTransform;
                baseline = new Baseline
                {
                    BackingWidth = __instance.backing.rectTransform.rect.width,
                    OutlineWidth = __instance.staminaBarOutline.rect.width,
                    BackingLeft = GetLeft(reference, __instance.backing.rectTransform),
                    OutlineLeft = GetLeft(reference, __instance.staminaBarOutline),
                    FullBarLeft = GetLeft(reference, __instance.fullBar),
                    StaminaBarLeft = GetLeft(reference, __instance.staminaBar),
                    MaxStaminaBarLeft = GetLeft(reference, __instance.maxStaminaBar),
                    ExtraBarLeft = __instance.extraBar != null ? GetLeft(reference, __instance.extraBar) : 0f,
                    StatusLayout = statusLayout,
                    StatusLayoutWidth = statusLayout != null ? statusLayout.rect.width : 0f,
                    StatusLayoutLeft = statusLayout != null ? GetLeft(reference, statusLayout) : 0f
                };
                Baselines.Add(__instance, baseline);
            }

            float capacityMultiplier = Plugin.Effects.EnduranceCapacityMultiplier;
            bool extend = Plugin.Settings.ExtendStaminaBar.Value
                && Plugin.Effects.CanApply(Character.observedCharacter)
                && capacityMultiplier > 1.0001f;
            float extraWidth = extend ? unitWidth * (capacityMultiplier - 1f) : 0f;
            float targetFrameWidth = unitWidth + extraWidth;

            SetRenderedWidthAndLeft(reference, __instance.backing.rectTransform, targetFrameWidth, baseline.BackingLeft);
            SetLeft(reference, __instance.fullBar, baseline.FullBarLeft);
            SetLeft(reference, __instance.staminaBar, baseline.StaminaBarLeft);
            if (baseline.StatusLayout != null
                && baseline.StatusLayout != __instance.fullBar
                && baseline.StatusLayout != __instance.backing.rectTransform)
            {
                SetRenderedWidthAndLeft(
                    reference,
                    baseline.StatusLayout,
                    targetFrameWidth,
                    baseline.StatusLayoutLeft);
            }

            SetLeft(reference, __instance.maxStaminaBar, baseline.MaxStaminaBarLeft);
            SetRenderedWidthAndLeft(
                reference,
                __instance.staminaBarOutline,
                targetFrameWidth + VanillaOutlinePadding,
                baseline.OutlineLeft);
            if (__instance.extraBar != null)
            {
                SetLeft(reference, __instance.extraBar, baseline.ExtraBarLeft);
            }

            UpdatePreciseWeightText(__instance, unitWidth);

            Plugin.Diagnostics?.RecordBar(
                unitWidth,
                __instance.backing.rectTransform.rect.width,
                __instance.staminaBarOutline.rect.width);

            bool capacityChanged = float.IsNaN(baseline.LastLoggedCapacity)
                || Math.Abs(baseline.LastLoggedCapacity - capacityMultiplier) >= 0.0001f;
            bool resolutionChanged = baseline.LastLoggedScreenWidth != Screen.width
                || baseline.LastLoggedScreenHeight != Screen.height;
            if (capacityChanged || resolutionChanged)
            {
                baseline.LastLoggedCapacity = capacityMultiplier;
                baseline.LastLoggedScreenWidth = Screen.width;
                baseline.LastLoggedScreenHeight = Screen.height;
                Mask[] masks = __instance.GetComponentsInParent<Mask>(true);
                RectMask2D[] rectMasks = __instance.GetComponentsInParent<RectMask2D>(true);
                Plugin.ModLog.LogInfo(
                    $"[UsageSkills:StaminaBar] extend={extend} capacity={capacityMultiplier:F4} "
                    + $"frameTarget={targetFrameWidth:F2} extraPx={extraWidth:F2} "
                    + $"unitWidth={unitWidth:F2} screen={Screen.width}x{Screen.height} "
                    + $"backingBase={baseline.BackingWidth:F2} outlineBase={baseline.OutlineWidth:F2} "
                    + $"parentMasks={masks.Length} parentRectMasks={rectMasks.Length} path={BuildPath(__instance.transform)} "
                    + $"backing=[{DescribeRect(reference, __instance.backing.rectTransform)}] "
                    + $"outline=[{DescribeRect(reference, __instance.staminaBarOutline)}] "
                    + $"full=[{DescribeRect(reference, __instance.fullBar)}] "
                    + $"current=[{DescribeRect(reference, __instance.staminaBar)}] "
                    + $"max=[{DescribeRect(reference, __instance.maxStaminaBar)}] "
                    + $"statusLayout=[{DescribeOptionalRect(reference, baseline.StatusLayout)}] "
                    + $"afflictions=[{DescribeAfflictions(reference, __instance.afflictions)}]");
            }
        }

        private static void SetRenderedWidthAndLeft(RectTransform reference, RectTransform transform, float width, float left)
        {
            Vector2 size = transform.sizeDelta;
            size.x = SkillMath.SizeDeltaForRenderedWidth(size.x, transform.rect.width, width);
            transform.sizeDelta = size;
            SetLeft(reference, transform, left);
        }

        private static void UpdatePreciseWeightText(StaminaBar staminaBar, float unitWidth)
        {
            if (staminaBar.afflictions == null || unitWidth <= 0f)
            {
                return;
            }

            foreach (BarAffliction affliction in staminaBar.afflictions)
            {
                if (affliction == null
                    || affliction.afflictionType != CharacterAfflictions.STATUSTYPE.Weight)
                {
                    continue;
                }

                Transform textTransform = affliction.transform.Find("StaminaInfo");
                TextMeshProUGUI? text = textTransform?.GetComponent<TextMeshProUGUI>();
                if (text == null || !text.gameObject.activeSelf)
                {
                    return;
                }

                string existing = text.text ?? string.Empty;
                int suffixStart = existing.IndexOf('(');
                string suffix = suffixStart >= 0 ? existing.Substring(suffixStart) : string.Empty;
                float weightPoints = affliction.size / unitWidth * 100f;
                string desired = weightPoints.ToString("F1", CultureInfo.InvariantCulture) + suffix;
                if (!string.Equals(existing, desired, StringComparison.Ordinal))
                {
                    text.text = desired;
                }

                return;
            }
        }

        private static void SetLeft(RectTransform reference, RectTransform transform, float left)
        {
            float delta = left - GetLeft(reference, transform);
            if (Math.Abs(delta) < 0.001f)
            {
                return;
            }

            transform.position += reference.TransformVector(new Vector3(delta, 0f, 0f));
        }

        private static float GetLeft(RectTransform reference, RectTransform transform)
        {
            transform.GetWorldCorners(WorldCorners);
            return reference.InverseTransformPoint(WorldCorners[0]).x;
        }

        private static string DescribeRect(RectTransform reference, RectTransform transform)
        {
            return $"name={transform.name},left={GetLeft(reference, transform):F2},rectW={transform.rect.width:F2},"
                + $"sizeX={transform.sizeDelta.x:F2},posX={transform.anchoredPosition.x:F2},"
                + $"anchor={transform.anchorMin.x:F2}/{transform.anchorMax.x:F2},pivot={transform.pivot.x:F2}";
        }

        private static string DescribeOptionalRect(RectTransform reference, RectTransform? transform)
        {
            return transform == null ? "none" : DescribeRect(reference, transform) + ",path=" + BuildPath(transform);
        }

        private static string DescribeAfflictions(RectTransform reference, BarAffliction[]? afflictions)
        {
            if (afflictions == null || afflictions.Length == 0)
            {
                return "none";
            }

            string[] descriptions = new string[afflictions.Length];
            for (int index = 0; index < afflictions.Length; index++)
            {
                BarAffliction affliction = afflictions[index];
                descriptions[index] = affliction == null || affliction.rtf == null
                    ? "missing"
                    : affliction.afflictionType + ":" + DescribeRect(reference, affliction.rtf) + ",path=" + BuildPath(affliction.rtf);
            }

            return string.Join(";", descriptions);
        }

        private static string BuildPath(Transform transform)
        {
            string path = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
