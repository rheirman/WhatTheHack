using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Storage;

namespace WhatTheHack.ThinkTree
{
    public class ThinkNode_ConditionalMechanoidWork : ThinkNode_Conditional
    {
        public override bool Satisfied(Pawn pawn)
        {
            bool result = pawn.IsHacked() && pawn.workSettings != null && pawn.CanStartWorkNow() && !pawn.Drafted;
            return result;
        }
    }
}
