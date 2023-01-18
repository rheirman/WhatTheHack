using Verse.AI.Group;

namespace WhatTheHack.ThinkTree;

public class LordJob_ControlMechanoid : LordJob
{
    public override StateGraph CreateGraph()
    {
        var graph = new StateGraph();
        var sdToil = new LordToil_ControlMechanoid();
        graph.AddToil(sdToil);
        var endToil = new LordToil_End();

        var endTransition = new Transition(sdToil, endToil);

        endTransition.AddTrigger(new Trigger_Custom(delegate
        {
            var pawn = lord.ownedPawns[0];
            var mech = pawn.RemoteControlLink();
            var shouldEnd = mech == null || !mech.Spawned || mech.Dead || mech.Downed || pawn.UnableToControl();
            if (!shouldEnd)
            {
                return false;
            }

            if (mech != null)
            {
                var mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                mechData.remoteControlLink = null;
            }

            var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.remoteControlLink = null;

            return true;
        }));
        graph.AddToil(endToil);
        graph.AddTransition(endTransition);

        return graph;
    }
}