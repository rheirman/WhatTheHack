using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.ThinkTree;

public class ThinkNode_ConditionalMechanoidRest : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        if (pawn.Faction == Faction.OfPlayer && pawn.IsHacked() && !pawn.IsActivated() && !pawn.CanStartWorkNow())
        {
            return true;
        }

        if (pawn.ShouldRecharge() || pawn.ShouldBeMaintained())
        {
            return true;
        }

        if (HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) && pawn.OnHackingTable())
        {
            return true;
        }

        return false;
    }
}