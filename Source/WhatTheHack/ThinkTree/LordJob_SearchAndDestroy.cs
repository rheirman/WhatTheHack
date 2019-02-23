using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace WhatTheHack.ThinkTree
{
    public class LordJob_SearchAndDestroy : LordJob
    {
        public override StateGraph CreateGraph()
        {
            StateGraph graph = new StateGraph();
            LordToil_SearchAndDestroy sdToil = new LordToil_SearchAndDestroy();
            graph.AddToil(sdToil);
            LordToil_End endToil = new LordToil_End();
            graph.AddToil(endToil);
            Transition endTransition = new Transition(sdToil, endToil);
            endTransition.AddTrigger(new Trigger_Custom(delegate {
                Pawn pawn = this.lord.ownedPawns[0]; 
                bool result = !pawn.IsActivated();
                return result;
            }));
            graph.AddTransition(endTransition);
            return graph;
        }
    }
}
