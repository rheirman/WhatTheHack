using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;
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

        public bool TryAddPawnForModification(Pawn pawn, RecipeDef recipeDef)
        {
            if((!pawn.IsHacked() || (pawn.IsHacked() && pawn.Faction != Faction.OfPlayer)))
            {

                Bill_Medical bill = new Bill_Medical(recipeDef);
                IEnumerable<BodyPartRecord> bodyparts = RecipeUtility.GetPartsToApplyOn(pawn, bill.recipe);
                if(bodyparts.Count() == 0)
                {
                    return false;
                }
                pawn.health.surgeryBills.AddBill(bill);
                bill.Part = bodyparts.First();
            }
            Need_Power powerNeed = pawn.needs.TryGetNeed<Need_Power>();
            if (powerNeed != null)
            {
                //discharge mech battery so pawns can work safely. 
                powerNeed.CurLevel = 0;
            }

            pawn.jobs.TryTakeOrderedJob(new Job(WTH_DefOf.WTH_Mechanoid_Rest, this));
            if (pawn.jobs.curDriver != null)
            {
                pawn.jobs.posture = PawnPosture.LayingInBed;
            }
            return true;
        }
        public bool HasPowerNow()
        {
            return this.powerComp != null && this.powerComp.PowerOn;
        }

    }
}
