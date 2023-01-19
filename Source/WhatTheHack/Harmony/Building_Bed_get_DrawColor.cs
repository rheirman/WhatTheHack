using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
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

[HarmonyPatch(typeof(Lord), "AddPawnInternal")]
internal static class Lord_AddPawnInternal
{
    private static bool Prefix(Pawn p, Lord __instance)
    {
        return !__instance.ownedPawns.Contains(p);
    }
}