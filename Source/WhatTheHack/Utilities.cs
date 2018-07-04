using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack
{
    static class Utilities
    {
        public static bool IsAllowedInModOptions(String pawnName, Faction faction)
        {
            bool found = Base.factionRestrictions.Value.InnerList[faction.def.defName].TryGetValue(pawnName, out Record value);
            if (found && value.isSelected)
            {
                return true;
            }
            return false;
        }


        public static Building_HackingTable GetAvailableHackingTable(Pawn pawn, Pawn targetPawn)
        {
            return (Building_HackingTable)GenClosest.ClosestThingReachable(targetPawn.Position, targetPawn.Map, ThingRequest.ForDef(WTH_DefOf.WTH_HackingTable), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing b)
            {
                if (b is Building_HackingTable)
                {
                    Building_HackingTable ht = (Building_HackingTable)b;
                    if (ht.GetCurOccupant(Building_HackingTable.SLOTINDEX) == null)
                    {
                        return true;
                    }
                }
                return false;
            });

        }
        public static Building_MechanoidPlatform GetAvailableMechanoidPlatform(Pawn pawn, Pawn targetPawn)
        {
            return (Building_MechanoidPlatform)GenClosest.ClosestThingReachable(targetPawn.Position, targetPawn.Map, ThingRequest.ForDef(WTH_DefOf.WTH_MechanoidPlatform), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing b)
            {

                if (b is Building_MechanoidPlatform)
                {
                    Building_MechanoidPlatform platform = (Building_MechanoidPlatform)b;
                    if (platform.GetCurOccupant(Building_MechanoidPlatform.SLOTINDEX) == null)
                    {
                        return true;
                    }
                }
                return false;
            });
        }
        public static float QuickDistance(IntVec3 a, IntVec3 b)
        {
            float arg_1D_0 = (float)(a.x - b.x);
            float num = (float)(a.z - b.z);
            return (float)Math.Sqrt(arg_1D_0 * arg_1D_0 + num * num);
        }
        public static float QuickDistanceSquared(IntVec3 a, IntVec3 b)
        {
            float arg_1D_0 = (float)(a.x - b.x);
            float num = (float)(a.z - b.z);
            return arg_1D_0 * arg_1D_0 + num * num;
        }
    }

}
