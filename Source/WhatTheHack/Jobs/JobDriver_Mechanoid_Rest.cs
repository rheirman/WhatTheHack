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

            Toil layDownToil = Toils_LayDown.LayDown(TargetIndex.A, true, false, false, false);
            layDownToil.defaultCompleteMode = ToilCompleteMode.Never;
            layDownToil.initAction = new Action(delegate {
                pawn.ClearAllReservations();
            });
            layDownToil.AddPreTickAction(delegate
            {
                if (!(pawn.health.hediffSet.HasNaturallyHealingInjury() || (pawn.OnHackingTable() && HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn))))
                {
                    ReadyForNextToil();
                }
            });
            yield return layDownToil;
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {
                pawn.jobs.posture = PawnPosture.Standing;
                pawn.Position = RestingPlace.GetSleepingSlotPos(RestingPlace is Building_HackingTable ? Building_HackingTable.SLOTINDEX : Building_BaseMechanoidPlatform.SLOTINDEX);
                pawn.CurJob.SetTarget(TargetIndex.C, new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
                this.rotateToFace = TargetIndex.C;
            };
            yield return toil;

        }
    }
}
