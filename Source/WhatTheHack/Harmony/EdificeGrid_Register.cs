using HarmonyLib;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(EdificeGrid), "Register")]
internal class EdificeGrid_Register
{
    private static bool Prefix(Building ed)
    {
        return ed.TryGetComp<CompMountable>() is not { Active: true };
    }
}