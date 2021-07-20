using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs
{
    class JobDriver_ClearHackingTable : JobDriver
    {
        protected Pawn Takee
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }
        protected Building_HackingTable HackingTable
        {
            get
            {
                return (Building_HackingTable)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //return true;
            return this.pawn.Reserve(this.Takee, this.job, 1, -1, null) && this.pawn.Reserve(this.HackingTable, this.job, 1, -1, null);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = new Action(delegate {
                Takee.jobs.EndCurrentJob(JobCondition.InterruptForced);
                Takee.health.surgeryBills.Clear();
            });
            yield return toil;
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.B, TargetIndex.C);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell);

            //yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A)
            yield return new Toil
            {
                initAction = delegate
                {
                     this.pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Direct, out Thing thing, null);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
