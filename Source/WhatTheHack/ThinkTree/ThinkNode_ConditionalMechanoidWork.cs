using Verse;
using Verse.AI;

namespace WhatTheHack.ThinkTree;

public class ThinkNode_ConditionalMechanoidWork : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        var result = pawn.IsHacked() && pawn.workSettings != null && pawn.CanStartWorkNow() && !pawn.Drafted;
        return result;
    }
}