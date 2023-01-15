using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn), "get_IsColonyMech")]
internal static class Pawn_IsColonyMech
{
    private static bool Prefix(Pawn __instance, ref bool __result)
    {
        if (!__instance.IsHacked())
        {
            return true;
        }

        __result = false;
        return false;
    }
}