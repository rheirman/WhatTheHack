using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse.AI.Group;

namespace WhatTheHack.ThinkTree
{
    class LordToil_SearchAndDestroy : LordToil
    {
        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }
        public override void UpdateAllDuties()

        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(WTH_DefOf.WTH_SearchAndDestroy);
            }
        }
    }
}
