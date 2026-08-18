using System;
using System.Collections.Generic;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using PEAKUsageSkills.Core;
using PEAKUsageSkills.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PEAKUsageSkills.UI
{
    internal static class PauseMenuIntegration
    {
        private const string RootObjectName = "UI_PEAKUsageSkills_ReleasePanels";
        private const float PanelWidth = 390f;
        private const float HeaderHeight = 56f;
        private const float RowHeight = 32f;
        private static readonly Color MainPanelColor = new Color(0.66f, 0.09f, 0.15f, 1f);
        private static readonly Color ResiliencyPanelColor = new Color(0.08f, 0.30f, 0.68f, 1f);
        private static RectTransform? activeRoot;

        private static readonly SkillId[] MainSkills =
        {
            SkillId.Strength,
            SkillId.Endurance,
            SkillId.WallClimbing,
            SkillId.RopeClimbing,
            SkillId.VineClimbing,
            SkillId.Athletics,
            SkillId.Agility,
            SkillId.Vitality,
            SkillId.WetGrip,
            SkillId.ClimbingTenacity
        };

        private static readonly SkillId[] ResiliencySkills =
        {
            SkillId.Toxicology,
            SkillId.ColdTolerance,
            SkillId.HeatTolerance,
            SkillId.DrowsyTolerance,
            SkillId.SporeTolerance,
            SkillId.HungerTolerance,
            SkillId.CurseTolerance,
            SkillId.PetrificationResistance
        };

        public static void Register()
        {
            LocalizationService.LanguageChanged += OnLanguageChanged;
            MenuAPI.AddToPauseMenu(BuildPauseMenu);
        }

        public static void Unregister()
        {
            LocalizationService.LanguageChanged -= OnLanguageChanged;
            activeRoot = null;
        }

        private static void BuildPauseMenu(Transform parent)
        {
            RemoveLegacyControls(parent);
            Transform existing = parent.Find(RootObjectName);
            if (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }

            GameObject rootObject = new GameObject(RootObjectName, typeof(RectTransform), typeof(SkillPanelRefresher));
            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            activeRoot = root;

            BuildSection(root, "Main", LocalizationService.Get("section.main"), MainSkills, MainPanelColor, new Vector2(24f, -88f), false);
            BuildSection(
                root,
                "Resiliency",
                LocalizationService.Get("section.resiliency"),
                ResiliencySkills,
                ResiliencyPanelColor,
                new Vector2(24f, -88f - SectionHeight(MainSkills.Length) - 18f),
                false);
            SkillTooltip.Create(root);
            Refresh(root);
            Plugin.ModLog.LogInfo("[UsageSkills:UI] release skill panels built; values refresh once when the pause UI opens");
        }

        private static void BuildSection(RectTransform root, string sectionName, string title, SkillId[] skills, Color panelColor, Vector2 offset, bool alignRight)
        {
            PeakMenuButton panel = MenuAPI.CreatePauseMenuButton(title);
            panel.gameObject.name = "UI_PEAKUsageSkills_Section_" + sectionName;
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.SetParent(root, false);
            rect.anchorMin = rect.anchorMax = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.pivot = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(PanelWidth, SectionHeight(skills.Length));

            LayoutElement? layout = panel.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.ignoreLayout = true;
            }

            ConfigurePanelGraphics(panel, panelColor);
            foreach (Button button in panel.GetComponentsInChildren<Button>(true))
            {
                button.enabled = false;
            }

            foreach (TextMeshProUGUI text in panel.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (string.Equals(text.text, title, StringComparison.OrdinalIgnoreCase))
                {
                    text.rectTransform.anchorMin = new Vector2(0f, 1f);
                    text.rectTransform.anchorMax = new Vector2(1f, 1f);
                    text.rectTransform.pivot = new Vector2(0.5f, 1f);
                    text.rectTransform.anchoredPosition = new Vector2(0f, -8f);
                    text.rectTransform.sizeDelta = new Vector2(-20f, 32f);
                    text.fontSize = 26f;
                    break;
                }
            }

            for (int index = 0; index < skills.Length; index++)
            {
                BuildSkillRow(rect, skills[index], index, alignRight);
            }
        }

        private static void BuildSkillRow(RectTransform panel, SkillId skillId, int index, bool openTooltipLeft)
        {
            GameObject rowObject = new GameObject("UI_PEAKUsageSkills_" + skillId, typeof(RectTransform), typeof(Image), typeof(SkillHoverTarget));
            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.SetParent(panel, false);
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, -HeaderHeight - index * RowHeight);
            rowRect.sizeDelta = new Vector2(-24f, RowHeight);
            rowObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.001f);

            PeakText text = MenuAPI.CreateText(string.Empty, "Text");
            text.transform.SetParent(rowRect, false);
            RectTransform textRect = text.GetComponent<RectTransform>();
            text.SetFontSize(24f);
            Fill(textRect, Vector2.zero, Vector2.zero);
            text.TextMesh.alignment = TextAlignmentOptions.MidlineLeft;
            text.TextMesh.textWrappingMode = TextWrappingModes.NoWrap;
            text.TextMesh.overflowMode = TextOverflowModes.Overflow;
            text.TextMesh.raycastTarget = false;
            text.TextMesh.richText = false;
            SkillHoverTarget hover = rowObject.GetComponent<SkillHoverTarget>();
            hover.Skill = skillId;
            hover.OpenTooltipLeft = openTooltipLeft;
        }

        internal static void Refresh(RectTransform root)
        {
            RefreshSectionTitle(root, "Main", "section.main");
            RefreshSectionTitle(root, "Resiliency", "section.resiliency");
            foreach (SkillId skillId in Enum.GetValues(typeof(SkillId)))
            {
                Transform? row = FindDeep(root, "UI_PEAKUsageSkills_" + skillId);
                PeakText? rowText = row?.Find("Text")?.GetComponent<PeakText>();
                if (rowText != null)
                {
                    rowText.SetText(GetSkillText(skillId));
                    Fill(rowText.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
                }
            }
        }

        private static void RefreshSectionTitle(RectTransform root, string sectionName, string localizationKey)
        {
            PeakMenuButton? panel = FindDeep(root, "UI_PEAKUsageSkills_Section_" + sectionName)?.GetComponent<PeakMenuButton>();
            panel?.SetText(LocalizationService.Get(localizationKey));
        }

        private static void OnLanguageChanged()
        {
            SkillTooltip.Hide();
            if (activeRoot != null)
            {
                Refresh(activeRoot);
            }
        }

        internal static string GetTooltipText(SkillId skillId)
        {
            int level = Plugin.Progression.GetLevel(skillId);
            float reduction;
            switch (skillId)
            {
                case SkillId.Strength:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.StrengthReductionPerLevel.Value)) * 100f;
                    return LocalizationService.Get(
                        "tooltip.strength",
                        LocalizationService.FormatNumber(reduction, 1),
                        GameAdapters.InventorySkillService.ExtraBackpackSlots);
                case SkillId.Endurance:
                    return LocalizationService.Get(
                        "tooltip.endurance",
                        LocalizationService.FormatNumber(level * 0.5f, 1),
                        LocalizationService.FormatNumber(level * 0.1f, 1));
                case SkillId.WallClimbing:
                    return ClimbingTooltip(LocalizationService.Get("term.wall"), level, Plugin.Settings.WallSpeedPerLevel.Value, Plugin.Settings.WallCostReductionPerLevel.Value);
                case SkillId.RopeClimbing:
                    return ClimbingTooltip(LocalizationService.Get("term.rope"), level, Plugin.Settings.RopeSpeedPerLevel.Value, Plugin.Settings.RopeCostEfficiencyPerLevel.Value);
                case SkillId.VineClimbing:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.VineCostEfficiencyPerLevel.Value)) * 100f;
                    return LocalizationService.Get(
                        "tooltip.vine",
                        LocalizationService.FormatNumber(level * 0.3f, 1),
                        LocalizationService.FormatNumber(reduction, 1));
                case SkillId.Athletics:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.AthleticsSprintEfficiencyPerLevel.Value)) * 100f;
                    return LocalizationService.Get("tooltip.athletics", LocalizationService.FormatNumber(reduction, 1));
                case SkillId.Agility:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.AgilityJumpEfficiencyPerLevel.Value)) * 100f;
                    return LocalizationService.Get(
                        "tooltip.agility",
                        LocalizationService.FormatNumber(level * 0.15f, 1),
                        LocalizationService.FormatNumber(reduction, 1));
                case SkillId.Vitality:
                    return ReductionTooltip(LocalizationService.Get("term.fall_injury"), level, Plugin.Settings.VitalityFallReductionPerLevel.Value);
                case SkillId.WetGrip:
                    return ReductionTooltip(LocalizationService.Get("term.wet_grip_penalty"), level, Plugin.Settings.WetGripReductionPerLevel.Value);
                case SkillId.ClimbingTenacity:
                    return ReductionTooltip(LocalizationService.Get("term.tenacity_penalty"), level, Plugin.Settings.ClimbingTenacityReductionPerLevel.Value);
                case SkillId.Toxicology:
                case SkillId.ColdTolerance:
                case SkillId.HeatTolerance:
                case SkillId.DrowsyTolerance:
                case SkillId.SporeTolerance:
                    return ToleranceTooltip(LocalizationService.SkillName(skillId), level, true);
                case SkillId.HungerTolerance:
                case SkillId.CurseTolerance:
                    return ToleranceTooltip(LocalizationService.SkillName(skillId), level, false);
                case SkillId.PetrificationResistance:
                    return ToleranceTooltip(LocalizationService.SkillName(skillId), level, false);
                default:
                    return LocalizationService.SkillName(skillId);
            }
        }

        private static string ClimbingTooltip(string kind, int level, float speedRate, float costRate)
        {
            float reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, costRate)) * 100f;
            return LocalizationService.Get(
                "tooltip.climbing",
                kind,
                LocalizationService.FormatNumber(speedRate * 100f, 1),
                LocalizationService.FormatNumber(level * speedRate * 100f, 1),
                LocalizationService.FormatNumber(reduction, 1));
        }

        private static string ReductionTooltip(string effect, int level, float rate)
        {
            float reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, rate)) * 100f;
            return LocalizationService.Get(
                "tooltip.reduction",
                effect,
                LocalizationService.FormatNumber(rate * 100f, 2),
                LocalizationService.FormatNumber(reduction, 1));
        }

        private static string ToleranceTooltip(string condition, int level, bool hasNaturalRecovery)
        {
            float reduction = (1f - SkillMath.AnchoredReductionMultiplier(
                level,
                Plugin.Settings.ConditionResistancePerLevel.Value)) * 100f;
            string recovery = hasNaturalRecovery
                ? LocalizationService.Get(
                    "tooltip.natural_recovery",
                    LocalizationService.FormatNumber(level * Plugin.Settings.ConditionRecoveryPerLevel.Value * 100f, 1))
                : "";
            return LocalizationService.Get(
                "tooltip.tolerance",
                condition,
                LocalizationService.FormatNumber(reduction, 1),
                recovery);
        }

        private static string GetSkillText(SkillId skillId)
        {
            int level = Plugin.Progression.GetLevel(skillId);
            int percentage = level >= Plugin.Progression.MaximumLevel
                ? 0
                : SkillMath.ExperienceProgressPercent(Plugin.Progression.GetExperience(skillId), Plugin.Progression.GetExperienceToNextLevel(skillId));
            return $"{LocalizationService.SkillName(skillId)} {LocalizationService.Get("level.abbreviation")} {level:00}.{percentage:00}";
        }

        private static float SectionHeight(int rowCount) => HeaderHeight + rowCount * RowHeight + 16f;

        private static void ConfigurePanelGraphics(PeakMenuButton panel, Color color)
        {
            panel.SetColor(color, true);
            Stretch(panel.Panel.rectTransform);
            panel.Panel.raycastTarget = false;
            panel.Panel.rectTransform.SetAsFirstSibling();

            ConfigureBorder(panel.BorderTop.rectTransform, true);
            ConfigureBorder(panel.BorderBottom.rectTransform, false);
            panel.BorderTop.raycastTarget = false;
            panel.BorderBottom.raycastTarget = false;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static void Fill(RectTransform rect, Vector2 minimumInset, Vector2 maximumInset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = minimumInset;
            rect.offsetMax = maximumInset;
        }

        private static void ConfigureBorder(RectTransform border, bool top)
        {
            float height = Mathf.Max(3f, border.rect.height);
            border.anchorMin = new Vector2(0f, top ? 1f : 0f);
            border.anchorMax = new Vector2(1f, top ? 1f : 0f);
            border.pivot = new Vector2(0.5f, top ? 1f : 0f);
            border.anchoredPosition = Vector2.zero;
            border.sizeDelta = new Vector2(0f, height);
        }

        internal static string WrapTooltipText(string message, int maximumLineLength = 58)
        {
            if (ContainsCjk(message))
            {
                int maximumColumns = Math.Max(32, maximumLineLength - 14);
                System.Text.StringBuilder localized = new System.Text.StringBuilder(message.Length + 8);
                int columns = 0;
                foreach (char character in message)
                {
                    if (character == '\n')
                    {
                        localized.Append(character);
                        columns = 0;
                        continue;
                    }

                    int width = IsCjk(character) ? 2 : 1;
                    if (columns > 0 && columns + width > maximumColumns)
                    {
                        localized.Append('\n');
                        columns = 0;
                        if (char.IsWhiteSpace(character))
                        {
                            continue;
                        }
                    }

                    localized.Append(character);
                    columns += width;
                }

                return localized.ToString();
            }

            string[] words = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            System.Text.StringBuilder builder = new System.Text.StringBuilder(message.Length + 8);
            int lineLength = 0;
            foreach (string word in words)
            {
                if (lineLength > 0 && lineLength + 1 + word.Length > maximumLineLength)
                {
                    builder.Append('\n');
                    lineLength = 0;
                }

                if (lineLength > 0)
                {
                    builder.Append(' ');
                    lineLength++;
                }

                builder.Append(word);
                lineLength += word.Length;
            }

            return builder.ToString();
        }

        private static bool ContainsCjk(string value)
        {
            foreach (char character in value)
            {
                if (IsCjk(character))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCjk(char character)
        {
            return (character >= '\u3040' && character <= '\u30ff')
                || (character >= '\u3400' && character <= '\u9fff')
                || (character >= '\uac00' && character <= '\ud7af');
        }

        private static void RemoveLegacyControls(Transform parent)
        {
            foreach (string name in new[] { "UI_MainMenuButton_PEAKUsageSkills", "UI_MainMenuButton_PEAKUsageSkillsTestLevel", "UI_MainMenuButton_PEAKUsageSkillsReset" })
            {
                Transform existing = parent.Find(name);
                if (existing != null)
                {
                    UnityEngine.Object.Destroy(existing.gameObject);
                }
            }

            List<GameObject> statusButtons = new List<GameObject>();
            foreach (Transform child in parent)
            {
                if (child.name.StartsWith("UI_MainMenuButton_PEAKUsageSkillsStatus_", StringComparison.Ordinal))
                {
                    statusButtons.Add(child.gameObject);
                }
            }

            foreach (GameObject statusButton in statusButtons)
            {
                UnityEngine.Object.Destroy(statusButton);
            }
        }

        private static Transform? FindDeep(Transform root, string objectName)
        {
            foreach (Transform child in root)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform? nested = FindDeep(child, objectName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }

    internal sealed class SkillPanelRefresher : MonoBehaviour
    {
        private void OnEnable() => PauseMenuIntegration.Refresh((RectTransform)transform);
    }

    internal sealed class SkillHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public SkillId Skill { get; set; }
        public bool OpenTooltipLeft { get; set; }
        public void OnPointerEnter(PointerEventData eventData) => SkillTooltip.Show((RectTransform)transform, PauseMenuIntegration.GetTooltipText(Skill), OpenTooltipLeft);
        public void OnPointerExit(PointerEventData eventData) => SkillTooltip.Hide();
    }

    internal sealed class SkillTooltip : MonoBehaviour
    {
        private static SkillTooltip? instance;
        private RectTransform rect = null!;
        private PeakText text = null!;

        public static void Create(RectTransform parent)
        {
            GameObject bubble = new GameObject("UI_PEAKUsageSkills_Tooltip", typeof(RectTransform), typeof(Image), typeof(SkillTooltip));
            RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
            bubbleRect.SetParent(parent, false);
            bubbleRect.anchorMin = bubbleRect.anchorMax = new Vector2(0.5f, 0.5f);
            bubbleRect.pivot = new Vector2(0f, 0.5f);
            bubbleRect.sizeDelta = new Vector2(500f, 124f);
            bubble.GetComponent<Image>().color = new Color(0.035f, 0.035f, 0.04f, 0.94f);

            PeakText tooltipText = MenuAPI.CreateText(string.Empty, "Text");
            tooltipText.transform.SetParent(bubbleRect, false);
            RectTransform textRect = tooltipText.GetComponent<RectTransform>();
            tooltipText.SetFontSize(21f);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 8f);
            textRect.offsetMax = new Vector2(-12f, -8f);
            tooltipText.TextMesh.alignment = TextAlignmentOptions.MidlineLeft;
            tooltipText.TextMesh.textWrappingMode = TextWrappingModes.Normal;
            tooltipText.TextMesh.overflowMode = TextOverflowModes.Truncate;
            tooltipText.TextMesh.raycastTarget = false;

            SkillTooltip component = bubble.GetComponent<SkillTooltip>();
            component.rect = bubbleRect;
            component.text = tooltipText;
            instance = component;
            bubble.SetActive(false);
        }

        public static void Show(RectTransform target, string message, bool openLeft)
        {
            if (instance == null)
            {
                return;
            }

            instance.gameObject.SetActive(true);
            instance.text.SetText(PauseMenuIntegration.WrapTooltipText(message));
            RectTransform textRect = instance.text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 8f);
            textRect.offsetMax = new Vector2(-12f, -8f);
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            instance.rect.pivot = new Vector2(openLeft ? 1f : 0f, 0.5f);
            instance.rect.position = (openLeft ? corners[1] : corners[2]) + Vector3.right * (openLeft ? -16f : 16f);
            instance.rect.SetAsLastSibling();
        }

        public static void Hide()
        {
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
            }
        }
    }
}
