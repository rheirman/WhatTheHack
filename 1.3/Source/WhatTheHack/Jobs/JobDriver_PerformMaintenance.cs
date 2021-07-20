using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs
{
    class JobDriver_PerformMaintenance : JobDriver
    {
        protected Pawn Deliveree
        {
            get
            {
                return (Pawn)this.job.targetA.Thing;
            }
        }
        protected Thing PartUsed
		{
			get
			{
				return this.job.targetB.Thing;
			}
		}
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.Deliveree;
            Job job = this.job;
            if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            else
            {

                int numReservable = this.pawn.Map.reservationManager.CanReserveStack(this.pawn, this.PartUsed, 10, null, false);
                if (numReservable > 0 && Deliveree.needs != null && Deliveree.needs.TryGetNeed<Need_Maintenance>() is Need_Maintenance need)
                {
                    pawn = this.pawn;
                    target = this.PartUsed;
                    job = this.job;
                    int maxPawns = 10;
                    int stackCount = Mathf.Min(numReservable, need.PartsNeededToRestore());
                    if (pawn.Reserve(target, job, maxPawns, stackCount, null, errorOnFailed))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(delegate
            {
                return !Deliveree.OnBaseMechanoidPlatform();
            });
            Need_Maintenance need = Deliveree.needs.TryGetNeed<Need_Maintenance>() as Need_Maintenance;
            Toil reserveParts = null;
            reserveParts = ReserveParts(TargetIndex.B, need).FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return reserveParts;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return PickupParts(TargetIndex.B, need).FailOnDestroyedOrNull(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveParts, TargetIndex.B, TargetIndex.None, true, null);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            int duration = (int)(1f / this.pawn.GetStatValue(WTH_DefOf.WTH_HackingMaintenanceSpeed, true) * 600f);
            EffecterDef effect = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault((EffecterDef ed) => ed.defName == "Repair");
            yield return Toils_General.Wait(duration, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).WithEffect(effect, TargetIndex.A);
            yield return FinalizeMaintenance(this.Deliveree, need, reserveParts);
        }

        private static Toil FinalizeMaintenance(Pawn targetPawn, Need_Maintenance need, Toil jumpToIfFailed)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {

                Pawn actor = toil.actor;
                float combatPowerCapped = targetPawn.kindDef.combatPower <= 10000 ? targetPawn.kindDef.combatPower : 300;
                float successChance = actor.GetStatValue(WTH_DefOf.WTH_HackingSuccessChance, true);
                if (Rand.Chance(successChance))
                {
                    actor.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.5f, false);
                    actor.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.5f, false);
                    need.RestoreUsingParts(actor.carryTracker.CarriedThing.stackCount);
                    Thing part = actor.CurJob.targetB.Thing;
                    if (!part.Destroyed)
                    {
                        part.Destroy(DestroyMode.Vanish);
                    }
                }
                else
                {
                    actor.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * 0.25f, false);
                    actor.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * 0.25f, false);
                    MoteMaker.ThrowText((actor.DrawPos + targetPawn.DrawPos) / 2f, actor.Map, "WTH_TextMote_MaintenanceFailed".Translate(new object[]{ successChance.ToStringPercent() }), 8f);
                    Thing part = actor.CurJob.targetB.Thing;
                    if (!part.Destroyed)
                    {
                        part.Destroy(DestroyMode.Vanish);
                    }
                    actor.jobs.curDriver.JumpToToil(jumpToIfFailed);
                }
            };
            return toil;
        }
        public static Toil ReserveParts(TargetIndex ind, Need_Maintenance need)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(ind).Thing;
                int num = actor.Map.reservationManager.CanReserveStack(actor, thing, 10, null, false);
                if (num <= 0 || !actor.Reserve(thing, curJob, 10, Mathf.Min(num, need.PartsNeededToRestore()), null, true))
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.atomicWithPrevious = true;
            return toil;
        }
        public static Toil PickupParts(TargetIndex ind, Need_Maintenance need)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(ind).Thing;
                int partsNeeded = need.PartsNeededToRestore();
                if (actor.carryTracker.CarriedThing != null)
                {
                    partsNeeded -= actor.carryTracker.CarriedThing.stackCount;
                }
                int partsFound = Mathf.Min(actor.Map.reservationManager.CanReserveStack(actor, thing, 10, null, false), partsNeeded);
                if (partsFound > 0)
                {
                    actor.carryTracker.TryStartCarry(thing, partsFound, true);
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
}
