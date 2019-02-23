using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;
using WhatTheHack.Storage;

namespace WhatTheHack.ThinkTree
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
                bool shouldEnd = (mech == null || !mech.Spawned || mech.Dead || mech.Downed || pawn.UnableToControl());
                if (shouldEnd)
                {
                    if(mech != null)
                    {
                        ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                        mechData.remoteControlLink = null;
                    }
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                    pawnData.remoteControlLink = null;

                }
                return shouldEnd;
            }));
            graph.AddToil(endToil);
            graph.AddTransition(endTransition);

            return graph;
        }


    }
}
