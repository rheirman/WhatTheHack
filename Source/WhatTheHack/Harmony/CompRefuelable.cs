using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    
    [HarmonyPatch(typeof(ThingWithComps), "InitializeComps")]
    class ThingWithComps_Initialize
    {
        //Only initialize the refeulcomp of mechanoids that have a repairmodule. 
        static bool Prefix(ref ThingWithComps __instance)
        {
            Log.Message("InitializeComps called");
            if (__instance is Pawn && ((Pawn)__instance).RaceProps.IsMechanoid && __instance.def.comps.Any<CompProperties>())
            {
                Log.Message("InitializeComps for mechanoid");
                Pawn pawn = (Pawn)__instance;
                List<ThingComp> comps = new List<ThingComp>();
                for (int i = 0; i < __instance.def.comps.Count; i++)
                {
                    ThingComp thingComp = (ThingComp)Activator.CreateInstance(__instance.def.comps[i].compClass);
                    Log.Message("thingComp is refuelable: " + (thingComp is CompRefuelable));
                    Log.Message("pawn.IsHacked()" + pawn.IsHacked());
                    if(pawn.health != null && pawn.health.hediffSet != null)
                    {
                        Log.Message("pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule)) : " + pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule));
                    }
                    if (!(thingComp is CompRefuelable) || (pawn.IsHacked() && pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule)))
                    {
                        Log.Message("adding refuelable component for mechanoid");
                        thingComp.parent = __instance;
                        comps.Add(thingComp);
                        thingComp.Initialize(__instance.def.comps[i]);
                        thingComp.PostSpawnSetup(false);
                    }
                }
                Traverse.Create(__instance).Field("comps").SetValue(comps);

                return false;
                
            }
            return true;          
        }
    }
    
}
