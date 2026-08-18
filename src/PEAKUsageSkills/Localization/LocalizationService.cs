using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using PEAKLib.UI;
using PEAKUsageSkills.Core;

namespace PEAKUsageSkills.Localization
{
    internal static class LocalizationService
    {
        private const string TranslationIndexPrefix = "ELINS_PEAK_";

        private static readonly Dictionary<string, string> English =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "section.main", "MAIN SKILLS" },
                { "section.resiliency", "RESILIENCY" },
                { "level.abbreviation", "Lv." },

                { "skill.strength", "Strength" },
                { "skill.endurance", "Endurance" },
                { "skill.wall_climbing", "Wall Climbing" },
                { "skill.rope_climbing", "Rope Climbing" },
                { "skill.vine_climbing", "Vine Climbing" },
                { "skill.athletics", "Athletics" },
                { "skill.agility", "Agility" },
                { "skill.vitality", "Vitality" },
                { "skill.wet_grip", "Wet Grip" },
                { "skill.climbing_tenacity", "Climbing Tenacity" },
                { "skill.poison", "Poison" },
                { "skill.cold", "Cold" },
                { "skill.heat", "Heat" },
                { "skill.drowsy", "Drowsy" },
                { "skill.spores", "Spores" },
                { "skill.hunger", "Hunger" },
                { "skill.curse", "Curse" },
                { "skill.petrification", "Petrification" },

                { "term.wall", "wall" },
                { "term.rope", "rope" },
                { "term.vine", "vine" },
                { "term.fall_injury", "fall Injury" },
                { "term.wet_grip_penalty", "rain/slippery climbing pull and wind stamina drain" },
                { "term.tenacity_penalty", "climbing control, slide, and stamina penalties below 20% stamina" },

                { "tooltip.strength", "Reduces carried Weight and adds item slots to Backpacks, Fanny Packs, and Jet Packs at levels 20/40/70/120/200. Current: {0}% less Weight, +{1} slots." },
                { "tooltip.endurance", "Adds 0.5% base stamina and 0.1% regeneration per level. Current: +{0} stamina, +{1}% regeneration." },
                { "tooltip.vine", "Improves vine speed by 0.3% per level, reduces stamina cost, and retains light slide momentum. Current: +{0}% speed, {1}% less cost." },
                { "tooltip.athletics", "Improves ground force by 0.1% and sprint force by another 0.2% per level. Current sprint cost reduction: {0}%." },
                { "tooltip.agility", "Improves jump impulse and very lightly improves air control. Current: +{0}% jump impulse, {1}% less jump cost." },
                { "tooltip.climbing", "Improves {0} climbing speed by {1}% per level and reduces stamina cost. Current: +{2}% speed, {3}% less cost." },
                { "tooltip.reduction", "Reduces {0} using the {1}% anchored curve. Current bonus: {2}% reduction." },
                { "tooltip.tolerance", "Reduces incoming {0}. Current reduction: {1}%.{2} XP comes only from receiving the affliction." },
                { "tooltip.natural_recovery", " Natural recovery: +{0}%." }
            };

        private static readonly Dictionary<SkillId, string> SkillKeys =
            new Dictionary<SkillId, string>
            {
                { SkillId.Strength, "skill.strength" },
                { SkillId.Endurance, "skill.endurance" },
                { SkillId.WallClimbing, "skill.wall_climbing" },
                { SkillId.RopeClimbing, "skill.rope_climbing" },
                { SkillId.VineClimbing, "skill.vine_climbing" },
                { SkillId.Athletics, "skill.athletics" },
                { SkillId.Agility, "skill.agility" },
                { SkillId.Vitality, "skill.vitality" },
                { SkillId.WetGrip, "skill.wet_grip" },
                { SkillId.ClimbingTenacity, "skill.climbing_tenacity" },
                { SkillId.Toxicology, "skill.poison" },
                { SkillId.ColdTolerance, "skill.cold" },
                { SkillId.HeatTolerance, "skill.heat" },
                { SkillId.DrowsyTolerance, "skill.drowsy" },
                { SkillId.SporeTolerance, "skill.spores" },
                { SkillId.HungerTolerance, "skill.hunger" },
                { SkillId.CurseTolerance, "skill.curse" },
                { SkillId.PetrificationResistance, "skill.petrification" }
            };

        private static readonly LocaleDefinition[] Locales =
        {
            new LocaleDefinition("fr.json", LocalizedText.Language.French),
            new LocaleDefinition("de.json", LocalizedText.Language.German),
            new LocaleDefinition("es.json", LocalizedText.Language.SpanishSpain, LocalizedText.Language.SpanishLatam),
            new LocaleDefinition("zh-CN.json", LocalizedText.Language.SimplifiedChinese),
            new LocaleDefinition("ja.json", LocalizedText.Language.Japanese),
            new LocaleDefinition("ko.json", LocalizedText.Language.Korean)
        };

        private static bool initialized;
        private static bool nativeRegistrationAvailable;

        public static event Action? LanguageChanged;

        public static string Locale => GetLocaleCode(LocalizedText.CURRENT_LANGUAGE);

        public static void Initialize(string pluginDirectory)
        {
            if (initialized)
            {
                Shutdown();
            }

            try
            {
                RegisterEnglish();
                string localizationDirectory = Path.Combine(pluginDirectory, "Localization");
                foreach (LocaleDefinition locale in Locales)
                {
                    LoadAndRegister(localizationDirectory, locale);
                }

                LocalizedText.OnLangugageChanged += OnGameLanguageChanged;
                nativeRegistrationAvailable = true;
                initialized = true;
                Plugin.ModLog.LogInfo(
                    $"[UsageSkills:Localization] registered={English.Count} locale={Locale} source=PEAK selector");
            }
            catch (Exception exception)
            {
                nativeRegistrationAvailable = false;
                initialized = false;
                Plugin.ModLog.LogWarning(
                    $"[UsageSkills:Localization] PEAK localization registration failed; using English. " +
                    $"{exception.GetType().Name}: {exception.Message}");
            }
        }

        public static void Shutdown()
        {
            if (!initialized)
            {
                return;
            }

            LocalizedText.OnLangugageChanged -= OnGameLanguageChanged;
            LanguageChanged = null;
            nativeRegistrationAvailable = false;
            initialized = false;
        }

        public static string Get(string key, params object[] args)
        {
            string value = "";
            if (nativeRegistrationAvailable)
            {
                try
                {
                    value = LocalizedText.GetText(GetTranslationIndex(key), false);
                }
                catch (Exception exception)
                {
                    Plugin.ModLog.LogWarning(
                        $"[UsageSkills:Localization] lookup failed key={key}; using English. " +
                        $"{exception.GetType().Name}: {exception.Message}");
                }
            }

            if (string.IsNullOrEmpty(value) && !English.TryGetValue(key, out value!))
            {
                value = key;
            }

            if (args == null || args.Length == 0)
            {
                return value;
            }

            try
            {
                return string.Format(GetFormattingCulture(), value, args);
            }
            catch (FormatException exception)
            {
                Plugin.ModLog.LogWarning(
                    $"[UsageSkills:Localization] invalid format key={key} locale={Locale}; using English. {exception.Message}");
                return English.TryGetValue(key, out string fallback)
                    ? string.Format(GetFormattingCulture(), fallback, args)
                    : value;
            }
        }

        public static string FormatNumber(float value, int decimalPlaces)
        {
            return value.ToString("F" + decimalPlaces, GetFormattingCulture());
        }

        public static string SkillName(SkillId skillId)
        {
            return SkillKeys.TryGetValue(skillId, out string key) ? Get(key) : skillId.ToString();
        }

        private static void RegisterEnglish()
        {
            foreach (KeyValuePair<string, string> translation in English)
            {
                MenuAPI.CreateLocalization(GetTranslationIndex(translation.Key))
                    .AddLocalization(translation.Value, LocalizedText.Language.English);
            }
        }

        private static void LoadAndRegister(string localizationDirectory, LocaleDefinition locale)
        {
            string path = Path.Combine(localizationDirectory, locale.FileName);
            try
            {
                if (!File.Exists(path))
                {
                    Plugin.ModLog.LogWarning(
                        $"[UsageSkills:Localization] missing={locale.FileName}; affected languages use English");
                    return;
                }

                Dictionary<string, string>? loaded =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
                if (loaded == null || loaded.Count == 0)
                {
                    Plugin.ModLog.LogWarning(
                        $"[UsageSkills:Localization] empty={locale.FileName}; affected languages use English");
                    return;
                }

                int registered = 0;
                int missing = 0;
                foreach (string key in English.Keys)
                {
                    if (!loaded.TryGetValue(key, out string translation)
                        || string.IsNullOrWhiteSpace(translation))
                    {
                        missing++;
                        continue;
                    }

                    foreach (LocalizedText.Language language in locale.Languages)
                    {
                        MenuAPI.CreateLocalization(GetTranslationIndex(key))
                            .AddLocalization(translation, language);
                    }

                    registered++;
                }

                Plugin.ModLog.LogInfo(
                    $"[UsageSkills:Localization] file={locale.FileName} registered={registered} missing={missing}");
            }
            catch (Exception exception)
            {
                Plugin.ModLog.LogWarning(
                    $"[UsageSkills:Localization] failed={locale.FileName}; affected languages use English. " +
                    $"{exception.GetType().Name}: {exception.Message}");
            }
        }

        private static void OnGameLanguageChanged()
        {
            Plugin.ModLog.LogInfo($"[UsageSkills:Localization] changed locale={Locale}");
            try
            {
                LanguageChanged?.Invoke();
            }
            catch (Exception exception)
            {
                Plugin.ModLog.LogWarning(
                    $"[UsageSkills:Localization] UI refresh failed after language change. " +
                    $"{exception.GetType().Name}: {exception.Message}");
            }
        }

        private static string GetTranslationIndex(string key)
        {
            return TranslationIndexPrefix + key.Replace('.', '_').ToUpperInvariant();
        }

        private static CultureInfo GetFormattingCulture()
        {
            switch (LocalizedText.CURRENT_LANGUAGE)
            {
                case LocalizedText.Language.French:
                    return CultureInfo.GetCultureInfo("fr-FR");
                case LocalizedText.Language.German:
                    return CultureInfo.GetCultureInfo("de-DE");
                case LocalizedText.Language.SpanishLatam:
                    return CultureInfo.GetCultureInfo("es-MX");
                case LocalizedText.Language.SpanishSpain:
                    return CultureInfo.GetCultureInfo("es-ES");
                case LocalizedText.Language.SimplifiedChinese:
                    return CultureInfo.GetCultureInfo("zh-CN");
                case LocalizedText.Language.Japanese:
                    return CultureInfo.GetCultureInfo("ja-JP");
                case LocalizedText.Language.Korean:
                    return CultureInfo.GetCultureInfo("ko-KR");
                default:
                    return CultureInfo.GetCultureInfo("en-US");
            }
        }

        private static string GetLocaleCode(LocalizedText.Language language)
        {
            switch (language)
            {
                case LocalizedText.Language.French:
                    return "fr";
                case LocalizedText.Language.Italian:
                    return "it";
                case LocalizedText.Language.German:
                    return "de";
                case LocalizedText.Language.SpanishSpain:
                    return "es-ES";
                case LocalizedText.Language.SpanishLatam:
                    return "es-419";
                case LocalizedText.Language.BRPortuguese:
                    return "pt-BR";
                case LocalizedText.Language.Russian:
                    return "ru";
                case LocalizedText.Language.Ukrainian:
                    return "uk";
                case LocalizedText.Language.SimplifiedChinese:
                    return "zh-CN";
                case LocalizedText.Language.TraditionalChinese:
                    return "zh-TW";
                case LocalizedText.Language.Japanese:
                    return "ja";
                case LocalizedText.Language.Korean:
                    return "ko";
                case LocalizedText.Language.Polish:
                    return "pl";
                case LocalizedText.Language.Turkish:
                    return "tr";
                default:
                    return "en";
            }
        }

        private sealed class LocaleDefinition
        {
            public LocaleDefinition(string fileName, params LocalizedText.Language[] languages)
            {
                FileName = fileName;
                Languages = languages;
            }

            public string FileName { get; }
            public LocalizedText.Language[] Languages { get; }
        }
    }
}
