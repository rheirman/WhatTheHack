using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;
using WhatTheHack.Storage;

namespace WhatTheHack.Duties
{
    public class LordJob_ControlMechanoid : LordJob
    {
        public override StateGraph CreateGraph()
        {
            StateGraph graph = new StateGraph();
            LordToil_ControlMechanoid sdToil = new LordToil_ControlMechanoid();
            graph.AddToil(sdToil);
            LordToil_End endToil = new LordToil_End();
            Transition endTransition = new Transition(sdToil, endToil);
            endTransition.AddTrigger(new Trigger_Custom(delegate
            {
                Pawn pawn = this.lord.ownedPawns[0];
                Pawn mech = pawn.RemoteControlLink();
                return (mech == null || !mech.Spawned || mech.Dead || mech.Downed);
            }));
            graph.AddToil(endToil);
            graph.AddTransition(endTransition);

            return graph;
        }


    }
}
