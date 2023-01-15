using HarmonyLib;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Thing), "Print")]
internal static class Thing_Print
{
    private static bool Prefix(Thing __instance)
    {
        return __instance.TryGetComp<CompMountable>() is not { Active: true };
    }
}