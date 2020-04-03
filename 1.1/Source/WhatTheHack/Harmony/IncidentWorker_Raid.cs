using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Comps;
using WhatTheHack.Needs;

namespace WhatTheHack.Harmony
{
    //Spawn hacked mechnoids in enemy raids
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    public static class IncidentWorker_Raid_TryExecuteWorker
    {
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                            
                //Replace Arrive method by SpawnHackedMechanoids (which also calls Arrive). This avoids the need of ldgarg calls, which seem to change after literally every update of Rimworld.
                if (instruction.operand as MethodInfo == AccessTools.Method(typeof(PawnsArrivalModeWorker), "Arrive"))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(IncidentWorker_Raid_TryExecuteWorker).GetMethod("SpawnHackedMechanoids"));
                    continue;
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        //returns pawns for compatibility reasons. 
        public static List<Pawn> SpawnHackedMechanoids(List<Pawn> pawns, IncidentParms parms)
        {
            //only call Arrive method when sure it's not already called. (can happen due to other mods)
            if (pawns.Count > 0 && !pawns[0].Spawned)
            {
                parms.raidArrivalMode.Worker.Arrive(pawns, parms);
            }

            if (pawns.Count == 0)
            {
                return pawns;
            }
            if (parms.faction == Faction.OfMechanoids)
            {
                return pawns;
            }
            Random rand = new Random(DateTime.Now.Millisecond);
            if (rand.Next(0, 100) > Base.hackedMechChance)
            {
                return pawns;
            }

            int minHackedMechPoints = Math.Min(Base.minHackedMechPoints, Base.maxHackedMechPoints);
            float maxMechPoints = parms.points * ((float)rand.Next(minHackedMechPoints, Base.maxHackedMechPoints)) / 100f; //TODO: no magic numbers
            float cumulativePoints = 0;
            Map map = parms.target as Map;
            List<Pawn> addedPawns = new List<Pawn>();

            while (cumulativePoints < maxMechPoints)
            {
                PawnKindDef pawnKindDef = null;
                IEnumerable<PawnKindDef> selectedPawns = (from a in DefDatabase<PawnKindDef>.AllDefs
                                                  where a.RaceProps.IsMechanoid &&
                                                  cumulativePoints + a.combatPower < maxMechPoints &&
                                                  Utilities.IsAllowedInModOptions(a.race.defName, parms.faction) &&
                                                  (parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn || a.RaceProps.baseBodySize <= 1) //Only allow small mechs to use drop pods
                                                  select a);

                if (selectedPawns != null)
                {
                    selectedPawns.TryRandomElement(out pawnKindDef);
                }
                if (pawnKindDef != null)
                {
                    Pawn mechanoid = PawnGenerator.GeneratePawn(pawnKindDef, parms.faction);
                    if(parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn)
                    {
                        IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8, null);
                        GenSpawn.Spawn(mechanoid, loc, map, parms.spawnRotation);
                    }
                    mechanoid.health.AddHediff(WTH_DefOf.WTH_TargetingHacked);
                    mechanoid.health.AddHediff(WTH_DefOf.WTH_BackupBattery);
                    Need_Power powerNeed = (Need_Power)mechanoid.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power);
                    powerNeed.CurLevel = powerNeed.MaxLevel;
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

            foreach (Pawn pawn in pawns)
            {
                if (pawn.equipment == null)
                {
                    pawn.equipment = new Pawn_EquipmentTracker(pawn);
                }
            }
            return pawns;
        }

        private static void AddModules(Pawn mechanoid)
        {
            List<HediffDef> modules = new List<HediffDef>();
            foreach(HediffDef hediff in Base.allSpawnableModules)
            {
                modules.Add(hediff);
            }

            int i = 0;
            int count = modules.Count;
            while (i < count)
            {
                if (Rand.Chance(1 - modules[i].GetModExtension<DefModextension_Hediff>().spawnChance))
                {//Chance that the mod is NOT used
                    modules.RemoveAt(i);
                    count--;
                }
                else
                {   
                    i++;
                }
            }
            foreach (HediffDef hediff in modules)
            {
                if (hediff == WTH_DefOf.WTH_TurretModule)
                {
                    if (mechanoid.BodySize < 2.0f)
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
            if (mechanoid.TryGetComp<CompRefuelable>() is CompRefuelable comp)
            {
                Traverse.Create(comp).Field("fuel").SetValue(comp.Props.fuelCapacity);
            }
        }

        private static void ConfigureTurretModule(Pawn mechanoid)
        {
            mechanoid.health.AddHediff(WTH_DefOf.WTH_MountedTurret);
            ThingDef turretDef = DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => td.HasComp(typeof(CompMountable))).RandomElement();
            Thing thing = ThingMaker.MakeThing(turretDef, ThingDefOf.Steel);
            CompMountable comp = thing.GetInnerIfMinified().TryGetComp<CompMountable>();
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
            Thing belt = ThingMaker.MakeThing(Base.allBelts.RandomElement());
            mechanoid.apparel.Wear(belt as Apparel);
        }
        //do nothing
    }
}
