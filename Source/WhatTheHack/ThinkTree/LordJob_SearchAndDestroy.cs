using Verse.AI.Group;

namespace WhatTheHack.ThinkTree;

public class LordJob_SearchAndDestroy : LordJob
{
    public override StateGraph CreateGraph()
    {
        var graph = new StateGraph();
        var sdToil = new LordToil_SearchAndDestroy();
        graph.AddToil(sdToil);
        var endToil = new LordToil_End();
        graph.AddToil(endToil);
        var endTransition = new Transition(sdToil, endToil);
        endTransition.AddTrigger(new Trigger_Custom(delegate
        {
            var pawn = lord.ownedPawns[0];
            var result = !pawn.IsActivated();
            return result;
        }));
        graph.AddTransition(endTransition);
        return graph;
    }
}