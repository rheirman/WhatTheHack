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
        static void Postfix(CompHibernatable __instance, ref int ___endStartupTick)
        {
            if(__instance.parent is Building_MechanoidBeacon beacon)
            {
                ___endStartupTick += beacon.GetComp<CompHibernatable_MechanoidBeacon>().extraStartUpDays * GenDate.TicksPerDay;
            }
        }
    }
}
