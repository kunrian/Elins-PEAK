using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PEAKUsageSkills.Config;
using PEAKUsageSkills.Core;
using PEAKUsageSkills.Diagnostics;
using PEAKUsageSkills.Effects;
using PEAKUsageSkills.GameAdapters;
using PEAKUsageSkills.GameAdapters.Patches;
using PEAKUsageSkills.Localization;
using PEAKUsageSkills.Persistence;
using PEAKUsageSkills.Tracking;
using PEAKUsageSkills.UI;
using UnityEngine.SceneManagement;

namespace PEAKUsageSkills
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("PEAK.exe")]
    [BepInDependency("com.github.PEAKModding.PEAKLib.Core", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.github.PEAKModding.PEAKLib.UI", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.chiseled.peak.usageskills";
        public const string PluginName = "Elin's PEAK";
        public const string PluginVersion = "0.4.2";

        private Harmony? harmony;

        internal static UsageSkillsConfig Settings { get; private set; } = null!;
        internal static RunStateAdapter RunState { get; private set; } = null!;
        internal static ProgressionService Progression { get; private set; } = null!;
        internal static EffectService Effects { get; private set; } = null!;
        internal static DiagnosticHub? Diagnostics { get; private set; }
        internal static ManualLogSource ModLog { get; private set; } = null!;

        private void Awake()
        {
            ModLog = Logger;
            Settings = new UsageSkillsConfig(Config);
            string pluginDirectory = Path.GetDirectoryName(Info.Location) ?? Paths.PluginPath;
            LocalizationService.Initialize(pluginDirectory);
            RunState = new RunStateAdapter(Settings);
            SaveStore store = new SaveStore(Logger);
            Progression = new ProgressionService(Settings, store, Logger, RunState);
            Effects = new EffectService(Settings, Progression, RunState);
            Diagnostics = new DiagnosticHub(Logger, Settings, RunState, Progression, Effects);
            Progression.LevelChanged += OnSkillLevelChanged;

            harmony = new Harmony(PluginGuid);
            PatchAllFailSoft(Assembly.GetExecutingAssembly());

            gameObject.AddComponent<ActivitySampler>();
            gameObject.AddComponent<InventorySkillController>();
            gameObject.AddComponent<DebugOverlay>();
            PauseMenuIntegration.Register();
            SceneManager.sceneLoaded += OnSceneLoaded;

            InspectPatchHealth();
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Save levels are player-owned; Airport XP is disabled and anti-farming policy is deferred for the first runtime pass.");
        }

        private void Update()
        {
            Progression.Tick();
            Diagnostics?.Tick();
        }

        private void OnApplicationQuit()
        {
            Progression.Flush();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Progression.LevelChanged -= OnSkillLevelChanged;
            PauseMenuIntegration.Unregister();
            LocalizationService.Shutdown();
            Progression.Flush();
            harmony?.UnpatchSelf();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Progression.Flush();
            ModLog.LogInfo($"[UsageSkills:Scene] loaded={scene.name} mode={mode} xpEligible={RunState.IsExperienceEligible}");
        }

        private static void OnSkillLevelChanged(SkillId skillId, int level)
        {
            if (skillId == SkillId.Strength && Settings.DebugAllSkillLevelOverride.Value < 0)
            {
                WeightPatch.RefreshLocalWeight($"StrengthLevel:{level}");
                BackpackData? data = InventorySkillService.TryGetEquippedBackpackData(
                    Character.localCharacter,
                    out BackpackSlot.BackpackType backpackType);
                InventorySkillService.EnsureBackpackCapacity(
                    data,
                    backpackType,
                    $"StrengthLevel:{level}");
            }
        }

        private static void InspectPatchHealth()
        {
            RecordHook("StaminaCapacity", typeof(Character), "GetMaxStamina");
            RecordHook("StaminaCost", typeof(Character), "UseStamina");
            RecordHook("BonusSprint", typeof(Character), "OutOfRegularStamina");
            RecordHook("StaminaRegen", typeof(Character), "UpdateVariablesFixed");
            RecordHook("StaminaHud", typeof(StaminaBar), "Update");
            RecordHook("Weight", typeof(CharacterAfflictions), "UpdateWeight");
            RecordHook("WallClimbing", typeof(CharacterClimbing), "GetRequestedPostition");
            RecordHook("RopeClimbing", typeof(CharacterRopeHandling), "Update");
            RecordHook("VineClimbing", typeof(CharacterVineClimbing), "FixedUpdate");
            RecordHook("Athletics", typeof(CharacterMovement), "GetMovementForce");
            RecordHook("Agility", typeof(CharacterMovement), "JumpRpc");
            RecordHook("NormalFalls", typeof(CharacterMovement), "CheckFallDamage");
            RecordHook("WallFalls", typeof(CharacterClimbing), "CheckFallDamage");
            RecordHook("Afflictions", typeof(CharacterAfflictions), "AddStatus");
            RecordHook("ConditionRecovery", typeof(CharacterAfflictions), "SubtractStatus");
            RecordHook("Petrification", typeof(CharacterAfflictions), "AddPetrify", new Type[] { typeof(int) });
            RecordHook("BackpackInventory", typeof(BackpackData), "DeserializeValue");
            RecordHook("BackpackWheel", typeof(BackpackWheel), "InitWheel");
            RecordHook("WetGrip", typeof(WindChillZone), "ApplyStatus");
        }

        private void PatchAllFailSoft(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            Array.Sort(types, (left, right) => string.CompareOrdinal(left.FullName, right.FullName));
            foreach (Type type in types)
            {
                if (type.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0)
                {
                    continue;
                }

                try
                {
                    harmony!.CreateClassProcessor(type).Patch();
                }
                catch (Exception exception)
                {
                    Logger.LogError($"[UsageSkills:Patch] Failed patch class {type.FullName}; its adapter will remain disabled. {exception}");
                }
            }
        }

        private static void RecordHook(string adapter, Type type, string methodName, Type[]? parameterTypes = null)
        {
            MethodInfo? method = parameterTypes == null
                ? AccessTools.Method(type, methodName)
                : AccessTools.Method(type, methodName, parameterTypes);
            bool healthy = method != null && Harmony.GetPatchInfo(method)?.Owners.Contains(PluginGuid) == true;
            string signature = parameterTypes == null
                ? type.FullName + "." + methodName
                : type.FullName + "." + methodName + "(" + string.Join(",", Array.ConvertAll(parameterTypes, value => value.Name)) + ")";
            Diagnostics?.RecordPatchHealth(adapter, healthy, signature);
        }
    }
}
