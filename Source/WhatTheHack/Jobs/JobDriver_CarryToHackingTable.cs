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
    class JobDriver_CarryToHackingTable : JobDriver
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
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.Takee, this.job, 1, -1, null) && this.pawn.Reserve(this.HackingTable, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            //this.FailOn(() => !this.DropPod.Accepts(this.Takee));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.HackingTable.OccupiedBy != null).FailOn(() => !this.Takee.Downed).FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            //yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A)
            yield return new Toil
            {
                initAction = delegate
                {
                    HackingTable.OccupiedBy = Takee;
                    if (!Takee.IsHacked())
                    {
                        Log.Message("added hack mechanoid bill bill");
                        Takee.health.surgeryBills.AddBill(new Bill_Medical(WTH_DefOf.HackMechanoid));
                    }                 
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
