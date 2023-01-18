using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CompAssignableToPawn_Bed), "get_AssigningCandidates")]
internal static class CompAssignableToPawn_Bed_AssigningCandidates
{
    private static bool Prefix(CompAssignableToPawn __instance, ref IEnumerable<Pawn> __result)
    {
        if (__instance.parent is not Building_BaseMechanoidPlatform)
        {
            return true;
        }

        if (!__instance.parent.Spawned)
        {
            __result = Enumerable.Empty<Pawn>();
        }

        __result = __instance.parent.Map.mapPawns.AllPawns.Where(p => p.IsHacked());
        return false;
    }
}