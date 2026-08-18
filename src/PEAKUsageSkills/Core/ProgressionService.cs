using System;
using System.Collections.Generic;
using BepInEx.Logging;
using PEAKUsageSkills.Config;
using PEAKUsageSkills.Diagnostics;
using PEAKUsageSkills.GameAdapters;
using PEAKUsageSkills.Persistence;
using UnityEngine;

namespace PEAKUsageSkills.Core
{
    internal sealed class ProgressionService
    {
        private readonly UsageSkillsConfig config;
        private readonly SaveStore store;
        private readonly ManualLogSource log;
        private readonly RunStateAdapter runState;
        private ProgressionSave save;
        private bool dirty;
        private float lastSaveTime;

        public ProgressionService(UsageSkillsConfig config, SaveStore store, ManualLogSource log, RunStateAdapter runState)
        {
            this.config = config;
            this.store = store;
            this.log = log;
            this.runState = runState;
            save = store.Load();
            if (save.Skills.Remove("PackRat"))
            {
                dirty = true;
                log.LogInfo("Removed retired Pack Rat progression from the local save.");
            }

            MergeRetiredRecoverySkills();

            EnsureAllSkills();
        }

        public event Action<SkillId, int>? LevelChanged;

        public int GetLevel(SkillId skillId)
        {
            return GetState(skillId).Level;
        }

        public double GetExperience(SkillId skillId)
        {
            return GetState(skillId).Experience;
        }

        public double GetExperienceToNextLevel(SkillId skillId)
        {
            SkillSave state = GetState(skillId);
            return state.Level >= MaximumLevel ? 0d : SkillMath.ExperienceToNextLevel(state.Level);
        }

        public double GetLifetimeWork(SkillId skillId)
        {
            return GetState(skillId).LifetimeWork;
        }

        public int MaximumLevel => Math.Max(1, config.MaximumLevel.Value);

        public void AddLevelsToAll(int levels)
        {
            if (levels <= 0)
            {
                return;
            }

            foreach (SkillId skillId in Enum.GetValues(typeof(SkillId)))
            {
                SkillSave state = GetState(skillId);
                int newLevel = Math.Min(MaximumLevel, state.Level + levels);
                if (newLevel == state.Level)
                {
                    continue;
                }

                state.Level = newLevel;
                if (newLevel >= MaximumLevel)
                {
                    state.Experience = 0d;
                }

                LevelChanged?.Invoke(skillId, newLevel);
            }

            dirty = true;
            Flush();
            log.LogWarning($"Added {levels} saved levels to every usage skill for runtime testing.");
        }

        public void ResetAllProgression()
        {
            foreach (SkillId skillId in Enum.GetValues(typeof(SkillId)))
            {
                SkillSave state = GetState(skillId);
                bool levelChanged = state.Level != 1;
                state.Level = 1;
                state.Experience = 0d;
                state.LifetimeWork = 0d;
                if (levelChanged)
                {
                    LevelChanged?.Invoke(skillId, 1);
                }
            }

            dirty = true;
            Flush();
            log.LogWarning("All saved usage-skill levels, XP, and lifetime work were reset to level 1.00.");
        }

        public bool AwardWork(SkillId skillId, double work, double experiencePerWork, string source)
        {
            if (!runState.IsExperienceEligible)
            {
                Plugin.Diagnostics?.RecordRejected(skillId, "IneligibleScene", work);
                return false;
            }

            if (work <= 0d || experiencePerWork <= 0d || double.IsNaN(work) || double.IsInfinity(work))
            {
                Plugin.Diagnostics?.RecordRejected(skillId, "NonPositiveWork", work);
                return false;
            }

            SkillSave state = GetState(skillId);
            if (state.Level >= MaximumLevel)
            {
                Plugin.Diagnostics?.RecordRejected(skillId, "MaximumLevel", work);
                return false;
            }

            double experience = work * experiencePerWork;
            state.LifetimeWork += work;
            state.Experience += experience;
            dirty = true;

            while (state.Level < MaximumLevel)
            {
                double required = SkillMath.ExperienceToNextLevel(state.Level);
                if (state.Experience + 0.000001d < required)
                {
                    break;
                }

                state.Experience -= required;
                state.Level++;
                log.LogInfo($"{skillId} increased to level {state.Level}.");
                LevelChanged?.Invoke(skillId, state.Level);
            }

            if (state.Level >= MaximumLevel)
            {
                state.Experience = 0d;
            }

            Plugin.Diagnostics?.RecordAward(skillId, source, work, experience);
            return true;
        }

        public void Tick()
        {
            if (!dirty)
            {
                return;
            }

            float interval = Math.Max(5f, config.SaveIntervalSeconds.Value);
            if (Time.unscaledTime - lastSaveTime >= interval)
            {
                Flush();
            }
        }

        public void Flush()
        {
            if (!dirty)
            {
                return;
            }

            try
            {
                store.Save(save);
                lastSaveTime = Time.unscaledTime;
                dirty = false;
                Plugin.Diagnostics?.RecordSave();
            }
            catch (Exception exception)
            {
                log.LogError($"Failed to save usage-skill progression: {exception}");
            }
        }

        private SkillSave GetState(SkillId skillId)
        {
            string key = skillId.ToString();
            if (!save.Skills.TryGetValue(key, out SkillSave state))
            {
                state = new SkillSave();
                save.Skills[key] = state;
                dirty = true;
            }

            state.Level = Math.Max(1, Math.Min(MaximumLevel, state.Level));
            state.Experience = Math.Max(0d, state.Experience);
            state.LifetimeWork = Math.Max(0d, state.LifetimeWork);
            return state;
        }

        private void EnsureAllSkills()
        {
            foreach (SkillId skillId in Enum.GetValues(typeof(SkillId)))
            {
                GetState(skillId);
            }
        }

        private void MergeRetiredRecoverySkills()
        {
            MergeRetiredRecoverySkill("PoisonRecovery", SkillId.Toxicology);
            MergeRetiredRecoverySkill("ColdRecovery", SkillId.ColdTolerance);
            MergeRetiredRecoverySkill("HeatRecovery", SkillId.HeatTolerance);
            MergeRetiredRecoverySkill("DrowsyRecovery", SkillId.DrowsyTolerance);
            MergeRetiredRecoverySkill("SporeRecovery", SkillId.SporeTolerance);
        }

        private void MergeRetiredRecoverySkill(string retiredKey, SkillId toleranceSkill)
        {
            if (!save.Skills.TryGetValue(retiredKey, out SkillSave retired))
            {
                return;
            }

            SkillSave tolerance = GetState(toleranceSkill);
            double combinedExperience = TotalAccumulatedExperience(tolerance)
                + TotalAccumulatedExperience(retired);
            tolerance.LifetimeWork = Math.Max(0d, tolerance.LifetimeWork)
                + Math.Max(0d, retired.LifetimeWork);
            SetFromTotalAccumulatedExperience(tolerance, combinedExperience);
            save.Skills.Remove(retiredKey);
            dirty = true;
            log.LogInfo($"Merged retired {retiredKey} progression into {toleranceSkill}.");
        }

        private double TotalAccumulatedExperience(SkillSave state)
        {
            int level = Math.Max(1, Math.Min(MaximumLevel, state.Level));
            double total = Math.Max(0d, state.Experience);
            for (int completedLevel = 1; completedLevel < level; completedLevel++)
            {
                total += SkillMath.ExperienceToNextLevel(completedLevel);
            }

            return total;
        }

        private void SetFromTotalAccumulatedExperience(SkillSave state, double total)
        {
            state.Level = 1;
            state.Experience = Math.Max(0d, total);
            while (state.Level < MaximumLevel)
            {
                double required = SkillMath.ExperienceToNextLevel(state.Level);
                if (state.Experience + 0.000001d < required)
                {
                    break;
                }

                state.Experience -= required;
                state.Level++;
            }

            if (state.Level >= MaximumLevel)
            {
                state.Experience = 0d;
            }
        }
    }
}
