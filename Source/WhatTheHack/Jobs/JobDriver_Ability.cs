using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobDriver_Ability : JobDriver
    {
        public bool finished = false;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if(TargetA != null)
            {
                this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
                this.FailOnNotCasualInterruptible(TargetIndex.A);
                this.FailOn(() => pawn.Dead);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                yield return Toils_General.Do(delegate {
                    Log.Message("before wait toil called for mech");
                });
                if (TargetA != pawn)
                {
                    yield return Toils_General.WaitWith(TargetIndex.A, job.count, true, true);
                }
                else
                {
                    yield return Toils_General.Wait(job.count).WithProgressBarToilDelay(TargetIndex.A);
                }
                yield return Toils_General.Do(delegate {
                    Log.Message("finish toil called for mech");
                    finished = true;                        //ended = true;
                });
            }
        }
    }
}
