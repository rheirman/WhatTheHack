using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobDriver_Mechanoid_Rest : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil layDownToil = Toils_LayDown.LayDown(TargetIndex.A, true, false, false, false);
            layDownToil.defaultCompleteMode = ToilCompleteMode.Never;
            layDownToil.AddPreTickAction(delegate
            {
                if (!pawn.health.hediffSet.HasNaturallyHealingInjury())
                {
                    Log.Message("ready for next toil!");
                    ReadyForNextToil();
                }
            });
            yield return layDownToil;
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {
                Log.Message("Starting next toil");
                pawn.CurJob.SetTarget(TargetIndex.C, new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
                this.rotateToFace = TargetIndex.C;

            };
            yield return toil;

        }
    }
}
