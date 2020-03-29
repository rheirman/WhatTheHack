using HarmonyLib;
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
    class JobDriver_Mechanoid_Rest : JobDriver
    {
        protected Building_Bed RestingPlace
        {
            get
            {
                return (Building_Bed)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.HasReserved(this.RestingPlace))
            {
                return true;
            }
            //pawn.reserve
            bool result = this.pawn.Reserve(this.RestingPlace, this.job, 1, -1, null);
            return result;
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //this.AddFinishAction(new Action(delegate { Log.Message("finish action called for job!");  }));
            this.FailOnDespawnedOrNull(TargetIndex.A);
            if(RestingPlace is Building_BaseMechanoidPlatform)
            {
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
                yield return GoToPlatform(TargetIndex.A);
            }
            //goToPlatform.AddPreInitAction(new Action(delegate { Log.Message("first toil pre-initaction"); }));
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {

                if ((pawn.health.hediffSet.HasNaturallyHealingInjury() || pawn.OnHackingTable()))
                {
                    pawn.jobs.posture = PawnPosture.LayingInBed;
                }
                this.job.expiryInterval = 50;
                this.job.checkOverrideOnExpire = true;
                pawn.ClearAllReservations();
                pawn.Position = RestingPlace.GetSleepingSlotPos(RestingPlace is Building_HackingTable ? Building_HackingTable.SLOTINDEX : Building_BaseMechanoidPlatform.SLOTINDEX);

            };

            toil.tickAction = delegate
            {

                if (RestingPlace is Building_BaseMechanoidPlatform && pawn.ownership.OwnedBed != RestingPlace)
                {
                   ReadyForNextToil();
                }
                if (RestingPlace.TryGetComp<CompAssignableToPawn_Bed>() is CompAssignableToPawn_Bed compAssignable && compAssignable.AssignedPawns.FirstOrDefault((Pawn p) => p != pawn) is Pawn otherPawn)
                {
                    pawn.ownership.UnclaimBed();
                    ReadyForNextToil();
                }

                if (pawn.health.hediffSet.HasNaturallyHealingInjury() || pawn.OnHackingTable())
                {
                    pawn.jobs.posture = PawnPosture.LayingInBed;
                }
                else
                {
                    pawn.jobs.posture = PawnPosture.Standing;
                    RotateToSouth();
                }
            };

            yield return toil;

        }


        public static Toil GoToPlatform(TargetIndex bedIndex)
        {
            Toil gotoBed = new Toil();
            gotoBed.initAction = delegate
            {
                Pawn actor = gotoBed.actor;
                Building_Bed bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
                IntVec3 bedSleepingSlotPosFor = RestUtility.GetBedSleepingSlotPosFor(actor, bed);
                if (actor.Position == bedSleepingSlotPosFor)
                {
                    actor.jobs.curDriver.ReadyForNextToil();
                }
                else
                {
                    actor.pather.StartPath(bedSleepingSlotPosFor, PathEndMode.OnCell);
                }
            };
            gotoBed.tickAction = delegate
            {
                Pawn actor = gotoBed.actor;
                Building_Bed building_Bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
                Pawn curOccupantAt = building_Bed.GetCurOccupantAt(actor.pather.Destination.Cell);
                if (curOccupantAt != null && curOccupantAt != actor)
                {
                    actor.pather.StartPath(RestUtility.GetBedSleepingSlotPosFor(actor, building_Bed), PathEndMode.OnCell);
                }
            };
            gotoBed.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            gotoBed.FailOnPlatformNoLongerUsable(bedIndex);
            //gotoBed.FailOnBedNoLongerUsable(bedIndex);
            return gotoBed;
        }


        private void RotateToSouth()
        {
            pawn.CurJob.SetTarget(TargetIndex.C, new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
            this.rotateToFace = TargetIndex.C;
        }
    }
}
