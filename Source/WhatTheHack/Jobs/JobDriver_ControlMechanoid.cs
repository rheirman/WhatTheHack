using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs;

internal class JobDriver_ControlMechanoid : JobDriver
{
    private Pawn Mech => pawn.RemoteControlLink();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        var toil = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Never
        };
        toil.FailOn(() => pawn.UnableToControl() || Mech.DestroyedOrNull() || Mech.Downed);
        toil.initAction = delegate { pawn.pather.StopDead(); };
        toil.tickAction = delegate
        {
            var radius = Utilities.GetRemoteControlRadius(pawn) - 5;
            if (Utilities.QuickDistanceSquared(pawn.Position, Mech.Position) > radius * radius)
            {
                ReadyForNextToil();
                return;
            }

            if (!GenSight.LineOfSight(pawn.Position, Mech.Position, Mech.Map))
            {
                return;
            }

            pawn.CurJob.targetC = Mech;
            rotateToFace = TargetIndex.C;
        };

        yield return toil;
    }
}