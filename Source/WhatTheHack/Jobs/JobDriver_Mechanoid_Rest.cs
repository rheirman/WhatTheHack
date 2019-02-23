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
            return this.pawn.Reserve(this.RestingPlace, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil goToPlatform = Toils_Bed.GotoBed(TargetIndex.A);
            yield return goToPlatform;
            
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
            yield return toil;

        }

        private void RotateToSouth()
        {
            pawn.CurJob.SetTarget(TargetIndex.C, new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
            this.rotateToFace = TargetIndex.C;
        }
    }
}
