using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Caravan), "Tick")]
    class Caravan_Tick
    {
        static void Postfix(Caravan __instance)
        {
            //Consume only fuel every interval of tickperday/fuelConsumptionRate. 
            float interval = GenDate.TicksPerDay / WTH_DefOf.WTH_PortableChargingPlatform.GetCompProperties<CompProperties_Refuelable>().fuelConsumptionRate;
            if (__instance.IsHashIntervalTick(Mathf.RoundToInt(interval))){
                int numMechanoids = 0;
                int numPlatforms = 0;
                foreach (Thing thing in __instance.AllThings)
                {
                    if (thing is Pawn && ((Pawn)thing).IsHacked())
                    {
                        numMechanoids++;
                    }
                    if (thing.def == ThingDefOf.MinifiedThing)
                    {
                        if (thing.GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform)
                        {
                            numPlatforms ++;
                        }
                    }
                }
                foreach (Thing thing in __instance.AllThings)
                {
                    Log.Message(thing.def.defName);
                    if (numMechanoids > 0 && numPlatforms > 0 && thing.def == ThingDefOf.Chemfuel && thing.stackCount > 0)
                    {
                        int fuelConsumedThisInterval = Math.Min(numMechanoids, numPlatforms);
                        thing.SplitOff(fuelConsumedThisInterval).Destroy(DestroyMode.Vanish);
                        if(thing.stackCount == 0) {
                            Messages.Message("WTH_Message_CaravanOutOfFuel".Translate(new object[]{__instance.LabelCap}), __instance, MessageTypeDefOf.ThreatBig, true);
                        }
                    }
                }
            }
           
        }
    }
}
