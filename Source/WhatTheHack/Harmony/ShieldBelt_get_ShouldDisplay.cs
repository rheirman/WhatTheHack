using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CompShield), "get_ShouldDisplay")]
internal class ShieldBelt_get_ShouldDisplay
{
    private static void Postfix(CompShield __instance, ref bool __result)
    {
        if (__result == false && __instance.PawnOwner.health != null &&
            __instance.PawnOwner.health.hediffSet.HasHediff(WTH_DefOf.WTH_BeltModule) &&
            __instance.PawnOwner.IsHacked() && __instance.PawnOwner.IsActivated())
        {
            __result = true;
        }
    }
}