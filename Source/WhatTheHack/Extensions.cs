using Harmony;
using RimWorld;
using RimWorld.Planet;
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
        public static void RemoveRemoteControlLink(this Pawn pawn)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            Pawn remoteControlLink = pawnData.remoteControlLink;
            if (remoteControlLink != null)
            {
                ExtendedPawnData linkPawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(remoteControlLink);
                linkPawnData.remoteControlLink = null;
            }
            pawnData.remoteControlLink = null;
        }
        public static bool IsHacked(this Pawn pawn)
        {
            if (pawn.health != null && (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TargetingHacked) || pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TargetingHackedPoorly)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool UnableToControl(this Pawn pawn)
        {
            return  pawn.DestroyedOrNull() || pawn.Downed || pawn.InMentalState || pawn.IsBurning() || pawn.RemoteControlLink() == null || pawn.apparel == null || pawn.apparel.WornApparel.FirstOrDefault((Apparel app) => app.def == WTH_DefOf.WTH_Apparel_MechControllerBelt) == null;
        }
        public static bool HasReplacedAI(this Pawn pawn)
        {
            if (pawn.health != null && (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_ReplacedAI)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Building_RogueAI ControllingAI(this Pawn pawn)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                return pawnData.controllingAI;
            }
            return null;
        }

        public static Pawn RemoteControlLink(this Pawn pawn)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if(store != null)
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                return pawnData.remoteControlLink;
            }
            return null;
        }

        public static BodyPartRecord TryGetReactor(this Pawn pawn)
        {
            BodyPartRecord result = null;
            if (pawn.health != null)
            {
                result = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord bpr) => bpr.def.defName == "Reactor");
            }
            return result;
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
            if (pawn.CurrentBed() != null && pawn.CurrentBed() is Building_HackingTable && pawn.jobs.posture == PawnPosture.LayingInBed)
            {
                return true;
            }
            return false;
        }
        public static bool OnBaseMechanoidPlatform(this Pawn pawn)
        {
            if (pawn.CurrentBed() != null && pawn.CurrentBed() is Building_BaseMechanoidPlatform)
            {
                return true;
            }
            return false;
        }
        public static bool HasValidCaravanPlatform(this Pawn pawn)
        {
            Caravan caravan = pawn.GetCaravan();
            if (caravan != null)
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                if (pawnData.caravanPlatform != null && pawnData.caravanPlatform.CaravanPawn == pawn)
                {
                    return true;
                }
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

        public static bool HasFuel(this Caravan caravan)
        {
            return caravan.AllThings.Any((Thing thing) => thing.def == ThingDefOf.Chemfuel && thing.stackCount > 0);
        }
        
    }
}
