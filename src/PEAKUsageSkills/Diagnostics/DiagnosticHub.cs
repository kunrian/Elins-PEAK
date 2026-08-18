using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using PEAKUsageSkills.Config;
using PEAKUsageSkills.Core;
using PEAKUsageSkills.Effects;
using PEAKUsageSkills.GameAdapters;
using UnityEngine;

namespace PEAKUsageSkills.Diagnostics
{
    internal sealed class DiagnosticHub
    {
        private readonly ManualLogSource log;
        private readonly UsageSkillsConfig config;
        private readonly RunStateAdapter runState;
        private readonly ProgressionService progression;
        private readonly EffectService effects;
        private readonly Dictionary<string, StatusAggregate> pendingStatuses = new Dictionary<string, StatusAggregate>(StringComparer.Ordinal);
        private float lastSnapshotTime;

        public DiagnosticHub(
            ManualLogSource log,
            UsageSkillsConfig config,
            RunStateAdapter runState,
            ProgressionService progression,
            EffectService effects)
        {
            this.log = log;
            this.config = config;
            this.runState = runState;
            this.progression = progression;
            this.effects = effects;
        }

        public RuntimeMetrics Metrics { get; } = new RuntimeMetrics();

        public void RecordAward(SkillId skillId, string source, double work, double experience)
        {
            SessionSkillMetrics skill = Metrics.GetSkill(skillId);
            skill.Work += work;
            skill.Experience += experience;
            skill.Awards++;
            skill.LastSource = source;
        }

        public void RecordRejected(SkillId skillId, string reason, double work)
        {
            SessionSkillMetrics skill = Metrics.GetSkill(skillId);
            skill.Rejections++;
            skill.LastRejection = reason;
        }

        public void RecordStaminaRequest(float raw, float effective, string source)
        {
            Metrics.RawStaminaRequest = raw;
            Metrics.EffectiveStaminaRequest = effective;
            Metrics.StaminaSource = source;
        }

        public void RecordWeight(float raw, float effective)
        {
            if (Math.Abs(Metrics.RawWeight - raw) >= 0.0001f || Math.Abs(Metrics.EffectiveWeight - effective) >= 0.0001f)
            {
                int overflow = InventorySkillService.GetOverflowItemCount(Character.localCharacter);
                log.LogInfo(
                    $"[UsageSkills:Weight] raw={raw:F4} effective={effective:F4} "
                    + $"strengthMultiplier={effects.StrengthWeightMultiplier:F4} overflowItems={overflow} "
                    + $"packRatWeightMultiplier={effects.PackRatWeightMultiplier(overflow):F4}");
            }

            Metrics.RawWeight = raw;
            Metrics.EffectiveWeight = effective;
        }

        public void RecordFall(float raw, float effective, string source)
        {
            Metrics.LastRawFallInjury = raw;
            Metrics.LastEffectiveFallInjury = effective;
            log.LogInfo($"[UsageSkills:Fall] source={source} rawInjury={raw:F4} effectiveInjury={effective:F4} resilienceMultiplier={effects.ResilienceFallMultiplier:F4}");
        }

        public void RecordStatusChange(string status, float requested, float actual, float latestValue, string source)
        {
            if (!config.LogStatusChanges.Value || Math.Abs(actual) < 0.0001f)
            {
                return;
            }

            string key = status + ":" + source;
            if (!pendingStatuses.TryGetValue(key, out StatusAggregate aggregate))
            {
                aggregate = new StatusAggregate();
                pendingStatuses.Add(key, aggregate);
            }

            aggregate.Events++;
            aggregate.Requested += requested;
            aggregate.Actual += actual;
            aggregate.LatestValue = latestValue;
            aggregate.LatestSource = source;
        }

        public void RecordBar(float unitWidth, float backingWidth, float outlineWidth)
        {
            Metrics.StaminaUnitWidth = unitWidth;
            Metrics.StaminaBackingWidth = backingWidth;
            Metrics.StaminaOutlineWidth = outlineWidth;
        }

        public void RecordPatchHealth(string adapter, bool healthy, string hook)
        {
            string status = healthy ? "Healthy" : "Missing";
            Metrics.PatchHealth[adapter] = status + " — " + hook;
            if (healthy)
            {
                log.LogInfo($"[UsageSkills:Patch] {adapter}=Healthy hook={hook}");
            }
            else
            {
                log.LogError($"[UsageSkills:Patch] {adapter}=Missing hook={hook}");
            }
        }

        public void RecordSave()
        {
            Metrics.SaveWrites++;
        }

        public void Tick()
        {
            if (!config.AutomaticDiagnostics.Value)
            {
                return;
            }

            float interval = Math.Max(0.5f, config.DiagnosticIntervalSeconds.Value);
            if (Time.unscaledTime - lastSnapshotTime < interval)
            {
                return;
            }

            lastSnapshotTime = Time.unscaledTime;
            FlushStatusAggregates();
            LogSnapshot();
        }

        public string BuildOverlayText()
        {
            StringBuilder builder = new StringBuilder(1200);
            Character character = Character.localCharacter;
            builder.Append(Plugin.PluginName).Append(' ').Append(Plugin.PluginVersion).Append("-dev\n");
            builder.Append("Scene: ").Append(runState.ActiveSceneName)
                .Append("  XP eligible: ").Append(runState.IsExperienceEligible ? "YES" : "NO").Append('\n');

            if (character != null)
            {
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Stamina: {0:F3} / {1:F3}  Extra: {2:F3}  Cost: {3:F4}->{4:F4} ({5})\n",
                    character.data.currentStamina,
                    character.GetMaxStamina(),
                    character.data.extraStamina,
                    Metrics.RawStaminaRequest,
                    Metrics.EffectiveStaminaRequest,
                    Metrics.StaminaSource);
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Bar px: unit {0:F1}  backing {1:F1}  outline {2:F1}\n",
                    Metrics.StaminaUnitWidth,
                    Metrics.StaminaBackingWidth,
                    Metrics.StaminaOutlineWidth);
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Weight: {0:F3}->{1:F3}  Move: {2:F3}m  Wall/Rope/Vine +Y: {3:F3}/{4:F3}/{5:F3}m\n",
                    Metrics.RawWeight,
                    Metrics.EffectiveWeight,
                    Metrics.LastPhysicalMovementDistance,
                    Metrics.LastWallVerticalDelta,
                    Metrics.LastRopeVerticalDelta,
                    Metrics.LastVineVerticalDelta);
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Fall injury: {0:F3}->{1:F3}  Grounded:{2} Sprint:{3} Wall:{4} Rope:{5} Vine:{6}\n",
                    Metrics.LastRawFallInjury,
                    Metrics.LastEffectiveFallInjury,
                    character.data.isGrounded,
                    character.data.isSprinting,
                    character.data.isClimbing,
                    character.data.isRopeClimbing,
                    character.data.isVineClimbing);
                builder.Append("Statuses: ").Append(BuildStatusSummary(character)).Append('\n');
                builder.Append("Timed effects: ").Append(BuildAfflictionSummary(character)).Append('\n');
            }

            foreach (SkillId skillId in Enum.GetValues(typeof(SkillId)))
            {
                SessionSkillMetrics session = Metrics.GetSkill(skillId);
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0}: L{1} ({2:F1}/{3:F1})  work {4:F2}  XP +{5:F2}\n",
                    skillId,
                    effects.GetEffectiveLevel(skillId),
                    progression.GetExperience(skillId),
                    progression.GetExperienceToNextLevel(skillId),
                    session.Work,
                    session.Experience);
            }

            builder.Append("Patch health: ");
            builder.Append(string.Join(", ", Metrics.PatchHealth.Select(pair => pair.Key + "=" + pair.Value.Split(' ')[0])));
            return builder.ToString();
        }

        private void FlushStatusAggregates()
        {
            foreach (KeyValuePair<string, StatusAggregate> pair in pendingStatuses)
            {
                StatusAggregate aggregate = pair.Value;
                log.LogInfo(
                    $"[UsageSkills:Status] key={pair.Key} events={aggregate.Events} requested={aggregate.Requested:F4} actual={aggregate.Actual:F4} current={aggregate.LatestValue:F4}");
            }

            pendingStatuses.Clear();
        }

        private void LogSnapshot()
        {
            Character character = Character.localCharacter;
            if (character == null)
            {
                log.LogInfo($"[UsageSkills:Snapshot] scene={runState.ActiveSceneName} xpEligible={runState.IsExperienceEligible} localCharacter=false");
                return;
            }

            log.LogInfo(
                $"[UsageSkills:Snapshot] scene={runState.ActiveSceneName} xpEligible={runState.IsExperienceEligible} "
                + $"stamina={character.data.currentStamina:F4}/{character.GetMaxStamina():F4} extra={character.data.extraStamina:F4} "
                + $"weight={Metrics.RawWeight:F4}->{Metrics.EffectiveWeight:F4} "
                + $"overflow={InventorySkillService.GetOverflowItemCount(character)} "
                + $"states=ground:{character.data.isGrounded},sprint:{character.data.isSprinting},wall:{character.data.isClimbing},rope:{character.data.isRopeClimbing},vine:{character.data.isVineClimbing} "
                + $"movement={Metrics.LastPhysicalMovementDistance:F4} wallDeltaY={Metrics.LastWallVerticalDelta:F4} "
                + $"ropeDeltaY={Metrics.LastRopeVerticalDelta:F4} vineDeltaY={Metrics.LastVineVerticalDelta:F4} "
                + $"work=[{BuildWorkSummary()}] statuses=[{BuildStatusSummary(character)}] afflictions=[{BuildAfflictionSummary(character)}]");
        }

        private string BuildWorkSummary()
        {
            List<string> values = new List<string>();
            foreach (SkillId skillId in Enum.GetValues(typeof(SkillId)))
            {
                SessionSkillMetrics metrics = Metrics.GetSkill(skillId);
                if (metrics.Awards <= 0)
                {
                    continue;
                }

                values.Add(
                    skillId
                    + ":w=" + metrics.Work.ToString("F3", CultureInfo.InvariantCulture)
                    + ",xp=" + metrics.Experience.ToString("F2", CultureInfo.InvariantCulture)
                    + ",n=" + metrics.Awards
                    + ",src=" + metrics.LastSource);
            }

            return values.Count == 0 ? "none" : string.Join(";", values);
        }

        private static string BuildStatusSummary(Character character)
        {
            if (character.refs == null || character.refs.afflictions == null)
            {
                return "unavailable";
            }

            List<string> values = new List<string>();
            foreach (CharacterAfflictions.STATUSTYPE type in Enum.GetValues(typeof(CharacterAfflictions.STATUSTYPE)))
            {
                float value = character.refs.afflictions.GetCurrentStatus(type);
                if (value > 0.0001f)
                {
                    values.Add(type + "=" + value.ToString("F3", CultureInfo.InvariantCulture));
                }
            }

            return values.Count == 0 ? "none" : string.Join(",", values);
        }

        private static string BuildAfflictionSummary(Character character)
        {
            if (character.refs == null || character.refs.afflictions == null || character.refs.afflictions.afflictionList == null)
            {
                return "unavailable";
            }

            List<string> values = new List<string>();
            foreach (Peak.Afflictions.Affliction affliction in character.refs.afflictions.afflictionList)
            {
                if (affliction == null)
                {
                    continue;
                }

                values.Add(
                    affliction.GetAfflictionType()
                    + "="
                    + affliction.timeElapsed.ToString("F1", CultureInfo.InvariantCulture)
                    + "/"
                    + affliction.totalTime.ToString("F1", CultureInfo.InvariantCulture));
            }

            return values.Count == 0 ? "none" : string.Join(",", values);
        }
    }
}
