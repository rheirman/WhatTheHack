using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Caravan), "Tick")]
    class Caravan_Tick
    {
        static void Postfix(Caravan __instance)
        {
            //Consume only fuel every interval of tickperday/fuelConsumptionRate. 
            float interval = GenDate.TicksPerDay / WTH_DefOf.WTH_PortableChargingPlatform.GetCompProperties<CompProperties_Refuelable>().fuelConsumptionRate;
            if (__instance.IsHashIntervalTick(Mathf.RoundToInt(interval)))
            {
                ConsumeFuelIfNeeded(__instance);
            }
            if (__instance.IsHashIntervalTick(240))
            {
                RepairMechsIfNeeded(__instance);
            }
        }

        private static void RepairMechsIfNeeded(Caravan __instance)
        {
            List<Thing> allParts = __instance.AllThings.Where((Thing t) => t.def == WTH_DefOf.WTH_MechanoidParts).ToList();
            if (allParts.NullOrEmpty())
            {
                return;
            }
            Thing partItem = allParts.First();
            if(partItem.stackCount < 5)
            {
                return;
            }
            List<Pawn> allHackedMechs = __instance.AllThings.Where((Thing t) => t is Pawn pawn && !pawn.Dead && pawn.IsHacked()).Cast<Pawn>().ToList();
            if (allHackedMechs.NullOrEmpty())
            {
                return;
            }
            List<Pawn> allMechsNeedingRepairs = allHackedMechs.Where((Pawn p) => p.health.hediffSet.HasNaturallyHealingInjury() && !p.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing) && (p.playerSettings == null || p.playerSettings.medCare != MedicalCareCategory.NoCare)).ToList();
            if (allMechsNeedingRepairs.NullOrEmpty())
            {
                return;
            }

            foreach (Pawn pawn in allHackedMechs)
            {
                if(pawn.needs.TryGetNeed<Need_Power>() is Need_Power powerNeed && !powerNeed.OutOfPower)
                {
                    if (pawn.health != null && pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairArm))
                    {
                        allMechsNeedingRepairs.RandomElement().health.AddHediff(WTH_DefOf.WTH_Repairing);
                        partItem.SplitOff(5);
                        return;
                    }
                    if (pawn.health != null && pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule) && pawn.health.hediffSet.HasNaturallyHealingInjury() && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing))
                    {
                        pawn.health.AddHediff(WTH_DefOf.WTH_Repairing);
                        partItem.SplitOff(5);
                        return;
                    }
                }

            }
        }

        private static void ConsumeFuelIfNeeded(Caravan __instance)
        {
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
                        numPlatforms++;
                    }
                }
                if (thing.def == WTH_DefOf.WTH_PortableChargingPlatform)
                {
                    numPlatforms += thing.stackCount;
                }
            }
            foreach (Thing thing in __instance.AllThings)
            {
                if (numMechanoids > 0 && numPlatforms > 0 && thing.def == ThingDefOf.Chemfuel && thing.stackCount > 0)
                {
                    int fuelConsumedThisInterval = Math.Min(numMechanoids, numPlatforms);
                    thing.SplitOff(fuelConsumedThisInterval).Destroy(DestroyMode.Vanish);
                    if (thing.stackCount == 0)
                    {
                        Messages.Message("WTH_Message_CaravanOutOfFuel".Translate(new object[] { __instance.LabelCap }), __instance, MessageTypeDefOf.ThreatBig, true);
                    }
                }
            }
        }
    }
}
