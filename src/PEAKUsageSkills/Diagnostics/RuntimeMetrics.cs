using System;
using System.Collections.Generic;
using PEAKUsageSkills.Core;

namespace PEAKUsageSkills.Diagnostics
{
    internal sealed class RuntimeMetrics
    {
        public float RawWeight { get; set; }
        public float EffectiveWeight { get; set; }
        public float RawStaminaRequest { get; set; }
        public float EffectiveStaminaRequest { get; set; }
        public string StaminaSource { get; set; } = "None";
        public float LastPhysicalMovementDistance { get; set; }
        public float LastWallVerticalDelta { get; set; }
        public float LastRopeVerticalDelta { get; set; }
        public float LastVineVerticalDelta { get; set; }
        public double SessionWallVerticalDistance { get; set; }
        public float LastRawFallInjury { get; set; }
        public float LastEffectiveFallInjury { get; set; }
        public float StaminaBackingWidth { get; set; }
        public float StaminaOutlineWidth { get; set; }
        public float StaminaUnitWidth { get; set; }
        public int SaveWrites { get; set; }
        public Dictionary<SkillId, SessionSkillMetrics> Skills { get; } = new Dictionary<SkillId, SessionSkillMetrics>();
        public Dictionary<string, string> PatchHealth { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        public SessionSkillMetrics GetSkill(SkillId skillId)
        {
            if (!Skills.TryGetValue(skillId, out SessionSkillMetrics metrics))
            {
                metrics = new SessionSkillMetrics();
                Skills.Add(skillId, metrics);
            }

            return metrics;
        }
    }

    internal sealed class SessionSkillMetrics
    {
        public double Work { get; set; }
        public double Experience { get; set; }
        public int Awards { get; set; }
        public int Rejections { get; set; }
        public string LastSource { get; set; } = "None";
        public string LastRejection { get; set; } = "None";
    }

    internal sealed class StatusAggregate
    {
        public int Events { get; set; }
        public float Requested { get; set; }
        public float Actual { get; set; }
        public float LatestValue { get; set; }
        public string LatestSource { get; set; } = string.Empty;
    }
}
