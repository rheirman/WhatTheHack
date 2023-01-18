using HarmonyLib;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

//TODO: transpile this for better performance. 
[HarmonyPatch(typeof(GenGrid), "Standable")]
internal class GenGrid_Standable
{
    private static void Postfix(ref bool __result, IntVec3 c, Map map)
    {
        if (__result)
        {
            return;
        }

        if (!c.Walkable(map))
        {
            __result = false;
            return;
        }

        var list = map.thingGrid.ThingsListAt(c);
        foreach (var t in list)
        {
            if (t is ThingWithComps twc && twc.GetComp<CompMountable>() is { Active: true })
            {
                __result = true;
            }
        }
    }
}