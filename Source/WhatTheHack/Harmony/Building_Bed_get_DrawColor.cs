using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
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

[HarmonyPatch(typeof(JobGiver_GetEnergy), "ShouldAutoRecharge")]
internal static class JobGiver_GetEnergy_ShouldAutoRecharge
{
    private static void Postfix(ref bool __result, Pawn pawn)
    {
        if (!__result)
        {
            return;
        }

        if (pawn.IsHacked())
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(Need_MechEnergy), "NeedInterval")]
internal static class Need_MechEnergy_NeedInterval
{
    private static bool Prefix(Need_MechEnergy __instance)
    {
        return !__instance.pawn.IsHacked();
    }
}