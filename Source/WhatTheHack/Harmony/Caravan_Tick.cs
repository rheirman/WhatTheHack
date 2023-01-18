using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Caravan), "Tick")]
internal class Caravan_Tick
{
    private static void Postfix(Caravan __instance)
    {
        //Consume only fuel every interval of tickperday/fuelConsumptionRate. 
        var interval = GenDate.TicksPerDay / WTH_DefOf.WTH_PortableChargingPlatform
            .GetCompProperties<CompProperties_Refuelable>().fuelConsumptionRate;
        if (__instance.IsHashIntervalTick(Mathf.RoundToInt(interval)))
        {
            ConsumeFuelIfNeeded(__instance);
        }

        if (!__instance.IsHashIntervalTick(480))
        {
            return;
        }

        var allParts = __instance.AllThings.Where(t => t.def == WTH_DefOf.WTH_MechanoidParts).ToList();
        if (allParts.NullOrEmpty())
        {
            return;
        }

        var allHackedMechs = __instance.AllThings.Where(t => t is Pawn { Dead: false } pawn && pawn.IsHacked())
            .Cast<Pawn>().ToList();
        if (allHackedMechs.NullOrEmpty())
        {
            return;
        }

        RepairMechsIfNeeded(__instance, allParts, allHackedMechs);
        MaintainMechsIfNeeded(__instance, allParts, allHackedMechs);
    }

    private static void MaintainMechsIfNeeded(Caravan caravan, List<Thing> allParts, List<Pawn> allHackedMechs)
    {
        var partItem = allParts.First();
        var allPawnsCapableOfMaintenance = caravan.AllThings.Where(t => t is Pawn { Dead: false, Downed: false })
            .Cast<Pawn>().ToList();
        if (allPawnsCapableOfMaintenance.NullOrEmpty())
        {
            return;
        }

        var allMechsNeedingMaintenance = allHackedMechs.Where(p =>
            p.needs.TryGetNeed<Need_Maintenance>() is { CurLevelPercentage: < 0.5f }).ToList();
        if (allMechsNeedingMaintenance.NullOrEmpty())
        {
            return;
        }

        var bestPawn = allPawnsCapableOfMaintenance.MaxBy(p => p.skills.AverageOfRelevantSkillsFor(WTH_DefOf.WTH_Hack));
        var successChance = bestPawn.GetStatValue(WTH_DefOf.WTH_HackingSuccessChance);
        if (successChance < 0.20f)
        {
            return;
        }

        var chosenMech = allMechsNeedingMaintenance.RandomElement();
        var need = chosenMech.needs.TryGetNeed<Need_Maintenance>();
        var partsAvailable = Math.Min(need.PartsNeededToRestore(), partItem.stackCount);
        var combatPowerCapped = chosenMech.kindDef.combatPower <= 10000 ? chosenMech.kindDef.combatPower : 300;

        if (Rand.Chance(successChance))
        {
            bestPawn.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.5f);
            bestPawn.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.5f);
            need.RestoreUsingParts(partsAvailable);
        }
        else
        {
            bestPawn.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.25f);
            bestPawn.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.25f);
        }

        partItem.SplitOff(partsAvailable);
    }

    private static void RepairMechsIfNeeded(Caravan caravan, List<Thing> allParts, List<Pawn> allHackedMechs)
    {
        var partItem = allParts.First();
        if (partItem.stackCount < 5)
        {
            return;
        }

        if (allHackedMechs.NullOrEmpty())
        {
            return;
        }

        var allMechsNeedingRepairs = allHackedMechs.Where(p =>
            p.health.hediffSet.HasNaturallyHealingInjury() && !p.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing) &&
            (p.playerSettings == null || p.playerSettings.medCare != MedicalCareCategory.NoCare)).ToList();
        if (allMechsNeedingRepairs.NullOrEmpty())
        {
            return;
        }

        foreach (var pawn in allHackedMechs)
        {
            if (pawn.needs.TryGetNeed<Need_Power>() is not { OutOfPower: false })
            {
                continue;
            }

            if (pawn.health != null && pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairArm))
            {
                allMechsNeedingRepairs.RandomElement().health.AddHediff(WTH_DefOf.WTH_Repairing);
                partItem.SplitOff(5);
                return;
            }

            if (pawn.health == null || !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule) ||
                !pawn.health.hediffSet.HasNaturallyHealingInjury() ||
                pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing))
            {
                continue;
            }

            pawn.health.AddHediff(WTH_DefOf.WTH_Repairing);
            partItem.SplitOff(5);
            return;
        }
    }

    private static void ConsumeFuelIfNeeded(Caravan __instance)
    {
        var numMechanoids = 0;
        var numPlatforms = 0;
        foreach (var thing in __instance.AllThings)
        {
            if (thing is Pawn pawn && pawn.IsHacked() &&
                !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
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

        foreach (var thing in __instance.AllThings)
        {
            if (numMechanoids <= 0 || numPlatforms <= 0 || thing.def != ThingDefOf.Chemfuel || thing.stackCount <= 0)
            {
                continue;
            }

            var fuelConsumedThisInterval = Math.Min(numMechanoids, numPlatforms);
            thing.SplitOff(fuelConsumedThisInterval).Destroy();
            if (thing.stackCount == 0)
            {
                Messages.Message("WTH_Message_CaravanOutOfFuel".Translate(__instance.LabelCap),
                    __instance, MessageTypeDefOf.ThreatBig);
            }
        }
    }
}