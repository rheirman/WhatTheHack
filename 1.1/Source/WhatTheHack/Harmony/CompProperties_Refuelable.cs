using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    //We added comprefuelable to mechanoids. Vanilla code assumes it to be used for buildings only, therefore this extra check to prevent errors. 
    [HarmonyPatch(typeof(CompProperties_Refuelable), "SpecialDisplayStats")]
    class CompProperties_Refuelable_SpecialDisplayStats
    {
        static bool Prefix(StatRequest req, ref IEnumerable<StatDrawEntry> __result)
        {
            if(((ThingDef)req.Def).building == null)
            {
                __result = new List<StatDrawEntry>();
                return false;
            }
            return true;
        }
    }
}
