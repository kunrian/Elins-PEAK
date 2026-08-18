using System;
using System.Collections.Generic;

namespace PEAKUsageSkills.Persistence
{
    internal sealed class ProgressionSave
    {
        public int SchemaVersion { get; set; } = 1;
        public DateTime LastSavedUtc { get; set; } = DateTime.UtcNow;
        public Dictionary<string, SkillSave> Skills { get; set; } = new Dictionary<string, SkillSave>(StringComparer.Ordinal);
    }

    internal sealed class SkillSave
    {
        public int Level { get; set; } = 1;
        public double Experience { get; set; }
        public double LifetimeWork { get; set; }
    }
}
