using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
internal static class Pawn_HealthTracker_HealthTick
{
    private const int healTickInterval = 200;

    private static void Postfix(Pawn_HealthTracker __instance)
    {
        var pawn = __instance.hediffSet.pawn;
        if (!pawn.IsMechanoid())
        {
            return;
        }

        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_SelfDestructed))
        {
            SelfDestruct(pawn);
            return;
        }

        if (!pawn.IsHashIntervalTick(healTickInterval))
        {
            return;
        }

        var repairHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_Repairing);
        if (repairHediff != null &&
            repairHediff.def.GetModExtension<DefModextension_Hediff>() is { } modExt)
        {
            TryHealRandomInjury(__instance, pawn, modExt.repairRate / GenDate.TicksPerDay * healTickInterval);
        }

        if (pawn.CurrentBed() is not Building_BaseMechanoidPlatform)
        {
            return;
        }

        var platform = (Building_BaseMechanoidPlatform)pawn.CurrentBed();

        if (platform.RepairActive && platform.CanHealNow())
        {
            if (__instance.hediffSet.HasNaturallyHealingInjury() &&
                !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing))
            {
                TryHealRandomInjury(__instance, pawn,
                    platform.GetStatValue(WTH_DefOf.WTH_RepairRate) * healTickInterval / GenDate.TicksPerDay, platform);
            }
        }

        {
            if (__instance.hediffSet.HasNaturallyHealingInjury() || !platform.RegenerateActive ||
                !(platform.refuelableComp.Fuel >= 4f)) //TODO: no magic number
            {
                return;
            }
        }

        TryRegeneratePart(pawn, platform);
        TryRearmWeapon(pawn, platform);
        RegainWeapon(pawn);
    }

    private static void SelfDestruct(Pawn pawn)
    {
        GenExplosion.DoExplosion(pawn.Position, pawn.Map, 4.5f, DamageDefOf.Bomb, pawn, DamageDefOf.Bomb.defaultDamage,
            DamageDefOf.Bomb.defaultArmorPenetration, DamageDefOf.Bomb.soundExplosion);
        pawn.jobs.startingNewJob = false;
        var reactorPart = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(r => r.def.defName == "Reactor");
        var guard = 0;
        while (!pawn.Dead && guard < 10)
        {
            if (reactorPart != null)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Bomb, reactorPart.def.GetMaxHealth(pawn), 9999, -1, null,
                    reactorPart));
            }

            guard++;
        }

        if (!pawn.Dead)
        {
            Log.Warning(
                $"Pawn {pawn.Name} should have died from self destruct but didn't. This should never happen, so please report this to the author of What the Hack!?");
        }
    }

    private static void RegainWeapon(Pawn pawn)
    {
        if (pawn.equipment.Primary == null)
        {
            PawnWeaponGenerator.TryGenerateWeaponFor(pawn, new PawnGenerationRequest(pawn.kindDef));
        }
    }

    private static void TryRearmWeapon(Pawn pawn, Building_BaseMechanoidPlatform platform)
    {
        var possibleTurrets = pawn.Map.thingGrid.ThingsListAt(pawn.Position)
            .Where(thing => thing is Building);
        if (!possibleTurrets.Any())
        {
            return;
        }

        foreach (var possibleTurret in possibleTurrets)
        {
            var mountableTurret = possibleTurret.TryGetComp<CompMountable>();
            if (mountableTurret == null)
            {
                continue;
            }

            if (mountableTurret.MountedTo != pawn)
            {
                continue;
            }

            var turret = (Building_TurretGun)mountableTurret.parent;
            var refuelable = turret.TryGetComp<CompRefuelable>();
            if (refuelable == null)
            {
                continue;
            }

            if (refuelable.IsFull)
            {
                continue;
            }

            refuelable.fuel = Math.Min(refuelable.fuel + 10f, refuelable.Props.fuelCapacity);
            platform.refuelableComp.ConsumeFuel(1f);
        }
    }

    private static void TryRegeneratePart(Pawn pawn, Building_BaseMechanoidPlatform platform)
    {
        var hediff = FindBiggestMissingBodyPart(pawn);
        if (hediff == null || pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RegeneratedPart))
        {
            return;
        }

        pawn.health.RemoveHediff(hediff);
        var partHealth = hediff.Part.def.GetMaxHealth(pawn);
        var fuelNeeded =
            Math.Min(4f, partHealth / 5f); //body parts with less health need less parts to regenerate, capped at 4. 

        platform.refuelableComp.ConsumeFuel(fuelNeeded);
        //Hediff_Injury injury = new Hediff_Injury();
        var addInjury = new DamageWorker_AddInjury();
        addInjury.Apply(
            new DamageInfo(WTH_DefOf.WTH_RegeneratedPartDamage, hediff.Part.def.GetMaxHealth(pawn) - 1, 0, -1, pawn,
                hediff.Part), pawn);
    }

    //almost literal copy vanilla CompUseEffect_FixWorstHealthCondition.FindBiggestMissingBodyPart, only returns the hediff instead. 
    private static Hediff_MissingPart FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
    {
        Hediff_MissingPart hediff = null;
        foreach (var current in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
        {
            if (!(current.Part.coverageAbsWithChildren >= minCoverage))
            {
                continue;
            }

            if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(current.Part))
            {
                continue;
            }

            if (hediff == null || current.Part.coverageAbsWithChildren > hediff.Part.coverageAbsWithChildren)
            {
                hediff = current;
            }
        }

        return hediff;
    }

    private static void TryHealRandomInjury(Pawn_HealthTracker __instance, Pawn pawn, float healAmount,
        Building_BaseMechanoidPlatform platform = null)
    {
        var hediffs =
            __instance.hediffSet.hediffs.Where(hediff => hediff is Hediff_Injury injury && injury.CanHealNaturally());
        if (!hediffs.Any())
        {
            return;
        }

        var hediff_Injury = hediffs.RandomElement();
        hediff_Injury.Heal(healAmount);
        if (pawn.Map != null && !pawn.Position.Fogged(pawn.Map))
        {
            FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, WTH_DefOf.WTH_Fleck_HealingCrossGreen);
        }

        platform?.refuelableComp.ConsumeFuel(platform.GetStatValue(WTH_DefOf.WTH_PartConsumptionRate) *
            healTickInterval / GenDate.TicksPerDay); //TODO no magic number
    }
}