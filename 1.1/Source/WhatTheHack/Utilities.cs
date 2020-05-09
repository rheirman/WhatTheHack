using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

namespace WhatTheHack
{
    static class Utilities
    {
        public static void ThrowStaticText(Vector3 loc, Map map, string text, Color color, float timeBeforeStartFadeout = -1f)
        {
            IntVec3 intVec = loc.ToIntVec3();
            if (!intVec.InBounds(map))
            {
                return;
            }
            MoteText moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text, null);
            moteText.exactPosition = loc;
            moteText.text = text;
            moteText.textColor = color;
            if (timeBeforeStartFadeout >= 0f)
            {
                moteText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
            }
            GenSpawn.Spawn(moteText, intVec, map, WipeMode.Vanish);
        }



        public static List<TransferableOneWay> LinkPortablePlatforms(List<TransferableOneWay> transferables)
        {
            List<Pawn> pawns = TransferableUtility.GetPawnsFromTransferables(transferables);
            Predicate<Thing> isChargingPlatform = (Thing t) => t != null && t.GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform;

            List<TransferableOneWay> chargingPlatformTows = transferables.FindAll((TransferableOneWay x) => x.CountToTransfer > 0 && x.HasAnyThing && isChargingPlatform(x.AnyThing));
            List<Building_PortableChargingPlatform> platforms = new List<Building_PortableChargingPlatform>();

            foreach (TransferableOneWay tow in chargingPlatformTows)
            {
                foreach (Thing t in tow.things)
                {
                    Building_PortableChargingPlatform platform = (Building_PortableChargingPlatform)t.GetInnerIfMinified();
                    platform.CaravanPawn = null;
                }
            }

            //Find and assign platform for each pawn. 
            foreach (Pawn pawn in pawns)
            {
                if (pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
                {
                    bool foundPlatform = false;
                    for (int j = 0; j < chargingPlatformTows.Count && !foundPlatform; j++)
                    {
                        for (int i = 0; i < chargingPlatformTows[j].things.Count && !foundPlatform; i++)
                        {
                            Building_PortableChargingPlatform platform = (Building_PortableChargingPlatform)chargingPlatformTows[j].things[i].GetInnerIfMinified();
                            if (platform != null && platform.CaravanPawn == null)
                            {
                                platform.CaravanPawn = pawn;
                                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                                pawnData.caravanPlatform = platform;
                                foundPlatform = true;
                            }
                        }

                    }
                }
            }
            return transferables;
        }

        public static int GetRemoteControlRadius(Pawn pawn)
        {
            Apparel apparel = pawn.apparel.WornApparel.FirstOrDefault((Apparel app) => app.def == WTH_DefOf.WTH_Apparel_MechControllerBelt);
            if (apparel is RemoteController rc)
            {
                return rc.ControlRadius;
            }
            return 0;
        }

        public static bool IsBelt(ApparelProperties apparel)
        {
            //Apparel is a belt when it can only be attached to a waist and nothing else. 
            if (apparel != null && apparel.layers.Contains(ApparelLayerDefOf.Belt))
            {
                return true;
            }
            return false;
        }

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
                if (b is Building_HackingTable ht && 
                ht.TryGetComp<CompFlickable>() is CompFlickable flickable && flickable.SwitchIsOn && 
                !b.IsBurning() &&
                !b.IsForbidden(pawn) &&
                pawn.CanReserve(b))
                {
                    if (!(ht.GetCurOccupant(Building_HackingTable.SLOTINDEX) is Pawn pawnOnTable && pawnOnTable.OnHackingTable()))
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
                if (b is Building_BaseMechanoidPlatform platform &&
                !b.IsBurning() &&
                !b.IsForbidden(targetPawn) &&
                targetPawn.CanReserve(b) &&
                (targetPawn.ownership.OwnedBed == null && !platform.CompAssignableToPawn.AssignedPawns.Any()|| platform.CompAssignableToPawn.AssignedPawns.Contains(targetPawn)))
                {
                    CompFlickable flickable = platform.TryGetComp<CompFlickable>();
                    if(flickable != null && !flickable.SwitchIsOn)
                    {
                        return false;
                    }
                    return true;
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
                if (tow.ThingDef != null && tow.ThingDef.race != null && tow.ThingDef.race.IsMechanoid && tow.AnyThing is Pawn pawn && pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
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
                if(tow.ThingDef == WTH_DefOf.WTH_PortableChargingPlatform)
                {
                    numPlatforms += tow.CountToTransfer;
                }
            }
            CalcDaysOfFuel(numMechanoids, fuelAmount, ref fuelConsumption, numPlatforms, ref daysOfFuel, daysOfFuelReason);
        }
        public static void InitWorkTypesAndSkills(Pawn pawn, ExtendedPawnData pawnData)
        {

            if (pawn.skills != null)
            {
                if (pawn.skills.GetSkill(SkillDefOf.Shooting).Level == 0)
                {
                    pawn.skills.GetSkill(SkillDefOf.Shooting).Level = 8;
                }
                if (pawn.skills.GetSkill(SkillDefOf.Melee).Level == 0)
                {
                    pawn.skills.GetSkill(SkillDefOf.Melee).Level = 4;
                }
            }
            if (pawn.workSettings != null)
            {
                var huntingWorkType = WorkTypeDefOf.Hunting;
                if(pawnData.workTypes == null)
                {
                    // Log.Message("WTHACK: " + pawn.Name);
                    pawnData.workTypes = new List<WorkTypeDef>();
                }
                pawnData.workTypes.Add(huntingWorkType);
                pawn.workSettings.SetPriority(huntingWorkType, 3);
            }
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
                float consumptionRate = WTH_DefOf.WTH_PortableChargingPlatform.GetCompProperties<CompProperties_Refuelable>().fuelConsumptionRate;
                fuelConsumption = numMechanoids * consumptionRate;
                daysOfFuel = fuelAmount / fuelConsumption;
                daysOfFuelReason.AppendLine("WTH_Explanation_FuelConsumption".Translate() + ": " + fuelConsumption.ToString("0.#"));
                daysOfFuelReason.AppendLine("WTH_Explanation_TotalFuel".Translate() + ": " + fuelAmount.ToString("0.#"));
                daysOfFuelReason.AppendLine("WTH_Explanation_DaysOfFuel".Translate() + ": " + fuelAmount.ToString("0.#") + " /( " + numMechanoids + " * " + consumptionRate + ") = " + daysOfFuel.ToString("0.#"));
            }
            else
            {
                daysOfFuelReason.AppendLine("WTH_Explanation_NotEnoughPlatforms".Translate());
            }
            Base.Instance.daysOfFuel = daysOfFuel;
            Base.Instance.daysOfFuelReason = daysOfFuelReason.ToString();
        }

        public static bool ShouldGetStatValue(Pawn pawn, StatDef stat)
        {
            if (pawn.IsHacked() && pawn.skills != null)
            {
                return StatAllowed(stat);
            }
            else
            {
                return pawn.skills != null;
            }
        }


        private static bool StatAllowed(StatDef stat)
        {
            bool found = false;
            if (!stat.skillNeedFactors.NullOrEmpty())
            {
                foreach (SkillNeed sn in stat.skillNeedFactors)
                {
                    if (Base.allowedMechSkills.Contains(sn.skill))
                    {
                        found = true;
                    }
                }
            }
            if (!stat.skillNeedOffsets.NullOrEmpty())
            {
                foreach (SkillNeed sn in stat.skillNeedOffsets)
                {
                    if (Base.allowedMechSkills.Contains(sn.skill))
                    {
                        found = true;
                    }
                }
            }

            return found;
        }
    }

}
