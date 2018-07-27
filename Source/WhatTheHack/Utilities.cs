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
                if (b is Building_HackingTable && !b.IsForbidden(pawn) && pawn.CanReserve(b))
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
        public static Building_BaseMechanoidPlatform GetAvailableMechanoidPlatform(Pawn pawn, Pawn targetPawn)
        {
            return (Building_BaseMechanoidPlatform)GenClosest.ClosestThingReachable(targetPawn.Position, targetPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing b)
            {
                if (b is Building_BaseMechanoidPlatform && !b.IsForbidden(pawn) && pawn.CanReserve(b))
                {
                    Building_BaseMechanoidPlatform platform = (Building_BaseMechanoidPlatform)b;
                    if (platform.GetCurOccupant(Building_BaseMechanoidPlatform.SLOTINDEX) == null)
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


        public static void CalcDaysOfFuel(List<TransferableOneWay> transferables)
        {
            int numMechanoids = 0;
            float fuelAmount = 0f;
            float fuelConsumption = 0f;
            int numPlatforms = 0;
            float daysOfFuel = 0;
            StringBuilder daysOfFuelReason = new StringBuilder();

            foreach (TransferableOneWay tow in transferables)
            {
                if (tow.ThingDef.race != null && tow.ThingDef.race.IsMechanoid)
                {
                    numMechanoids += tow.CountToTransfer;
                }
                if (tow.ThingDef == ThingDefOf.Chemfuel)
                {
                    fuelAmount += tow.CountToTransfer;
                }
                if (tow.ThingDef == ThingDefOf.MinifiedThing)
                {
                    if (tow.things[0].GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform)
                    {
                        numPlatforms += tow.CountToTransfer;
                    }

                }
            }
            CalcDaysOfFuel(numMechanoids, fuelAmount, ref fuelConsumption, numPlatforms, ref daysOfFuel, daysOfFuelReason);
        }

        public static void CalcDaysOfFuel(int numMechanoids, float fuelAmount, ref float fuelConsumption, int numPlatforms, ref float daysOfFuel, StringBuilder daysOfFuelReason)
        {
            if (numMechanoids == 0)
            {
                return;
            }
            daysOfFuelReason.AppendLine("WTH_Explanation_NumMechs".Translate() + ": " + numMechanoids);
            daysOfFuelReason.AppendLine("WTH_Explanation_NumPlatforms".Translate() + ": " + numPlatforms);
            if (numPlatforms >= numMechanoids)
            {
                fuelConsumption = numMechanoids * WTH_DefOf.WTH_PortableChargingPlatform.GetCompProperties<CompProperties_Refuelable>().fuelConsumptionRate;
                daysOfFuel = fuelAmount / fuelConsumption;
                daysOfFuelReason.AppendLine("WTH_Explanation_FuelConsumption".Translate() + ": " + fuelConsumption.ToString("0.#"));
                daysOfFuelReason.AppendLine("WTH_Explanation_TotalFuel".Translate() + ": " + fuelAmount.ToString("0.#"));
                daysOfFuelReason.AppendLine("WTH_Explanation_DaysOfFuel".Translate() + ": " + fuelAmount.ToString("0.#") + " /( " + numMechanoids + " * " + fuelConsumption + ") = " + daysOfFuel.ToString("0.#"));
            }
            else
            {
                daysOfFuelReason.AppendLine("WTH_Explanation_NotEnoughPlatforms".Translate());
            }
            Base.Instance.daysOfFuel = daysOfFuel;
            Base.Instance.daysOfFuelReason = daysOfFuelReason.ToString();
        }
    }

}
