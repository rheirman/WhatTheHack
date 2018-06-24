using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Harmony
{
    //Spawn hacked mechnoids in enemy raids
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    static class IncidentWorker_Raid_TryExecuteWorker
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                yield return instruction;
                //Log.Message(instructionsList[i].opcode.ToString());
                //Log.Message(instructionsList[i].operand as String);

                if (instructionsList[i].operand == AccessTools.Method(typeof(IncidentWorker_Raid), "GetLetterLabel")) //Identifier for which IL line to inject to
                {
                    //Start of injection
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 2);//load generated pawns as parameter
                    yield return new CodeInstruction(OpCodes.Ldarg_1);//load incidentparms as parameter
                    yield return new CodeInstruction(OpCodes.Call, typeof(IncidentWorker_Raid_TryExecuteWorker).GetMethod("SpawnHackedMechanoids"));//Injected code
                    //yield return new CodeInstruction(OpCodes.Stloc_2);
                }
            }
        }
        public static void SpawnHackedMechanoids(ref List<Pawn> list, IncidentParms parms)
        {
            //Don't use mechs in sieges or with droppods
            if (list.Count == 0 
                || !(parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn) 
                || (parms.raidStrategy != null && parms.raidStrategy.workerClass == typeof(RaidStrategyWorker_Siege)))
            {
                return;
            }
            if(parms.faction == Faction.OfMechanoids)
            {
                return;
            }
            //Only allow hacked mechs for advanced factions
            if(parms.faction.def.techLevel != TechLevel.Spacer && parms.faction.def.techLevel != TechLevel.Archotech && parms.faction.def.techLevel != TechLevel.Ultra)
            {
                return;
            }

            Random rand = new Random(DateTime.Now.Millisecond);
            if(rand.Next(0, 100) < 40)//TODO: no magic number
            {
                return; 
            }

            float maxMechPoints = parms.points * ((float)rand.Next(0, 50))/100; //TODO: no magic numbers
            float cumulativePoints = 0;
            Map map = parms.target as Map;

            while (cumulativePoints < maxMechPoints)
            {
                PawnKindDef pawnKindDef = null;
                (from a in DefDatabase<PawnKindDef>.AllDefs
                 where a.RaceProps.IsMechanoid &&
                 cumulativePoints + a.combatPower < maxMechPoints
                 //&& IsMountableUtility.isAllowedInModOptions(a.defName)
                 //&& (factionWildAnimalRestrictions.NullOrEmpty() || factionWildAnimalRestrictions.Contains(a.defName))
                 select a).TryRandomElement(out pawnKindDef);

                if (pawnKindDef != null)
                {
                    Pawn mechanoid = PawnGenerator.GeneratePawn(pawnKindDef, parms.faction);
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8, null);
                    GenSpawn.Spawn(mechanoid, loc, map, parms.spawnRotation);
                    mechanoid.health.AddHediff(WTH_DefOf.TargetingHacked);
                    Need_Power powerNeed = (Need_Power) mechanoid.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power);
                    powerNeed.CurLevel = powerNeed.MaxLevel;
                    list.Add(mechanoid);
                    cumulativePoints += pawnKindDef.combatPower;
                }
                else
                {
                    break;
                }
            }

            foreach (Pawn pawn in list)
            {
                if (pawn.equipment == null)
                {
                    pawn.equipment = new Pawn_EquipmentTracker(pawn);
                }
            }
        }
    }
}
