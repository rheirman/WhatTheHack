using HarmonyLib;
using UnityEngine;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Thing), "get_DrawPos")]
internal static class Thing_get_DrawPos
{
    private static bool Prefix(Thing __instance, ref Vector3 __result)
    {
        if (__instance.Destroyed || __instance.TryGetComp<CompMountable>() is not { Active: true } comp)
        {
            return true;
        }

        var drawPos = comp.mountedTo.DrawPos;
        drawPos.z = comp.mountedTo.DrawPos.z + comp.drawOffset;
        drawPos.y = comp.mountedTo.DrawPos.y + 1;
        __result = drawPos;
        return false;
    }
}