using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Storage;

namespace WhatTheHack.ThinkTree
{
    class ThinkNode_ConditionalMechanoidWork : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            bool result = pawn.workSettings != null && pawn.CanStartWorkNow() && !pawn.Drafted;
            Log.Message("mech can start work: " + result);
            return result;
        }
    }
}
