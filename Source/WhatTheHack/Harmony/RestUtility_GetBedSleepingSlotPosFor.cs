using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(RestUtility), "GetBedSleepingSlotPosFor")]
internal class RestUtility_GetBedSleepingSlotPosFor
{
    private static bool Prefix(Building_Bed bed, ref IntVec3 __result)
    {
        switch (bed)
        {
            case Building_BaseMechanoidPlatform:
                __result = bed.GetSleepingSlotPos(Building_BaseMechanoidPlatform.SLOTINDEX);
                return false;
            case Building_HackingTable:
                __result = bed.GetSleepingSlotPos(Building_HackingTable.SLOTINDEX);
                return false;
            default:
                return true;
        }
    }
}