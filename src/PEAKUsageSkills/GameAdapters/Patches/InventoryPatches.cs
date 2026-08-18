using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace PEAKUsageSkills.GameAdapters.Patches
{
    [HarmonyPatch(typeof(BackpackData), "DeserializeValue")]
    internal static class BackpackDataDeserializePatch
    {
        private static bool Prefix(BackpackData __instance, Zorro.Core.Serizalization.BinaryDeserializer deserializer)
        {
            InventorySyncData sync = default;
            sync.Deserialize(deserializer);
            InventorySyncData.SlotData[] serializedSlots = sync.slots ?? Array.Empty<InventorySyncData.SlotData>();
            int serializedCount = serializedSlots.Length;
            int currentCount = __instance.itemSlots?.Length ?? 0;
            // BackpackData does not carry its BackpackType. Preserve every
            // serialized/current slot here; the typed wheel/controller hook
            // adds Strength slots later without guessing Fanny/Jet/Rocket.
            int required = Math.Max(serializedCount, currentCount);
            ItemSlot[] slots = __instance.itemSlots ?? Array.Empty<ItemSlot>();
            if (slots.Length < required)
            {
                Array.Resize(ref slots, required);
                __instance.itemSlots = slots;
            }

            for (int index = 0; index < slots.Length; index++)
            {
                slots[index] ??= new ItemSlot((byte)index);
                if (index < serializedCount)
                {
                    InventorySyncData.SlotData serialized = serializedSlots[index];
                    Item? item = ItemDatabase.TryGetItem(serialized.ItemID, out Item found) ? found : null;
                    slots[index].SetItem(item, serialized.Data);
                }
                else
                {
                    slots[index].SetItem(null, null);
                }
            }

            Plugin.ModLog.LogInfo(
                $"[UsageSkills:Inventory] backpack deserialized serialized={serializedCount} "
                + $"retainedCapacity={slots.Length}");
            return false;
        }
    }

    [HarmonyPatch(typeof(BackpackWheel), "InitWheel")]
    internal static class BackpackWheelCapacityPatch
    {
        private static void Prefix(
            BackpackWheel __instance,
            BackpackReference bp,
            ref int slotCount,
            BackpackSlot.BackpackType backpackType)
        {
            if (!InventorySkillService.IsItemStorage(backpackType))
            {
                return;
            }

            BackpackData data = bp.GetData();
            slotCount = InventorySkillService.EnsureBackpackCapacity(data, backpackType, "WheelOpen");
            // Vanilla iterates the complete backing array even when slotCount
            // hides entries. Keep enough inactive slices for retained data,
            // but lay out only the logical visible capacity.
            int requiredSlices = Math.Max(slotCount, data.itemSlots.Length) + 1;
            if (__instance.slices.Length < requiredSlices)
            {
                List<BackpackWheelSlice> slices = new List<BackpackWheelSlice>(__instance.slices);
                BackpackWheelSlice template = __instance.slices[Math.Min(1, __instance.slices.Length - 1)];
                while (slices.Count < requiredSlices)
                {
                    slices.Add(UnityEngine.Object.Instantiate(template, template.transform.parent));
                }

                __instance.slices = slices.ToArray();
            }

            int visibleSlices = slotCount + 1;
            float radius = 158f + Math.Max(0, visibleSlices - 5) * 20f;
            for (int index = 0; index < visibleSlices; index++)
            {
                float angle = 360f / visibleSlices * index + 158f;
                float radians = (angle + 112f) * Mathf.Deg2Rad;
                Transform transform = __instance.slices[index].transform;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                transform.localPosition = new Vector3(
                    Mathf.Cos(radians) * radius,
                    Mathf.Sin(radians) * radius,
                    0f);
            }
        }
    }

    [HarmonyPatch(typeof(BackpackVisuals), "RefreshVisuals")]
    internal static class BackpackVisualCapacityPatch
    {
        private static void Prefix(BackpackVisuals __instance)
        {
            BackpackSlot.BackpackType backpackType = GetBackpackType(__instance);
            if (!InventorySkillService.IsItemStorage(backpackType))
            {
                return;
            }

            BackpackData? data = __instance.GetBackpackData();
            if (data == null || __instance.backpackSlots == null || __instance.backpackSlots.Length == 0)
            {
                return;
            }

            int visibleSlots = InventorySkillService.EnsureBackpackCapacity(
                data,
                backpackType,
                "BackpackVisuals");
            if (__instance.backpackSlots.Length < visibleSlots)
            {
                Transform[] old = __instance.backpackSlots;
                Transform[] expanded = new Transform[visibleSlots];
                for (int index = 0; index < expanded.Length; index++)
                {
                    expanded[index] = old[index % old.Length];
                }

                __instance.backpackSlots = expanded;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo countMethod = AccessTools.Method(
                typeof(BackpackVisualCapacityPatch),
                nameof(GetVisualSlotCount));
            for (int index = 0; index + 1 < codes.Count; index++)
            {
                if (codes[index].LoadsConstant(4)
                    && (codes[index + 1].opcode == OpCodes.Blt || codes[index + 1].opcode == OpCodes.Blt_S))
                {
                    codes[index].opcode = OpCodes.Ldarg_0;
                    codes[index].operand = null;
                    codes.Insert(index + 1, new CodeInstruction(OpCodes.Call, countMethod));
                    break;
                }
            }

            return codes;
        }

        private static int GetVisualSlotCount(BackpackVisuals visuals)
        {
            BackpackSlot.BackpackType backpackType = GetBackpackType(visuals);
            if (!InventorySkillService.IsItemStorage(backpackType))
            {
                return 0;
            }

            return InventorySkillService.EnsureBackpackCapacity(
                visuals.GetBackpackData(),
                backpackType,
                "VisualRefresh");
        }

        private static BackpackSlot.BackpackType GetBackpackType(BackpackVisuals visuals)
        {
            if (visuals is BackpackOnBackVisuals onBack)
            {
                return onBack.backpackType;
            }

            Backpack backpack = visuals.GetComponent<Backpack>();
            return backpack != null
                ? backpack.backpackType
                : BackpackSlot.BackpackType.None;
        }
    }
}
