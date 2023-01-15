using HarmonyLib;
using RimWorld;
using UnityEngine;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Building_Bed), "get_DrawColor")]
internal static class Building_Bed_get_DrawColor
{
    private static void Postfix(Building_Bed __instance, ref Color __result)
    {
        if (__instance is Building_BaseMechanoidPlatform || __instance is Building_HackingTable)
        {
            __result = new Color(1f, 1f, 1f);
        }
    }
}

//[HarmonyPatch(typeof(CompAssignableToPawn), "CanAssignTo")]
//static class CompAssignableToPawn_CanAssignTo
//{
//    static bool Prefix(CompAssignableToPawn __instance, Pawn pawn, ref AcceptanceReport __result)
//    {
//        if (pawn.IsHacked() && pawn.Faction == Faction.OfPlayer)
//        {
//            __result = AcceptanceReport.WasAccepted;
//            return false;
//        }
//        return true;
//    }
//}

//Patch is needed so mechanoids that are standing up can still have a "cur bed"