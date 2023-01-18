using HarmonyLib;
using RimWorld;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CompHibernatable), "Startup")]
internal class CompHibernatable_Startup
{
    private static void Postfix(CompHibernatable __instance, ref int ___endStartupTick)
    {
        if (__instance.parent is Building_MechanoidBeacon beacon)
        {
            ___endStartupTick += beacon.GetComp<CompHibernatable_MechanoidBeacon>().extraStartUpDays *
                                 GenDate.TicksPerDay;
        }
    }
}