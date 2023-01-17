using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

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
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Takee, this.job, 1, -1, null) && this.pawn.Reserve(this.HackingTable, this.job, 1, -1, null);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            //this.FailOn(() => !this.DropPod.Accepts(this.Takee));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.HackingTable.GetCurOccupant(Building_HackingTable.SLOTINDEX) != null && this.HackingTable.GetCurOccupant(Building_HackingTable.SLOTINDEX).OnHackingTable()).FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);

            //yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A)
            yield return new Toil
            {
                initAction = delegate
                {
                    this.pawn.carryTracker.TryDropCarriedThing(HackingTable.GetSleepingSlotPos(Building_HackingTable.SLOTINDEX), ThingPlaceMode.Direct, out Thing thing, null);
                    this.pawn.ClearAllReservations();
                    HackingTable.TryAddPawnForModification(Takee, WTH_DefOf.WTH_HackMechanoid);
                    Takee.Position = HackingTable.GetSleepingSlotPos(Building_HackingTable.SLOTINDEX);
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(Takee);
                    pawnData.isActive = false;
                    pawnData.canWorkNow = false;
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
