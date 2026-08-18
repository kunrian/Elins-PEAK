using System;
using PEAKUsageSkills.Core;
using TMPro;
using UnityEngine;

namespace PEAKUsageSkills.GameAdapters
{
    internal static class InventorySkillService
    {
        public const int VanillaMainSlots = 3;
        public const int VanillaBackpackSlots = 4;
        public const int BackpackHotbarSlotId = 3;
        private static Character? cachedBackpackCharacter;
        private static ItemInstanceData? cachedBackpackInstance;
        private static BackpackData? cachedBackpackData;

        public static int ExtraMainSlots
        {
            get
            {
                return SkillMath.ExtraMainInventorySlots(Plugin.Progression.GetLevel(SkillId.PackRat));
            }
        }

        public static int ExtraBackpackSlots
        {
            get
            {
                return SkillMath.ExtraBackpackSlots(Plugin.Progression.GetLevel(SkillId.PackRat));
            }
        }

        public static int DesiredMainArrayLength => ExtraMainSlots == 0
            ? VanillaMainSlots
            : VanillaMainSlots + 1 + ExtraMainSlots;

        public static int DesiredBackpackCapacity => VanillaBackpackSlots + ExtraBackpackSlots;

        public static void EnsureMainCapacity(Player? player, string source)
        {
            if (player == null)
            {
                return;
            }

            int desired = DesiredMainArrayLength;
            ItemSlot[] existing = player.itemSlots ?? Array.Empty<ItemSlot>();
            int required = Math.Max(desired, HighestOccupiedIndex(existing) + 1);
            if (existing.Length < required)
            {
                Array.Resize(ref existing, required);
                player.itemSlots = existing;
                Plugin.ModLog.LogInfo($"[UsageSkills:Inventory] main slots expanded to {required} source={source}");
            }

            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] == null)
                {
                    existing[index] = new ItemSlot((byte)index, player);
                }
            }
        }

        public static void EnsureBackpackCapacity(BackpackData? data, string source)
        {
            if (data == null)
            {
                return;
            }

            ItemSlot[] existing = data.itemSlots ?? Array.Empty<ItemSlot>();
            int required = Math.Max(DesiredBackpackCapacity, HighestOccupiedIndex(existing) + 1);
            if (existing.Length < required)
            {
                Array.Resize(ref existing, required);
                data.itemSlots = existing;
                Plugin.ModLog.LogInfo($"[UsageSkills:Inventory] backpack slots expanded to {required} source={source}");
            }

            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] == null)
                {
                    existing[index] = new ItemSlot((byte)index);
                }
            }
        }

        public static int GetOverflowItemCount(Character? character)
        {
            Player? player = character?.player;
            if (player == null)
            {
                return 0;
            }

            int mainOccupied = 0;
            ItemSlot[] main = player.itemSlots ?? Array.Empty<ItemSlot>();
            for (int index = 0; index < main.Length; index++)
            {
                if (index != BackpackHotbarSlotId && main[index] != null && !main[index].IsEmpty())
                {
                    mainOccupied++;
                }
            }

            int backpackOccupied = 0;
            BackpackData? backpackData = TryGetEquippedBackpackData(character);
            if (backpackData?.itemSlots != null)
            {
                foreach (ItemSlot slot in backpackData.itemSlots)
                {
                    if (slot != null && !slot.IsEmpty())
                    {
                        backpackOccupied++;
                    }
                }
            }

            return SkillMath.OverflowItemCount(mainOccupied, backpackOccupied);
        }

        public static int GetPackRatTrainingLoad(Character? character)
        {
            int overflow = GetOverflowItemCount(character);
            if (overflow > 0 || Plugin.Progression.GetLevel(SkillId.PackRat) >= 10)
            {
                return overflow;
            }

            // The first extra slot unlocks at level 10, so levels 1-9 need a
            // bootstrap workload. A completely full vanilla main inventory
            // counts as one training item, but does not receive overflow penalties.
            ItemSlot[] main = character?.player?.itemSlots ?? Array.Empty<ItemSlot>();
            int occupied = 0;
            for (int index = 0; index < Math.Min(VanillaMainSlots, main.Length); index++)
            {
                if (main[index] != null && !main[index].IsEmpty())
                {
                    occupied++;
                }
            }

            return SkillMath.PackRatTrainingLoad(
                Plugin.Progression.GetLevel(SkillId.PackRat),
                occupied,
                overflow);
        }

        public static BackpackData? TryGetEquippedBackpackData(Character? character)
        {
            if (character?.player?.backpackSlot == null || character.player.backpackSlot.IsEmpty())
            {
                cachedBackpackCharacter = character;
                cachedBackpackInstance = null;
                cachedBackpackData = null;
                return null;
            }

            ItemInstanceData? instanceData = character.player.backpackSlot.data;
            if (character == cachedBackpackCharacter && ReferenceEquals(instanceData, cachedBackpackInstance))
            {
                return cachedBackpackData;
            }

            try
            {
                BackpackReference reference = BackpackReference.GetFromEquippedBackpack(character);
                cachedBackpackCharacter = character;
                cachedBackpackInstance = instanceData;
                cachedBackpackData = reference.exists ? reference.GetData() : null;
                return cachedBackpackData;
            }
            catch (Exception exception)
            {
                Plugin.ModLog.LogWarning($"[UsageSkills:Inventory] backpack lookup failed: {exception.Message}");
                return null;
            }
        }

        public static void EnsureHotbarUI(GUIManager? gui)
        {
            Player? player = Character.localCharacter?.player;
            if (gui == null || player == null || gui.items == null || gui.items.Length == 0)
            {
                return;
            }

            int required = player.itemSlots?.Length ?? VanillaMainSlots;
            if (required <= gui.items.Length)
            {
                return;
            }

            InventoryItemUI[] oldItems = gui.items;
            InventoryItemUI[] expanded = new InventoryItemUI[required];
            Array.Copy(oldItems, expanded, oldItems.Length);
            InventoryItemUI template = oldItems[Math.Min(2, oldItems.Length - 1)];
            Transform parent = template.transform.parent;
            Vector2 step = oldItems.Length >= 2
                ? oldItems[1].rectTransform.anchoredPosition - oldItems[0].rectTransform.anchoredPosition
                : new Vector2(64f, 0f);
            if (step.sqrMagnitude < 1f)
            {
                step = new Vector2(64f, 0f);
            }

            Vector2 backpackPosition = gui.backpack != null
                ? gui.backpack.rectTransform.anchoredPosition
                : template.rectTransform.anchoredPosition + step;
            for (int index = oldItems.Length; index < required; index++)
            {
                GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent);
                clone.name = "UI_InventoryItem_UsageSkills_" + index;
                InventoryItemUI itemUi = clone.GetComponent<InventoryItemUI>();
                expanded[index] = itemUi;
                itemUi.rectTransform.anchoredPosition = index == BackpackHotbarSlotId
                    ? backpackPosition
                    : backpackPosition + step * (index - BackpackHotbarSlotId);
                SetInputLabel(itemUi, (index + 1).ToString());
                clone.SetActive(index != BackpackHotbarSlotId);
            }

            gui.items = expanded;
            Plugin.ModLog.LogInfo($"[UsageSkills:Inventory] hotbar UI expanded to {required} entries");
        }

        private static int HighestOccupiedIndex(ItemSlot[] slots)
        {
            for (int index = slots.Length - 1; index >= 0; index--)
            {
                if (slots[index] != null && !slots[index].IsEmpty())
                {
                    return index;
                }
            }

            return -1;
        }

        private static void SetInputLabel(InventoryItemUI itemUi, string label)
        {
            Transform icon = itemUi.transform.Find("UI_InputIcon");
            TextMeshProUGUI? text = icon?.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.text = label;
            }
        }
    }

    internal sealed class InventorySkillController : MonoBehaviour
    {
        private Player? lastPlayer;
        private int lastMainLength;
        private int lastBackpackCapacity;

        private void Update()
        {
            Player? player = Character.localCharacter?.player;
            int desiredMain = InventorySkillService.DesiredMainArrayLength;
            int desiredBackpack = InventorySkillService.DesiredBackpackCapacity;
            if (player != lastPlayer || desiredMain != lastMainLength)
            {
                InventorySkillService.EnsureMainCapacity(player, "Controller");
                lastPlayer = player;
                lastMainLength = desiredMain;
            }

            BackpackData? backpack = InventorySkillService.TryGetEquippedBackpackData(Character.localCharacter);
            if (backpack != null && (desiredBackpack != lastBackpackCapacity || backpack.itemSlots.Length < desiredBackpack))
            {
                InventorySkillService.EnsureBackpackCapacity(backpack, "Controller");
                lastBackpackCapacity = desiredBackpack;
            }

            InventorySkillService.EnsureHotbarUI(GUIManager.instance);
        }
    }
}
