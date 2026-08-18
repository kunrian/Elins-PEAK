using System;
using System.Collections.Generic;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using PEAKUsageSkills.Core;
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

        private static readonly Dictionary<SkillId, string> DisplayNames = new Dictionary<SkillId, string>
        {
            { SkillId.Strength, "Strength" },
            { SkillId.Endurance, "Endurance" },
            { SkillId.WallClimbing, "Wall Climbing" },
            { SkillId.RopeClimbing, "Rope Climbing" },
            { SkillId.VineClimbing, "Vine Climbing" },
            { SkillId.Athletics, "Athletics" },
            { SkillId.Agility, "Agility" },
            { SkillId.Vitality, "Vitality" },
            { SkillId.WetGrip, "Wet Grip" },
            { SkillId.ClimbingTenacity, "Climbing Tenacity" },
            { SkillId.Toxicology, "Poison" },
            { SkillId.ColdTolerance, "Cold" },
            { SkillId.HeatTolerance, "Heat" },
            { SkillId.DrowsyTolerance, "Drowsy" },
            { SkillId.SporeTolerance, "Spores" },
            { SkillId.HungerTolerance, "Hunger" },
            { SkillId.CurseTolerance, "Curse" },
            { SkillId.PetrificationResistance, "Petrification" }
        };

        public static void Register()
        {
            MenuAPI.AddToPauseMenu(BuildPauseMenu);
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

            BuildSection(root, "MAIN SKILLS", MainSkills, MainPanelColor, new Vector2(24f, -88f), false);
            BuildSection(
                root,
                "RESILIENCY",
                ResiliencySkills,
                ResiliencyPanelColor,
                new Vector2(24f, -88f - SectionHeight(MainSkills.Length) - 18f),
                false);
            SkillTooltip.Create(root);
            Refresh(root);
            Plugin.ModLog.LogInfo("[UsageSkills:UI] release skill panels built; values refresh once when the pause UI opens");
        }

        private static void BuildSection(RectTransform root, string title, SkillId[] skills, Color panelColor, Vector2 offset, bool alignRight)
        {
            PeakMenuButton panel = MenuAPI.CreatePauseMenuButton(title);
            panel.gameObject.name = "UI_PEAKUsageSkills_Section_" + title.Replace(" ", string.Empty);
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

        internal static string GetTooltipText(SkillId skillId)
        {
            int level = Plugin.Progression.GetLevel(skillId);
            float reduction;
            switch (skillId)
            {
                case SkillId.Strength:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.StrengthReductionPerLevel.Value)) * 100f;
                    return $"Reduces carried Weight and adds item slots to Backpacks, Fanny Packs, and Jet Packs at levels 20/40/70/120/200. Current: {reduction:F1}% less Weight, +{GameAdapters.InventorySkillService.ExtraBackpackSlots} slots.";
                case SkillId.Endurance:
                    return $"Adds 0.5% base stamina and 0.1% regeneration per level. Current: +{level * 0.5f:F1} stamina, +{level * 0.1f:F1}% regeneration.";
                case SkillId.WallClimbing:
                    return ClimbingTooltip("wall", level, Plugin.Settings.WallSpeedPerLevel.Value, Plugin.Settings.WallCostReductionPerLevel.Value);
                case SkillId.RopeClimbing:
                    return ClimbingTooltip("rope", level, Plugin.Settings.RopeSpeedPerLevel.Value, Plugin.Settings.RopeCostEfficiencyPerLevel.Value);
                case SkillId.VineClimbing:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.VineCostEfficiencyPerLevel.Value)) * 100f;
                    return $"Improves vine speed by 0.3% per level, reduces stamina cost, and retains light slide momentum. Current: +{level * 0.3f:F1}% speed, {reduction:F1}% less cost.";
                case SkillId.Athletics:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.AthleticsSprintEfficiencyPerLevel.Value)) * 100f;
                    return $"Improves ground force by 0.1% and sprint force by another 0.2% per level. Current sprint cost reduction: {reduction:F1}%.";
                case SkillId.Agility:
                    reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, Plugin.Settings.AgilityJumpEfficiencyPerLevel.Value)) * 100f;
                    return $"Improves jump impulse and very lightly improves air control. Current: +{level * 0.15f:F1}% jump impulse, {reduction:F1}% less jump cost.";
                case SkillId.Vitality:
                    return ReductionTooltip("fall Injury", level, Plugin.Settings.VitalityFallReductionPerLevel.Value);
                case SkillId.WetGrip:
                    return ReductionTooltip("rain/slippery climbing pull and wind stamina drain", level, Plugin.Settings.WetGripReductionPerLevel.Value);
                case SkillId.ClimbingTenacity:
                    return ReductionTooltip("climbing control, slide, and stamina penalties below 20% stamina", level, Plugin.Settings.ClimbingTenacityReductionPerLevel.Value);
                case SkillId.Toxicology:
                case SkillId.ColdTolerance:
                case SkillId.HeatTolerance:
                case SkillId.DrowsyTolerance:
                case SkillId.SporeTolerance:
                    return ToleranceTooltip(DisplayNames[skillId].ToLowerInvariant(), level, true);
                case SkillId.HungerTolerance:
                case SkillId.CurseTolerance:
                    return ToleranceTooltip(DisplayNames[skillId].ToLowerInvariant(), level, false);
                case SkillId.PetrificationResistance:
                    return ToleranceTooltip("petrification", level, false);
                default:
                    return DisplayNames[skillId];
            }
        }

        private static string ClimbingTooltip(string kind, int level, float speedRate, float costRate)
        {
            float reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, costRate)) * 100f;
            return $"Improves {kind} climbing speed by {speedRate * 100f:F1}% per level and reduces stamina cost. Current: +{level * speedRate * 100f:F1}% speed, {reduction:F1}% less cost.";
        }

        private static string ReductionTooltip(string effect, int level, float rate)
        {
            float reduction = (1f - SkillMath.AnchoredReductionMultiplier(level, rate)) * 100f;
            return $"Reduces {effect} using the {rate * 100f:F2}% anchored curve. Current bonus: {reduction:F1}% reduction.";
        }

        private static string ToleranceTooltip(string condition, int level, bool hasNaturalRecovery)
        {
            float reduction = (1f - SkillMath.AnchoredReductionMultiplier(
                level,
                Plugin.Settings.ConditionResistancePerLevel.Value)) * 100f;
            string recovery = hasNaturalRecovery
                ? $" Natural recovery: +{level * Plugin.Settings.ConditionRecoveryPerLevel.Value * 100f:F1}%."
                : "";
            return $"Reduces incoming {condition}. Current reduction: {reduction:F1}%.{recovery} XP comes only from receiving the affliction.";
        }

        private static string GetSkillText(SkillId skillId)
        {
            int level = Plugin.Progression.GetLevel(skillId);
            int percentage = level >= Plugin.Progression.MaximumLevel
                ? 0
                : SkillMath.ExperienceProgressPercent(Plugin.Progression.GetExperience(skillId), Plugin.Progression.GetExperienceToNextLevel(skillId));
            return $"{DisplayNames[skillId]} Lv. {level:00}.{percentage:00}";
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
