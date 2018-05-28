using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

namespace WhatTheHack
{
    public static class Extensions
    {
        public static bool IsHacked(this Pawn pawn)
        {
            if (pawn.health != null && (pawn.health.hediffSet.HasHediff(WTH_DefOf.TargetingHacked) || pawn.health.hediffSet.HasHediff(WTH_DefOf.TargetingHackedPoorly)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /*
        public static Building_HackingTable HackingTable(this Pawn pawn)
        {
            List<Thing> thingList = pawn.Position.GetThingList(pawn.Map);
            Building_HackingTable hackingTable = null;
            for (int i = 0; i < thingList.Count; i++)
            {
                hackingTable = (thingList[i] as Building_HackingTable);
                if (hackingTable != null)
                {
                    break;
                }
            }
            if (hackingTable == null)
            {
                return null;
            }
            if(hackingTable.GetCurOccupant(0) == pawn)
            {
                return hackingTable;
            }
            return null;
        }
        */
        /*
        public static bool OnHackingTable(this Pawn pawn)
        {
            if(pawn.HackingTable() != null)
            {
                return true;
            }
            return false;
        }
        */
        public static bool OnHackingTable(this Pawn pawn)
        {
            if (pawn.CurrentBed() != null && pawn.CurrentBed() is Building_HackingTable)
            {
                return true;
            }
            return false;
        }
        public static bool OnMechanoidPlatform(this Pawn pawn)
        {
            if (pawn.CurrentBed() != null && pawn.CurrentBed() is Building_MechanoidPlatform)
            {
                return true;
            }
            return false;
        }
        public static bool IsActivated(this Pawn pawn)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if(store != null)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);
                return pawnData.isActive;
            }
            return false;
        }
        /*
        public static bool CanTakeOrder(this Pawn pawn)
        {
            bool flagIsCreatureMine = pawn.Faction != null && pawn.Faction.IsPlayer;
            bool flagIsCreatureDraftable = (pawn.IsHacked());

            if (flagIsCreatureDraftable && flagIsCreatureMine)
            {
                return true;
            }
            return false;
        }
        */
        
    }
}
