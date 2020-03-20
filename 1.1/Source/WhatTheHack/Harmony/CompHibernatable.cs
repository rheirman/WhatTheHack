using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(CompHibernatable), "Startup")]
    class CompHibernatable_Startup
    {
        static void Postfix(CompHibernatable __instance)
        {
            if(__instance.parent is Building_MechanoidBeacon beacon)
            {
                Traverse.Create(__instance).Field("endStartupTicks").SetValue(Traverse.Create(__instance).Field("endStartupTicks").GetValue<int>() + beacon.GetComp<CompHibernatable_MechanoidBeacon>().extraStartUpDays * GenDate.TicksPerDay);
            }
        }
    }
}
