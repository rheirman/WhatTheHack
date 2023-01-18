using Verse.AI;
using Verse.AI.Group;

namespace WhatTheHack.ThinkTree;

internal class LordToil_ControlMechanoid : LordToil
{
    public override bool AllowSatisfyLongNeeds => false;

    public override void UpdateAllDuties()

    {
        for (var i = 0; i < lord.ownedPawns.Count; i++)
        {
            lord.ownedPawns[i].mindState.duty = new PawnDuty(WTH_DefOf.WTH_ControlMechanoidDuty);
        }
    }
}