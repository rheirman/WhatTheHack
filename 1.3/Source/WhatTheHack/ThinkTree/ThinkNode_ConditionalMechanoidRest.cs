using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.ThinkTree
{
    public class ThinkNode_ConditionalMechanoidRest : ThinkNode_Conditional
    {
        public override bool Satisfied(Pawn pawn) { 
            if (pawn.Faction == Faction.OfPlayer && pawn.IsHacked() && !pawn.IsActivated() && !pawn.CanStartWorkNow())
            {
                return true;
            }
            else if (pawn.ShouldRecharge() || pawn.ShouldBeMaintained())
            {
                return true;
            }
            else if (HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) && pawn.OnHackingTable())
            {
                return true;
            }
            return false;
        }

    }
}
