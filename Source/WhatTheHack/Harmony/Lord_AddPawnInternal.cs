using HarmonyLib;
using Verse;
using Verse.AI.Group;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Lord), "AddPawnInternal")]
internal static class Lord_AddPawnInternal
{
    private static bool Prefix(Pawn p, Lord __instance)
    {
        return !__instance.ownedPawns.Contains(p);
    }
}