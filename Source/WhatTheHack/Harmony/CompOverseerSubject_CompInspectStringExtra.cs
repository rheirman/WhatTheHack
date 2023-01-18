using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CompOverseerSubject), "CompInspectStringExtra")]
internal class CompOverseerSubject_CompInspectStringExtra
{
    private static bool Prefix(CompOverseerSubject __instance, ref string __result)
    {
        if (__instance.parent is not Pawn pawn || !pawn.IsHacked())
        {
            return true;
        }

        __result = null;
        return false;
    }
}