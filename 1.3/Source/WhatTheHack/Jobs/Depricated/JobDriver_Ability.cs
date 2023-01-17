using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    /**
     * This class is deprecated but removing it would cause old saves to break.
     **/
    class JobDriver_Ability : JobDriver
    {
        public bool finished = false;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private Pawn TargetPawn
        {
            get
            {
                return (Pawn)TargetA;
            }
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            if(TargetA != null)
            {
                //this.FailOnDespawnedOrNull(TargetIndex.A);
                //this.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
                //this.FailOn(() => pawn.Dead);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                if (TargetA != pawn && !TargetPawn.Faction.HostileTo(pawn.Faction))
                {
                    yield return Toils_General.WaitWith(TargetIndex.A, job.count, true, true);
                }
                else
                {
                    yield return Toils_General.Wait(job.count).WithProgressBarToilDelay(TargetIndex.A);
                }
                yield return Toils_General.Do(delegate {
                    finished = true;                        //ended = true;
                });
            }
        }
    }
}
