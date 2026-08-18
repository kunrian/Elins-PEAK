using HarmonyLib;
using System;
using System.Reflection;

namespace PEAKUsageSkills.GameAdapters.Patches
{
    [HarmonyPatch(typeof(CharacterAfflictions), "UpdateWeight")]
    [HarmonyAfter("nakazora.peak.piggyback")]
    [HarmonyPriority(Priority.Last)]
    internal static class WeightPatch
    {
        private static readonly MethodInfo? UpdateWeightMethod = AccessTools.Method(typeof(CharacterAfflictions), "UpdateWeight");

        private static void Postfix(CharacterAfflictions __instance)
        {
            if (__instance == null || __instance.character == null || Character.localCharacter != __instance.character)
            {
                return;
            }

            float rawWeight = __instance.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Weight);
            float effectiveWeight = rawWeight;
            if (rawWeight > 0f && Plugin.Effects.CanApply(__instance.character))
            {
                effectiveWeight = rawWeight * Plugin.Effects.StrengthWeightMultiplier;
                if (System.Math.Abs(effectiveWeight - rawWeight) > 0.00001f)
                {
                    __instance.SetStatus(CharacterAfflictions.STATUSTYPE.Weight, effectiveWeight, true);
                }
            }

            Plugin.Diagnostics?.RecordWeight(rawWeight, effectiveWeight);
        }

        internal static void RefreshLocalWeight(string source)
        {
            Character character = Character.localCharacter;
            CharacterAfflictions? afflictions = character?.refs?.afflictions;
            if (afflictions == null || UpdateWeightMethod == null)
            {
                Plugin.ModLog.LogWarning($"[UsageSkills:Weight] refresh skipped source={source} localAfflictions={afflictions != null}");
                return;
            }

            try
            {
                UpdateWeightMethod.Invoke(afflictions, null);
                Plugin.ModLog.LogInfo($"[UsageSkills:Weight] refreshed source={source}");
            }
            catch (Exception exception)
            {
                Plugin.ModLog.LogError($"[UsageSkills:Weight] refresh failed source={source}: {exception}");
            }
        }
    }
}
