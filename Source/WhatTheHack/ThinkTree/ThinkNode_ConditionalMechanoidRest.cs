using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.ThinkTree
{
    class ThinkNode_ConditionalMechanoidRest : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn) { 
            if (pawn.Faction == Faction.OfPlayer && pawn.IsHacked() && !pawn.IsActivated() && !pawn.CanStartWorkNow()){
                return true;
            }
            return false;
        }

    }
}
