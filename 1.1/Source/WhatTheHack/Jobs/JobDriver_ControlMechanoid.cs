using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobDriver_ControlMechanoid : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        private Pawn Mech
        {
            get
            {
                return pawn.RemoteControlLink();
            }
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOn(() => pawn.UnableToControl() || this.Mech.DestroyedOrNull() || this.Mech.Downed);
            toil.initAction = new Action(delegate
            {
                pawn.pather.StopDead();
            });
            toil.tickAction = new Action(delegate {

                int radius = Utilities.GetRemoteControlRadius(pawn) - 5;
                if (Utilities.QuickDistanceSquared(pawn.Position, this.Mech.Position) > radius * radius)
                {
                    ReadyForNextToil();
                    return;
                }
                if(GenSight.LineOfSight(pawn.Position, Mech.Position, Mech.Map, false, null, 0, 0))
                {
                    pawn.CurJob.targetC = this.Mech;
                    rotateToFace = TargetIndex.C;
                }
            });

            yield return toil;
        }
    }
}
