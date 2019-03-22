using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.ThinkTree
{
    class ThinkNode_ConditionalMechanoidRest : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn) { 
            if (pawn.Faction == Faction.OfPlayer && pawn.IsHacked() && !pawn.IsActivated() && !pawn.CanStartWorkNow())
            {
                return true;
            }
            else if (HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) && pawn.health.surgeryBills.FirstShouldDoNow is Bill b && b.recipe == WTH_DefOf.WTH_HackMechanoid)
            {
                Log.Message("ThinkNode_ConditionalMechanoidRest satisfied for mech");
                return true;
            }
            else
            {
                Log.Message("meh :(");
                Log.Message("HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn): " + HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn));
                if(pawn.health.surgeryBills.FirstShouldDoNow is Bill bill)
                {
                    Log.Message("pawn.health.surgeryBills.FirstShouldDoNow is Bill b && b.recipe == WTH_DefOf.WTH_HackMechanoid: " + (bill.recipe == WTH_DefOf.WTH_HackMechanoid));

                }
                else
                {
                    Log.Message("pawn.health.surgeryBills.FirstShouldDoNow is no Bill");
                }
            }
            return false;
        }

    }
}
