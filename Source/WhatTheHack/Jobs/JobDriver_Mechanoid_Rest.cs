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
            Log.Message("TryMakePreToilReservations called");
            if (pawn.HasReserved(this.RestingPlace))
            {
                Log.Message("Had already reserved target");
                return true;
            }
            //pawn.reserve
            bool result = this.pawn.Reserve(this.RestingPlace, this.job, 1, -1, null);
            Log.Message("TryMakePreToilReservations result: " + result);
            return result;
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Message("MakeNewToils called!");
            //this.AddFinishAction(new Action(delegate { Log.Message("finish action called for job!");  }));
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
            yield return Toils_Bed.GotoBed(TargetIndex.A);

            //goToPlatform.AddPreInitAction(new Action(delegate { Log.Message("first toil pre-initaction"); }));
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {
                Log.Message("InitAction called for mech");
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
                
                
                if (pawn.ownership == null || pawn.ownership.OwnedBed != RestingPlace)
                {
                   ReadyForNextToil();
                }
                
                
                if ((pawn.health.hediffSet.HasNaturallyHealingInjury() || (pawn.OnHackingTable() && HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn))))
                {
                    pawn.jobs.posture = PawnPosture.LayingInBed;
                }
                else
                {
                    pawn.jobs.posture = PawnPosture.Standing;
                    RotateToSouth();
                }
            };
            //toil.FailOn(() => pawn.ownership.OwnedBed != RestingPlace);
            yield return toil;

        }

        private void RotateToSouth()
        {
            pawn.CurJob.SetTarget(TargetIndex.C, new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
            this.rotateToFace = TargetIndex.C;
        }
    }
}
