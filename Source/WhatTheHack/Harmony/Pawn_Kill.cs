using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn), "Kill")]
internal static class Pawn_Kill
{
    private static void Prefix(Pawn __instance)
    {
        if (!__instance.IsMechanoid())
        {
            return;
        }

        if (__instance.relations == null)
        {
            __instance.relations = new Pawn_RelationsTracker(__instance);
        }

        __instance.RemoveAllLinks();
    }
}