using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs;

internal class JobDriver_PerformMaintenance : JobDriver
{
    protected Pawn Deliveree => (Pawn)job.targetA.Thing;

    protected Thing PartUsed => job.targetB.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var localPawn = pawn;
        LocalTargetInfo target = Deliveree;
        var localJob = job;
        if (!localPawn.Reserve(target, localJob, 1, -1, null, errorOnFailed))
        {
            return false;
        }

        var numReservable = pawn.Map.reservationManager.CanReserveStack(pawn, PartUsed, 10);
        if (numReservable <= 0 || Deliveree.needs == null ||
            Deliveree.needs.TryGetNeed<Need_Maintenance>() is not { } need)
        {
            return false;
        }

        localPawn = pawn;
        target = PartUsed;
        localJob = job;
        var maxPawns = 10;
        var stackCount = Mathf.Min(numReservable, need.PartsNeededToRestore());
        return localPawn.Reserve(target, localJob, maxPawns, stackCount, null, errorOnFailed);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnAggroMentalState(TargetIndex.A);
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(() => !Deliveree.OnBaseMechanoidPlatform());
        var need = Deliveree.needs.TryGetNeed<Need_Maintenance>();
        var reserveParts = ReserveParts(TargetIndex.B, need).FailOnDespawnedNullOrForbidden(TargetIndex.B);
        yield return reserveParts;
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B);
        yield return PickupParts(TargetIndex.B, need).FailOnDestroyedOrNull(TargetIndex.B);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveParts, TargetIndex.B, TargetIndex.None, true);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
        var duration = (int)(1f / pawn.GetStatValue(WTH_DefOf.WTH_HackingMaintenanceSpeed) * 600f);
        var effect = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(ed => ed.defName == "Repair");
        yield return Toils_General.Wait(duration).FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch)
            .WithProgressBarToilDelay(TargetIndex.A).WithEffect(effect, TargetIndex.A);
        yield return FinalizeMaintenance(Deliveree, need, reserveParts);
    }

    private static Toil FinalizeMaintenance(Pawn targetPawn, Need_Maintenance need, Toil jumpToIfFailed)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var combatPowerCapped = targetPawn.kindDef.combatPower <= 10000 ? targetPawn.kindDef.combatPower : 300;
            var successChance = actor.GetStatValue(WTH_DefOf.WTH_HackingSuccessChance);
            if (Rand.Chance(successChance))
            {
                actor.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.5f);
                actor.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.5f);
                need.RestoreUsingParts(actor.carryTracker.CarriedThing.stackCount);
                var part = actor.CurJob.targetB.Thing;
                if (!part.Destroyed)
                {
                    part.Destroy();
                }
            }
            else
            {
                actor.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.25f);
                actor.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.25f);
                MoteMaker.ThrowText((actor.DrawPos + targetPawn.DrawPos) / 2f, actor.Map,
                    "WTH_TextMote_MaintenanceFailed".Translate(successChance.ToStringPercent()), 8f);
                var part = actor.CurJob.targetB.Thing;
                if (!part.Destroyed)
                {
                    part.Destroy();
                }

                actor.jobs.curDriver.JumpToToil(jumpToIfFailed);
            }
        };
        return toil;
    }

    public static Toil ReserveParts(TargetIndex ind, Need_Maintenance need)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var thing = curJob.GetTarget(ind).Thing;
            var num = actor.Map.reservationManager.CanReserveStack(actor, thing, 10);
            if (num <= 0 || !actor.Reserve(thing, curJob, 10, Mathf.Min(num, need.PartsNeededToRestore())))
            {
                toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        toil.atomicWithPrevious = true;
        return toil;
    }

    public static Toil PickupParts(TargetIndex ind, Need_Maintenance need)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var thing = curJob.GetTarget(ind).Thing;
            var partsNeeded = need.PartsNeededToRestore();
            if (actor.carryTracker.CarriedThing != null)
            {
                partsNeeded -= actor.carryTracker.CarriedThing.stackCount;
            }

            var partsFound = Mathf.Min(actor.Map.reservationManager.CanReserveStack(actor, thing, 10), partsNeeded);
            if (partsFound > 0)
            {
                actor.carryTracker.TryStartCarry(thing, partsFound);
            }

            curJob.count = partsNeeded - partsFound;
            if (thing.Spawned)
            {
                toil.actor.Map.reservationManager.Release(thing, actor, curJob);
            }

            curJob.SetTarget(ind, actor.carryTracker.CarriedThing);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}