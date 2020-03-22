using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.ThinkTree
{
    class ThinkNode_ConditionalMechanoidAbility : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.IsHacked() && pawn.HasMechAbility();
        }
    }
}
