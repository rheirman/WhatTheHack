using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs;

internal class JobGiver_ControlMechanoid_Follow : ThinkNode_JobGiver
{
    private int FollowJobExpireInterval => 200;

    public override Job TryGiveJob(Pawn pawn)
    {
        var followee = pawn.RemoteControlLink();
        if (followee == null)
        {
            //Log.Warning(base.GetType() + "has null followee.");
            return null;
        }

        //if (!GenAI.CanInteractPawn(pawn, followee))
        // {
        //    return null;
        //  }
        var radius = Utilities.GetRemoteControlRadius(pawn) - 5f; //TODO: no magic number
        if (followee.Position.DistanceToSquared(pawn.Position) <= radius * radius)
        {
            return null;
        }

        if (followee.pather.Moving && followee.pather.curPath != null)
        {
            followee.pather.curPath.TryFindLastCellBeforeBlockingDoor(followee, out _);
        }

        var job = new Job(WTH_DefOf.WTH_ControlMechanoid_Goto, followee.Position)
        {
            expiryInterval = FollowJobExpireInterval,
            checkOverrideOnExpire = true
        };
        if (pawn.mindState.duty != null && pawn.mindState.duty.locomotion != LocomotionUrgency.None)
        {
            job.locomotionUrgency = pawn.mindState.duty.locomotion;
        }

        return job;
    }
}