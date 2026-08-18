using System;
using PEAKUsageSkills.Core;
using UnityEngine;

namespace PEAKUsageSkills.GameAdapters
{
    internal static class InventorySkillService
    {
        public const int VanillaBackpackSlots = 4;
        private static Character? cachedBackpackCharacter;
        private static ItemInstanceData? cachedBackpackInstance;
        private static BackpackData? cachedBackpackData;

        public static int ExtraBackpackSlots => SkillMath.ExtraBackpackSlots(
            Plugin.Progression.GetLevel(SkillId.Strength));

        public static int DesiredBackpackCapacity => VanillaBackpackSlots + ExtraBackpackSlots;

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
                Plugin.ModLog.LogInfo(
                    $"[UsageSkills:Inventory] backpack slots expanded to {required} "
                    + $"strength={Plugin.Progression.GetLevel(SkillId.Strength)} source={source}");
            }

            for (int index = 0; index < existing.Length; index++)
            {
                existing[index] ??= new ItemSlot((byte)index);
            }
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
        private int lastBackpackCapacity;

        private void Update()
        {
            int desiredBackpack = InventorySkillService.DesiredBackpackCapacity;
            BackpackData? backpack = InventorySkillService.TryGetEquippedBackpackData(Character.localCharacter);
            if (backpack != null
                && (desiredBackpack != lastBackpackCapacity || backpack.itemSlots.Length < desiredBackpack))
            {
                InventorySkillService.EnsureBackpackCapacity(backpack, "Controller");
                lastBackpackCapacity = desiredBackpack;
            }
        }
    }
}
