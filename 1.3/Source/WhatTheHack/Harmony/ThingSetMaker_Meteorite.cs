using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(ThingSetMaker_Meteorite), "FindRandomMineableDef")]
    static class ThingSetMaker_Meteorite_FindRandomMineableDef
    {
        static void Postfix(ThingSetMaker_Meteorite __instance, ThingDef __result)
        {
            if(__result == WTH_DefOf.WTH_MechanoidParts)
            {
                __result = WTH_DefOf.MineableSteel;
                Log.Message("Replaced WTH parts by steel");
            }
        }
    }
    
}
