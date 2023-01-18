using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs;

internal class JobDriver_ControlMechanoid_Goto : JobDriver
{
    private Pawn Mech => pawn.RemoteControlLink();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        var gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        gotoCell.FailOn(() => pawn.UnableToControl() || Mech.DestroyedOrNull() || Mech.Downed);
        var radius = Utilities.GetRemoteControlRadius(pawn) / 2;
        gotoCell.AddPreTickAction(delegate
        {
            if (!(Utilities.QuickDistanceSquared(pawn.Position, Mech.Position) < radius * radius))
            {
                return;
            }

            pawn.pather.StopDead();
            ReadyForNextToil();
        });
        yield return gotoCell;
    }
}