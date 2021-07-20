using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony
{
    //Don't allow multiple belts for mechs with belt modules. Mechs often don't have a waist, and vanilla CanWearTogether therefore always returns true for belt items.
    //Almost literal copy of JobDriver_Wear_TryUnequipSomething, but applies for hacked mechs only, and disallows multiple belts. 
    [HarmonyPatch(typeof(JobDriver_Wear), "TryUnequipSomething")]
    class JobDriver_Wear_TryUnequipSomething
    {
        static bool Prefix(ref JobDriver_Wear __instance)
        {
            if (__instance.pawn.IsHacked())
            {
                Apparel apparel = Traverse.Create(__instance).Property("Apparel").GetValue<Apparel>();
                List<Apparel> wornApparel = __instance.pawn.apparel.WornApparel;
                foreach (Apparel wornApp in wornApparel)
                {
                    if (BothBelts(apparel.def, wornApp.def))
                    {
                        bool forbid = __instance.pawn.Faction != null && __instance.pawn.Faction.HostileTo(Faction.OfPlayer);
                        Apparel apparel2;
                        if (!__instance.pawn.apparel.TryDrop(wornApp, out apparel2, __instance.pawn.PositionHeld, forbid))
                        {
                            Log.Error(__instance.pawn + " could not drop " + wornApp.ToStringSafe<Apparel>(), false);
                            __instance.EndJobWith(JobCondition.Errored);
                            return false;
                        }                   
                        break;
                    }
                }
            }
            return true;
        }
        private static bool BothBelts(ThingDef A, ThingDef B)
        {
            if (Utilities.IsBelt(A.apparel) && Utilities.IsBelt(B.apparel))
            {
                return true;
            }
            return false;
        }
    }
}
