using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Comps;
using WhatTheHack.Needs;

namespace WhatTheHack.Harmony;

//Spawn hacked mechnoids in enemy raids
[HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
public static class IncidentWorker_Raid_TryExecuteWorker
{
    [HarmonyPriority(Priority.First)]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionList = instructions.ToList();

        for (var i = 0; i < instructions.Count(); i++)
        {
            if (instructionList[i].operand as MethodInfo ==
                AccessTools.Method(typeof(PawnsArrivalModeWorker), "Arrive"))
            {
                yield return instructionList[i];
                yield return instructionList[i - 2];
                yield return instructionList[i - 1];
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(IncidentWorker_Raid_TryExecuteWorker).GetMethod("SpawnHackedMechanoids"));
            }
            else
            {
                yield return instructionList[i];
            }
        }
    }

    //returns pawns for compatibility reasons. 
    public static void SpawnHackedMechanoids(List<Pawn> pawns, IncidentParms parms)
    {
        if (pawns.Count == 0)
        {
            return;
        }

        if (parms.faction == Faction.OfMechanoids)
        {
            return;
        }

        var rand = new Random(DateTime.Now.Millisecond);
        if (rand.Next(0, 100) > Base.hackedMechChance)
        {
            return;
        }

        var minHackedMechPoints = Math.Min(Base.minHackedMechPoints, Base.maxHackedMechPoints);
        var maxMechPoints =
            parms.points * rand.Next(minHackedMechPoints, Base.maxHackedMechPoints) / 100f; //TODO: no magic numbers
        float cumulativePoints = 0;
        var map = parms.target as Map;
        var addedPawns = new List<Pawn>();

        while (cumulativePoints < maxMechPoints)
        {
            var points = cumulativePoints;
            var selectedPawns = from a in DefDatabase<PawnKindDef>.AllDefs
                where a.IsMechanoid() &&
                      points + a.combatPower < maxMechPoints &&
                      Utilities.IsAllowedInModOptions(a.race.defName, parms.faction) &&
                      (parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn ||
                       a.RaceProps.baseBodySize <= 1) //Only allow small mechs to use drop pods
                select a;

            selectedPawns.TryRandomElement(out var pawnKindDef);

            if (pawnKindDef != null)
            {
                var mechanoid = PawnGenerator.GeneratePawn(pawnKindDef, parms.faction);
                if (parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn)
                {
                    var loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8);
                    GenSpawn.Spawn(mechanoid, loc, map, parms.spawnRotation);
                }

                mechanoid.health.AddHediff(WTH_DefOf.WTH_TargetingHacked);
                mechanoid.health.AddHediff(WTH_DefOf.WTH_BackupBattery);
                var powerNeed = (Need_Power)mechanoid.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power);
                powerNeed.CurLevel = powerNeed.MaxLevel;
                if (ModsConfig.BiotechActive)
                {
                    mechanoid.needs.energy.curLevelInt = mechanoid.needs.energy.MaxLevel;
                }

                addedPawns.Add(mechanoid);
                cumulativePoints += pawnKindDef.combatPower;
                AddModules(mechanoid);
            }
            else
            {
                break;
            }
        }

        if (addedPawns.Count > 0 && !addedPawns[0].Spawned)
        {
            parms.raidArrivalMode.Worker.Arrive(addedPawns, parms);
        }

        pawns.AddRange(addedPawns);

        foreach (var pawn in pawns)
        {
            if (pawn.equipment == null)
            {
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
            }
        }
    }


    private static void AddModules(Pawn mechanoid)
    {
        var modules = new List<HediffDef>();
        foreach (var hediff in Base.allSpawnableModules)
        {
            modules.Add(hediff);
        }

        var i = 0;
        var count = modules.Count;
        while (i < count)
        {
            if (Rand.Chance(1 - modules[i].GetModExtension<DefModextension_Hediff>().spawnChance))
            {
                //Chance that the mod is NOT used
                modules.RemoveAt(i);
                count--;
            }
            else
            {
                i++;
            }
        }

        foreach (var hediff in modules)
        {
            if (hediff == WTH_DefOf.WTH_TurretModule)
            {
                var ignoreBodySize = mechanoid.def.GetModExtension<DefModExtension_TurretModule>() is
                    { ignoreMinBodySize: true };
                if (!ignoreBodySize && mechanoid.BodySize < 1.5f)
                {
                    continue;
                }

                ConfigureTurretModule(mechanoid);
            }

            if (hediff == WTH_DefOf.WTH_BeltModule)
            {
                if (mechanoid.verbTracker.PrimaryVerb.IsMeleeAttack)
                {
                    continue;
                }

                ConfigureBeltModule(mechanoid);
            }

            if (hediff == WTH_DefOf.WTH_RepairModule)
            {
                ConfigureRepairModule(mechanoid);
            }

            mechanoid.health.AddHediff(hediff);
        }
    }

    private static void ConfigureRepairModule(Pawn mechanoid)
    {
        mechanoid.InitializeComps();
        if (mechanoid.TryGetComp<CompRefuelable>() is { } comp)
        {
            comp.fuel = comp.Props.fuelCapacity;
            //Traverse.Create(comp).Field("fuel").SetValue(comp.Props.fuelCapacity);
        }
    }

    private static void ConfigureTurretModule(Pawn mechanoid)
    {
        mechanoid.health.AddHediff(WTH_DefOf.WTH_MountedTurret);
        var turretDef = DefDatabase<ThingDef>.AllDefs.Where(td => td.HasComp(typeof(CompMountable))).RandomElement();
        var thing = ThingMaker.MakeThing(turretDef, ThingDefOf.Steel);
        var comp = thing.GetInnerIfMinified().TryGetComp<CompMountable>();
        comp.MountToPawn(mechanoid);
    }

    private static void ConfigureBeltModule(Pawn mechanoid)
    {
        if (mechanoid.apparel == null)
        {
            mechanoid.apparel = new Pawn_ApparelTracker(mechanoid);
        }

        if (mechanoid.outfits == null)
        {
            if (mechanoid.story == null)
            {
                mechanoid.story = new Pawn_StoryTracker(mechanoid);
            }

            if (mechanoid.story.bodyType == null)
            {
                mechanoid.story.bodyType = BodyTypeDefOf.Fat; //any body type will do, so why not pick "Fat". 
            }
        }

        var belt = ThingMaker.MakeThing(Base.allBelts.RandomElement());
        mechanoid.apparel.Wear(belt as Apparel);
    }
    //do nothing
}