using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs;

internal class JobGiver_ControlMechanoid : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.RemoteControlLink() == null ||
            !(Utilities.QuickDistance(pawn.Position, pawn.RemoteControlLink().Position) <=
              Utilities.GetRemoteControlRadius(pawn) - 5f))
        {
            return null;
        }

        var job = new Job(WTH_DefOf.WTH_ControlMechanoid)
        {
            count = 1
        };
        return job;
    }
}