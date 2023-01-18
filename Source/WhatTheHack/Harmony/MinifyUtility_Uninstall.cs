using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(MinifyUtility), "Uninstall")]
internal class MinifyUtility_Uninstall
{
    private static void Prefix(ref Thing th)
    {
        if (th.TryGetComp<CompMountable>() is { Active: true } comp)
        {
            comp.Uninstall();
        }
    }
}