using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Building_Bed), "GetSleepingSlotPos")]
internal static class Building_Bed_GetSleepingSlotPos
{
    private static void Postfix(Building_Bed __instance, ref IntVec3 __result)
    {
        if (__instance is Building_BaseMechanoidPlatform)
        {
            __result = __instance.InteractionCell;
        }
    }
}