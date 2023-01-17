using HarmonyLib;
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
            if (__instance.IsHashIntervalTick(480))
            {
                List<Thing> allParts = __instance.AllThings.Where((Thing t) => t.def == WTH_DefOf.WTH_MechanoidParts).ToList();
                if (!allParts.NullOrEmpty())
                {
                    List<Pawn> allHackedMechs = __instance.AllThings.Where((Thing t) => t is Pawn pawn && !pawn.Dead && pawn.IsHacked()).Cast<Pawn>().ToList();
                    if (!allHackedMechs.NullOrEmpty())
                    {
                        RepairMechsIfNeeded(__instance, allParts, allHackedMechs);
                        MaintainMechsIfNeeded(__instance, allParts, allHackedMechs);
                    }
                }
            }
        }

        private static void MaintainMechsIfNeeded(Caravan caravan, List<Thing> allParts, List<Pawn> allHackedMechs)
        {
            Thing partItem = allParts.First();
            List<Pawn> allPawnsCapableOfMaintenance = caravan.AllThings.Where((Thing t) => t is Pawn pawn && !pawn.Dead && !pawn.Downed).Cast<Pawn>().ToList();
            if (allPawnsCapableOfMaintenance.NullOrEmpty())
            {
                return;
            }
            List<Pawn> allMechsNeedingMaintenance = allHackedMechs.Where((Pawn p) => p.needs.TryGetNeed<Need_Maintenance>() is Need_Maintenance needM && needM.CurLevelPercentage < 0.5f).ToList();
            if (allMechsNeedingMaintenance.NullOrEmpty())
            {
                return;
            }
            Pawn bestPawn = allPawnsCapableOfMaintenance.MaxBy((Pawn p) => p.skills.AverageOfRelevantSkillsFor(WTH_DefOf.WTH_Hack));
            float successChance = bestPawn.GetStatValue(WTH_DefOf.WTH_HackingSuccessChance, true);
            if(successChance < 0.20f)
            {
                return;
            }

            Pawn chosenMech = allMechsNeedingMaintenance.RandomElement();
            Need_Maintenance need = chosenMech.needs.TryGetNeed<Need_Maintenance>();
            int partsAvailable = Math.Min(need.PartsNeededToRestore(), partItem.stackCount);
            float combatPowerCapped = chosenMech.kindDef.combatPower <= 10000 ? chosenMech.kindDef.combatPower : 300;

            if (Rand.Chance(successChance))
            {
                bestPawn.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.5f, false);
                bestPawn.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.5f, false);
                need.RestoreUsingParts(partsAvailable);
            }
            else
            {
                bestPawn.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.25f, false);
                bestPawn.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.25f, false);
            }
            partItem.SplitOff(partsAvailable);
        }

        private static void RepairMechsIfNeeded(Caravan caravan, List<Thing> allParts, List<Pawn> allHackedMechs)
        {
            Thing partItem = allParts.First();
            if(partItem.stackCount < 5)
            {
                return;
            }
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
                if (thing is Pawn pawn && pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
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
