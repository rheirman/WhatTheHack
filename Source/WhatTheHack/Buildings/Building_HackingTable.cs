using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Recipes;

namespace WhatTheHack.Buildings
{
    public class Building_HackingTable : Building , IBillGiver
    {
        private Pawn occupiedByInt = null;
        public Pawn OccupiedBy { get => occupiedByInt;}

        public BillStack BillStack { get; }
        public Building_HackingTable() => BillStack = new BillStack(this);

        

        public IEnumerable<IntVec3> IngredientStackCells => throw new NotImplementedException();

        public bool CurrentlyUsableForBills()
        {
            return OccupiedBy != null;
        }

        public bool TryAddPawnForModification(Pawn pawn, RecipeDef recipeDef)
        {
            if(OccupiedBy == null && !pawn.health.hediffSet.HasHediff(recipeDef.addsHediff))
            {
                occupiedByInt = pawn;
                Bill_Medical bill = new Bill_Medical(recipeDef);
                IEnumerable<BodyPartRecord> bodyparts = RecipeUtility.GetPartsToApplyOn(pawn, bill.recipe);
                if(bodyparts.Count() == 0)
                {
                    return false;
                }
                pawn.health.surgeryBills.AddBill(bill);
                bill.Part = bodyparts.First();
                return true;
            }
            return false;
        }

        public IntVec3 GetLyingSlotPos() 
        {
            var index = 2;
            var cellRect = this.OccupiedRect();
            if (Rotation == Rot4.North)
            {
                return new IntVec3(cellRect.minX + index, Position.y, cellRect.minZ);
            }
            if (Rotation == Rot4.East)
            {
                return new IntVec3(cellRect.minX, Position.y, cellRect.maxZ - index);
            }
            return Rotation == Rot4.South ? new IntVec3(cellRect.minX + index, Position.y, cellRect.maxZ) : new IntVec3(cellRect.maxX, Position.y, cellRect.maxZ - index);
        }

    }
}
