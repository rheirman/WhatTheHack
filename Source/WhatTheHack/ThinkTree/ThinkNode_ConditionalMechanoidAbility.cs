using Verse;
using Verse.AI;

namespace WhatTheHack.ThinkTree;

public class ThinkNode_ConditionalMechanoidAbility : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        return pawn.IsHacked() && pawn.HasMechAbility();
    }
}