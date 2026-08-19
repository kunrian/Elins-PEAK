using PEAKUsageSkills.Config;
using UnityEngine.SceneManagement;

namespace PEAKUsageSkills.GameAdapters
{
    internal sealed class RunStateAdapter
    {
        private readonly UsageSkillsConfig config;

        public RunStateAdapter(UsageSkillsConfig config)
        {
            this.config = config;
        }

        public string ActiveSceneName => SceneManager.GetActiveScene().name ?? string.Empty;

        public bool IsAirport
        {
            get
            {
                return ActiveSceneName.IndexOf(
                    "airport",
                    System.StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        public bool IsGameplayScene
        {
            get
            {
                return !IsAirport && Character.localCharacter != null;
            }
        }

        public bool IsExperienceEligible
        {
            get
            {
                if (!config.EnableMod.Value
                    || !config.EnableExperience.Value
                    || !IsGameplayScene
                    || Character.localCharacter == null)
                {
                    return false;
                }

                return config.EnableXpInCustomRuns.Value || !RunSettings.IsCustomRun;
            }
        }
    }
}