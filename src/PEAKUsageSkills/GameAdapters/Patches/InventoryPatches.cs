using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace PEAKUsageSkills.GameAdapters.Patches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    internal static class PlayerInventoryCapacityPatch
    {
        private static void Postfix(Player __instance)
        {
            if (__instance == Player.localPlayer)
            {
                InventorySkillService.EnsureMainCapacity(__instance, "PlayerAwake");
            }
        }
    }

    [HarmonyPatch(typeof(BackpackData), "Init")]
    internal static class BackpackDataInitPatch
    {
        private static void Prefix(BackpackData __instance)
        {
            InventorySkillService.EnsureBackpackCapacity(__instance, "BackpackInit");
        }
    }

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
            int required = Math.Max(InventorySkillService.DesiredBackpackCapacity, Math.Max(serializedCount, currentCount));
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

            Plugin.ModLog.LogInfo($"[UsageSkills:Inventory] backpack deserialized serialized={serializedCount} retainedCapacity={slots.Length}");
            return false;
        }
    }

    [HarmonyPatch(typeof(GUIManager), "UpdateItems")]
    internal static class HotbarUiCapacityPatch
    {
        private static void Prefix(GUIManager __instance)
        {
            InventorySkillService.EnsureHotbarUI(__instance);
        }
    }

    [HarmonyPatch(typeof(BackpackWheel), "InitWheel")]
    internal static class BackpackWheelCapacityPatch
    {
        private static void Prefix(BackpackWheel __instance, BackpackReference bp, ref int slotCount)
        {
            BackpackData data = bp.GetData();
            InventorySkillService.EnsureBackpackCapacity(data, "WheelOpen");
            slotCount = data.itemSlots.Length;
            int requiredSlices = slotCount + 1;
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

            float radius = 158f + Math.Max(0, requiredSlices - 5) * 20f;
            for (int index = 0; index < requiredSlices; index++)
            {
                float angle = 360f / requiredSlices * index + 158f;
                float radians = (angle + 112f) * Mathf.Deg2Rad;
                Transform transform = __instance.slices[index].transform;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                transform.localPosition = new Vector3(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius, 0f);
            }
        }
    }

    [HarmonyPatch(typeof(BackpackVisuals), "RefreshVisuals")]
    internal static class BackpackVisualCapacityPatch
    {
        private static void Prefix(BackpackVisuals __instance)
        {
            BackpackData? data = __instance.GetBackpackData();
            if (data == null || __instance.backpackSlots == null || __instance.backpackSlots.Length == 0)
            {
                return;
            }

            if (__instance.backpackSlots.Length < data.itemSlots.Length)
            {
                Transform[] old = __instance.backpackSlots;
                Transform[] expanded = new Transform[data.itemSlots.Length];
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
            MethodInfo countMethod = AccessTools.Method(typeof(BackpackVisualCapacityPatch), nameof(GetVisualSlotCount));
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
            return visuals.GetBackpackData()?.itemSlots?.Length ?? InventorySkillService.VanillaBackpackSlots;
        }
    }

    [HarmonyPatch(typeof(CharacterItems), "DoSwitching")]
    internal static class ExtendedHotbarSwitchingPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo lastSlotMethod = AccessTools.Method(typeof(ExtendedHotbarSwitchingPatch), nameof(GetLastSlot));
            for (int index = 0; index < codes.Count; index++)
            {
                if ((codes[index].opcode == OpCodes.Ble || codes[index].opcode == OpCodes.Ble_S)
                    && index >= 2
                    && codes[index - 1].opcode == OpCodes.Conv_I4
                    && codes[index - 2].opcode == OpCodes.Ldlen)
                {
                    codes[index].opcode = codes[index].opcode == OpCodes.Ble ? OpCodes.Blt : OpCodes.Blt_S;
                }

                if (codes[index].LoadsConstant(3)
                    && index + 1 < codes.Count
                    && codes[index + 1].opcode == OpCodes.Newobj
                    && codes[index + 1].operand is ConstructorInfo constructor
                    && constructor.DeclaringType == typeof(decimal))
                {
                    codes[index].opcode = OpCodes.Call;
                    codes[index].operand = lastSlotMethod;
                }
            }

            return codes;
        }

        private static int GetLastSlot()
        {
            return Math.Max(InventorySkillService.BackpackHotbarSlotId, (Character.localCharacter?.player?.itemSlots?.Length ?? 4) - 1);
        }
    }

    [HarmonyPatch(typeof(CharacterItems), "Update")]
    internal static class ExtendedHotbarNumberKeysPatch
    {
        private static readonly FieldInfo CharacterField = AccessTools.Field(typeof(CharacterItems), "character");

        private static void Postfix(CharacterItems __instance)
        {
            Character? character = CharacterField.GetValue(__instance) as Character;
            if (character == null || !character.IsLocal || character.input == null || GUIManager.InPauseMenu)
            {
                return;
            }

            int length = character.player?.itemSlots?.Length ?? 0;
            for (int slot = 4; slot < length && slot < 9; slot++)
            {
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + slot)))
                {
                    __instance.EquipSlot(Optionable<byte>.Some((byte)slot));
                }
            }
        }
    }

    [HarmonyPatch(typeof(CharacterItems), "DropAllItems")]
    internal static class ExtendedInventoryDropPatch
    {
        private static void Prefix(PhotonView ___photonView)
        {
            Character character = Character.localCharacter;
            ItemSlot[] slots = character?.player?.itemSlots ?? Array.Empty<ItemSlot>();
            Vector3 position = character == null ? Vector3.zero : character.Center + Vector3.up * 0.5f;
            for (int index = 4; index < slots.Length; index++)
            {
                ItemSlot slot = slots[index];
                if (slot?.prefab != null && slot.prefab.UIData.canDrop)
                {
                    ___photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, (byte)index, position);
                }
            }
        }
    }
}
