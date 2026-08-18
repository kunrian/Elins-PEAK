using System;
using PEAKUsageSkills.Core;
using UnityEngine;

namespace PEAKUsageSkills.GameAdapters
{
    internal static class InventorySkillService
    {
        private static Character? cachedBackpackCharacter;
        private static ItemInstanceData? cachedBackpackInstance;
        private static BackpackData? cachedBackpackData;

        public static int ExtraBackpackSlots => SkillMath.ExtraBackpackSlots(
            Plugin.Progression.GetLevel(SkillId.Strength));

        public static bool IsItemStorage(BackpackSlot.BackpackType backpackType)
        {
            return backpackType == BackpackSlot.BackpackType.Backpack
                || backpackType == BackpackSlot.BackpackType.Fannypack
                || backpackType == BackpackSlot.BackpackType.Jetpack;
        }

        public static int BaseItemSlots(BackpackSlot.BackpackType backpackType)
        {
            switch (backpackType)
            {
                case BackpackSlot.BackpackType.Backpack:
                    return 4;
                case BackpackSlot.BackpackType.Fannypack:
                    return 2;
                case BackpackSlot.BackpackType.Jetpack:
                    return 1;
                default:
                    return 0;
            }
        }

        public static int DesiredItemCapacity(BackpackSlot.BackpackType backpackType)
        {
            int baseSlots = BaseItemSlots(backpackType);
            return baseSlots == 0 ? 0 : baseSlots + ExtraBackpackSlots;
        }

        public static int EnsureBackpackCapacity(
            BackpackData? data,
            BackpackSlot.BackpackType backpackType,
            string source)
        {
            if (data == null || !IsItemStorage(backpackType))
            {
                return 0;
            }

            ItemSlot[] existing = data.itemSlots ?? Array.Empty<ItemSlot>();
            int required = Math.Max(DesiredItemCapacity(backpackType), HighestOccupiedIndex(existing) + 1);
            if (existing.Length < required)
            {
                Array.Resize(ref existing, required);
                data.itemSlots = existing;
                Plugin.ModLog.LogInfo(
                    $"[UsageSkills:Inventory] {backpackType} item slots expanded to {required} "
                    + $"strength={Plugin.Progression.GetLevel(SkillId.Strength)} source={source}");
            }

            for (int index = 0; index < existing.Length; index++)
            {
                existing[index] ??= new ItemSlot((byte)index);
            }

            return required;
        }

        public static BackpackData? TryGetEquippedBackpackData(
            Character? character,
            out BackpackSlot.BackpackType backpackType)
        {
            backpackType = character?.player?.backpackSlot?.backpackType
                ?? BackpackSlot.BackpackType.None;
            if (character?.player?.backpackSlot == null || character.player.backpackSlot.IsEmpty())
            {
                cachedBackpackCharacter = character;
                cachedBackpackInstance = null;
                cachedBackpackData = null;
                return null;
            }

            if (!IsItemStorage(backpackType))
            {
                cachedBackpackCharacter = character;
                cachedBackpackInstance = character.player.backpackSlot.data;
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
    }

    internal sealed class InventorySkillController : MonoBehaviour
    {
        private BackpackData? lastBackpackData;
        private int lastDesiredCapacity;
        private BackpackSlot.BackpackType lastBackpackType;

        private void Update()
        {
            BackpackData? backpack = InventorySkillService.TryGetEquippedBackpackData(
                Character.localCharacter,
                out BackpackSlot.BackpackType backpackType);
            int desiredBackpack = InventorySkillService.DesiredItemCapacity(backpackType);
            if (backpack != null
                && (!ReferenceEquals(backpack, lastBackpackData)
                    || backpackType != lastBackpackType
                    || desiredBackpack != lastDesiredCapacity
                    || backpack.itemSlots.Length < desiredBackpack))
            {
                InventorySkillService.EnsureBackpackCapacity(backpack, backpackType, "Controller");
                lastBackpackData = backpack;
                lastBackpackType = backpackType;
                lastDesiredCapacity = desiredBackpack;
            }
        }
    }
}
