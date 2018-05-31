using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Recipes;

namespace WhatTheHack.Buildings
{
    public class Building_HackingTable : Building_Bed
    {
        //private Pawn occupiedByInt = null;
        //public Pawn OccupiedBy { get => occupiedByInt;}
        public CompPowerTrader powerComp;
        public const int SLOTINDEX = 2;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public static bool TryAddPawnForModification(Pawn pawn, RecipeDef recipeDef)
        {
            if(!pawn.IsHacked())
            {

                Bill_Medical bill = new Bill_Medical(recipeDef);
                IEnumerable<BodyPartRecord> bodyparts = RecipeUtility.GetPartsToApplyOn(pawn, bill.recipe);
                if(bodyparts.Count() == 0)
                {
                    return false;
                }
                pawn.health.surgeryBills.AddBill(bill);
                bill.Part = bodyparts.First();
                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.LayDown));
                
                if(pawn.jobs.curDriver != null)
                {
                    pawn.jobs.curDriver.layingDown = LayingDownState.LayingInBed;
                }
                return true;
            }
            return false;
        }
        public bool HasPowerNow()
        {
            return this.powerComp != null && this.powerComp.PowerOn;
        }

    }
}
