using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    //Can wear belts d
    [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
    class Caravan_NeedsTracker_TrySatisfyPawnNeeds
    {
        static void Postfix(Pawn p, ThingDef apparel, ref bool __result)
        {
            if(!__result && Utilities.IsBelt(apparel.apparel) && p.health != null && p.health.hediffSet.HasHediff(WTH_DefOf.WTH_BeltModule)) //TODO: make more general
            {
                __result = true;
            }
        }
    }

   
}
