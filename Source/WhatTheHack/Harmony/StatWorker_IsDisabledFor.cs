using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(StatWorker), "IsDisabledFor")]
internal static class StatWorker_IsDisabledFor
{
    private static bool Prefix(Thing thing, ref bool __result)
    {
        if (thing is not Pawn pawn)
        {
            return true;
        }

        if (!pawn.IsHacked())
        {
            return true;
        }

        __result = false;
        return false;
    }
}