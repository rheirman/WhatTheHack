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
            if (__instance is Pawn && ((Pawn)__instance).RaceProps.IsMechanoid && __instance.def.comps.Any<CompProperties>())
            {
                Pawn pawn = (Pawn)__instance;
                List<ThingComp> comps = Traverse.Create(__instance).Field("comps").GetValue<List<ThingComp>>();
                if (comps == null)
                {
                    comps = new List<ThingComp>();
                }
                for (int i = 0; i < __instance.def.comps.Count; i++)
                {
                    ThingComp thingComp = (ThingComp)Activator.CreateInstance(__instance.def.comps[i].compClass);
                    CompProperties_Refuelable refuelableProps = thingComp.props as CompProperties_Refuelable;

                    if (!(thingComp is CompRefuelable) || 
                        pawn.health == null ||
                        !refuelableProps.fuelFilter.Allows(WTH_DefOf.WTH_MechanoidParts) ||
                        (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule)))
                    {
                        thingComp.parent = __instance;
                        comps.Add(thingComp);
                        thingComp.Initialize(__instance.def.comps[i]);
                    }
                }
                Traverse.Create(__instance).Field("comps").SetValue(comps);
                return false;

            }
            return true;
        }
    }

}