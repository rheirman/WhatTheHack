using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Storage;

namespace WhatTheHack.Duties
{
    class ThinkNode_ConditionalWorker : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.workSettings != null;
        }
    }
}
