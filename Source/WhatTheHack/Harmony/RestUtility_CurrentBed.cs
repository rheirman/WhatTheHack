using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(RestUtility), "CurrentBed", typeof(Pawn))]
internal class RestUtility_CurrentBed
{
    private static bool Prefix(Pawn p, ref Building_Bed __result)
    {
        if (p.jobs == null || p.CurJob == null)
        {
            return true;
        }

        if (p.CurJob.def != WTH_DefOf.WTH_Mechanoid_Rest)
        {
            return true;
        }

        if (!p.IsHacked())
        {
            return true;
        }

        var thingList = p.Position.GetThingList(p.Map);
        foreach (var thing in thingList)
        {
            switch (thing)
            {
                case Building_HackingTable hackingTable when
                    p.Position == hackingTable.GetSleepingSlotPos(Building_HackingTable.SLOTINDEX):
                    __result = hackingTable;
                    return false;
                case Building_BaseMechanoidPlatform platform when
                    p.Position == platform.GetSleepingSlotPos(Building_BaseMechanoidPlatform.SLOTINDEX):
                    __result = platform;
                    return false;
            }
        }

        return true;
    }
}