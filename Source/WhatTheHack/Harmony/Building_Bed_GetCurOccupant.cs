using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Building_Bed), "GetCurOccupant")]
internal static class Building_Bed_GetCurOccupant
{
    //Copied from vanilla, prefixing and replacing is safe because of the check for Building_MechanoidPlatform
    private static bool Prefix(Building_Bed __instance, int slotIndex, ref Pawn __result)
    {
        if (__instance is not Building_BaseMechanoidPlatform && __instance is not Building_HackingTable)
        {
            return true;
        }

        if (!__instance.Spawned)
        {
            return false;
        }

        var sleepingSlotPos = __instance.GetSleepingSlotPos(slotIndex);
        var list = __instance.Map.thingGrid.ThingsListAt(sleepingSlotPos);
        foreach (var thing in list)
        {
            if (thing is not Pawn pawn)
            {
                continue;
            }

            if (__instance is Building_BaseMechanoidPlatform && pawn.IsHacked())
            {
                __result = pawn;
            }
            else if (pawn.CurJob != null)
            {
                __result = pawn;
            }
        }

        return false;
    }
}