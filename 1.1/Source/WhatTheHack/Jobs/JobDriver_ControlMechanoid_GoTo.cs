using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobDriver_ControlMechanoid_Goto : JobDriver
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
            Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            gotoCell.FailOn(() => pawn.UnableToControl() || this.Mech.DestroyedOrNull() || this.Mech.Downed);
            int radius = Utilities.GetRemoteControlRadius(pawn) / 2;
            gotoCell.AddPreTickAction(new Action(delegate {
                if(Utilities.QuickDistanceSquared(pawn.Position, Mech.Position) < radius * radius)
                {
                    pawn.pather.StopDead();
                    ReadyForNextToil();
                }
            }));
            yield return gotoCell;
        }
    }
}
