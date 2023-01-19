using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

namespace WhatTheHack;

internal static class Utilities
{
    public static void ThrowStaticText(Vector3 loc, Map map, string text, Color color,
        float timeBeforeStartFadeout = -1f)
    {
        var intVec = loc.ToIntVec3();
        if (!intVec.InBounds(map))
        {
            return;
        }

        var moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text);
        moteText.exactPosition = loc;
        moteText.text = text;
        moteText.textColor = color;
        if (timeBeforeStartFadeout >= 0f)
        {
            moteText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
        }

        GenSpawn.Spawn(moteText, intVec, map);
    }


    public static List<TransferableOneWay> LinkPortablePlatforms(List<TransferableOneWay> transferables)
    {
        var pawns = TransferableUtility.GetPawnsFromTransferables(transferables);

        bool IsChargingPlatform(Thing t)
        {
            return t != null && t.GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform;
        }

        var chargingPlatformTows =
            transferables.FindAll(x => x.CountToTransfer > 0 && x.HasAnyThing && IsChargingPlatform(x.AnyThing));
        var unused = new List<Building_PortableChargingPlatform>();

        foreach (var tow in chargingPlatformTows)
        {
            foreach (var t in tow.things)
            {
                var platform = (Building_PortableChargingPlatform)t.GetInnerIfMinified();
                platform.CaravanPawn = null;
            }
        }

        //Find and assign platform for each pawn. 
        foreach (var pawn in pawns)
        {
            if (!pawn.IsHacked() || pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
            {
                continue;
            }

            var foundPlatform = false;
            for (var j = 0; j < chargingPlatformTows.Count && !foundPlatform; j++)
            {
                for (var i = 0; i < chargingPlatformTows[j].things.Count && !foundPlatform; i++)
                {
                    var platform =
                        (Building_PortableChargingPlatform)chargingPlatformTows[j].things[i].GetInnerIfMinified();
                    if (platform is not { CaravanPawn: null })
                    {
                        continue;
                    }

                    platform.CaravanPawn = pawn;
                    var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                    pawnData.caravanPlatform = platform;
                    foundPlatform = true;
                }
            }
        }

        return transferables;
    }

    public static int GetRemoteControlRadius(Pawn pawn)
    {
        var apparel =
            pawn.apparel.WornApparel.FirstOrDefault(app => app.def == WTH_DefOf.WTH_Apparel_MechControllerBelt);
        if (apparel is RemoteController rc)
        {
            return rc.ControlRadius;
        }

        return 0;
    }

    public static bool IsBelt(ApparelProperties apparel)
    {
        //Apparel is a belt when it can only be attached to a waist and nothing else. 
        return apparel != null && apparel.layers.Contains(ApparelLayerDefOf.Belt);
    }

    public static bool IsAllowedInModOptions(string pawnName, Faction faction)
    {
        var found = Base.factionRestrictions.Value.InnerList[faction.def.defName].TryGetValue(pawnName, out var value);
        return found && value.isSelected;
    }


    public static Building_HackingTable GetAvailableHackingTable(Pawn pawn, Pawn targetPawn)
    {
        return (Building_HackingTable)GenClosest.ClosestThingReachable(targetPawn.Position, targetPawn.Map,
            ThingRequest.ForDef(WTH_DefOf.WTH_HackingTable), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f,
            delegate(Thing b)
            {
                if (b is not Building_HackingTable ht ||
                    ht.TryGetComp<CompFlickable>() is not { SwitchIsOn: true } ||
                    b.IsBurning() ||
                    b.IsForbidden(pawn) ||
                    !pawn.CanReserve(b))
                {
                    return false;
                }

                return !(ht.GetCurOccupant(Building_HackingTable.SLOTINDEX) is { } pawnOnTable &&
                         pawnOnTable.OnHackingTable());
            });
    }

    public static Building_BaseMechanoidPlatform GetAvailableMechanoidPlatform(Pawn pawn, Pawn targetPawn)
    {
        return (Building_BaseMechanoidPlatform)GenClosest.ClosestThingReachable(targetPawn.Position, targetPawn.Map,
            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn),
            9999f, delegate(Thing b)
            {
                if (b is not Building_BaseMechanoidPlatform platform ||
                    b.IsBurning() ||
                    b.IsForbidden(targetPawn) ||
                    !targetPawn.CanReserve(b) ||
                    (targetPawn.ownership.OwnedBed != null || platform.CompAssignableToPawn.AssignedPawns.Any()) &&
                    !platform.CompAssignableToPawn.AssignedPawns.Contains(targetPawn))
                {
                    return false;
                }

                var flickable = platform.TryGetComp<CompFlickable>();
                return flickable is not { SwitchIsOn: false };
            });
    }

    public static float QuickDistance(IntVec3 a, IntVec3 b)
    {
        float arg_1D_0 = a.x - b.x;
        float num = a.z - b.z;
        return (float)Math.Sqrt((arg_1D_0 * arg_1D_0) + (num * num));
    }

    public static float QuickDistanceSquared(IntVec3 a, IntVec3 b)
    {
        float arg_1D_0 = a.x - b.x;
        float num = a.z - b.z;
        return (arg_1D_0 * arg_1D_0) + (num * num);
    }


    public static void CalcDaysOfFuel(List<TransferableOneWay> transferables)
    {
        var numMechanoids = 0;
        var fuelAmount = 0f;
        var fuelConsumption = 0f;
        var numPlatforms = 0;
        float daysOfFuel = 0;
        var daysOfFuelReason = new StringBuilder();

        foreach (var tow in transferables)
        {
            if (tow.ThingDef is { race.IsMechanoid: true } && tow.AnyThing is Pawn pawn && pawn.IsHacked() &&
                !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
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

            if (tow.ThingDef == WTH_DefOf.WTH_PortableChargingPlatform)
            {
                numPlatforms += tow.CountToTransfer;
            }
        }

        CalcDaysOfFuel(numMechanoids, fuelAmount, ref fuelConsumption, numPlatforms, ref daysOfFuel, daysOfFuelReason);
    }

    public static void InitWorkTypesAndSkills(Pawn pawn, ExtendedPawnData pawnData)
    {
        if (pawnData.workTypes == null)
        {
            pawnData.workTypes = new List<WorkTypeDef>();
        }

        if (pawn.skills == null)
        {
            pawn.skills = new Pawn_SkillTracker(pawn);
        }

        if (pawn.workSettings == null)
        {
            pawn.workSettings = new Pawn_WorkSettings(pawn);
            pawn.workSettings.EnableAndInitialize();
        }

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

        if (pawn.workSettings == null)
        {
            return;
        }

        var huntingWorkType = WorkTypeDefOf.Hunting;
        if (pawnData.workTypes == null)
        {
            pawnData.workTypes = new List<WorkTypeDef>();
        }

        pawnData.workTypes.Add(huntingWorkType);
        pawn.workSettings.SetPriority(huntingWorkType, 3);
    }

    public static void CalcDaysOfFuel(int numMechanoids, float fuelAmount, ref float fuelConsumption, int numPlatforms,
        ref float daysOfFuel, StringBuilder daysOfFuelReason)
    {
        if (numMechanoids == 0)
        {
            return;
        }

        daysOfFuelReason.AppendLine("WTH_Explanation_NumMechs".Translate() + ": " + numMechanoids);
        daysOfFuelReason.AppendLine("WTH_Explanation_NumPlatforms".Translate() + ": " + numPlatforms);
        if (numPlatforms >= numMechanoids)
        {
            var consumptionRate = WTH_DefOf.WTH_PortableChargingPlatform.GetCompProperties<CompProperties_Refuelable>()
                .fuelConsumptionRate;
            fuelConsumption = numMechanoids * consumptionRate;
            daysOfFuel = fuelAmount / fuelConsumption;
            daysOfFuelReason.AppendLine("WTH_Explanation_FuelConsumption".Translate() + ": " +
                                        fuelConsumption.ToString("0.#"));
            daysOfFuelReason.AppendLine("WTH_Explanation_TotalFuel".Translate() + ": " + fuelAmount.ToString("0.#"));
            daysOfFuelReason.AppendLine(
                $"{"WTH_Explanation_DaysOfFuel".Translate() + ": " + fuelAmount.ToString("0.#") + " /( "}{numMechanoids} * {consumptionRate}) = {daysOfFuel:0.#}");
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

        return pawn.skills != null;
    }


    private static bool StatAllowed(StatDef stat)
    {
        var found = false;
        if (!stat.skillNeedFactors.NullOrEmpty())
        {
            foreach (var sn in stat.skillNeedFactors)
            {
                if (Base.allowedMechSkills.Contains(sn.skill))
                {
                    found = true;
                }
            }
        }

        if (stat.skillNeedOffsets.NullOrEmpty())
        {
            return found;
        }

        foreach (var sn in stat.skillNeedOffsets)
        {
            if (Base.allowedMechSkills.Contains(sn.skill))
            {
                found = true;
            }
        }

        return found;
    }
}