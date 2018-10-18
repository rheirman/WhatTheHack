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
    public static class IncidentWorker_Raid_TryExecuteWorker
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];

                if (instruction.operand == AccessTools.Method(typeof(PawnsArrivalModeWorker), "Arrive"))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(IncidentWorker_Raid_TryExecuteWorker).GetMethod("DoNothing"));//don't execute this, execute it in MountAnimals
                    continue;
                }
                if (instruction.operand == AccessTools.Method(typeof(PawnGroupMakerUtility), "GeneratePawns")) //Identifier for which IL line to inject to
                {
                    //Start of injection
                    yield return new CodeInstruction(OpCodes.Ldarg_1);//load incidentparms as parameter
                    yield return new CodeInstruction(OpCodes.Call, typeof(IncidentWorker_Raid_TryExecuteWorker).GetMethod("SpawnHackedMechanoids"));//replace GeneratePawns by custom code
                }
                /*
                else if (instructionsList[i].operand == AccessTools.Method(typeof(RaidStrategyWorker), "MakeLords")) //Identifier for which IL line to inject to
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(IncidentWorker_Raid_TryExecuteWorker).GetMethod("RemoveAnimals"));//Injected code
                } 
                */
                else
                {
                    yield return instruction;
                }
            }
        }
        public static void DoNothing(List<Pawn> pawns, IncidentParms parms)
        {
            //do nothing
        }
        public static IEnumerable<Pawn> SpawnHackedMechanoids(PawnGroupMakerParms groupParms, bool warnOnZeroResults, IncidentParms parms)
        {
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(groupParms, true).ToList();
            if (list.Count == 0)
            {
                return list;
            }
            parms.raidArrivalMode.Worker.Arrive(list, parms);
            if (list.Count == 0 || !(parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn))
            {
                return list;
            }
            if(parms.faction == Faction.OfMechanoids)
            {
                return list;
            }
            Random rand = new Random(DateTime.Now.Millisecond);
            if(rand.Next(0, 100) > Base.hackedMechChance)
            {
                return list;
            }

            int minHackedMechPoints = Math.Min(Base.minHackedMechPoints, Base.maxHackedMechPoints);
            float maxMechPoints = parms.points * ((float)rand.Next(minHackedMechPoints, Base.maxHackedMechPoints))/100; //TODO: no magic numbers
            float cumulativePoints = 0;
            Map map = parms.target as Map;

            while (cumulativePoints < maxMechPoints)
            {
                PawnKindDef pawnKindDef = null;
                (from a in DefDatabase<PawnKindDef>.AllDefs
                 where a.RaceProps.IsMechanoid &&
                 cumulativePoints + a.combatPower < maxMechPoints && 
                 Utilities.IsAllowedInModOptions(a.race.defName, parms.faction)                
                 //&& IsMountableUtility.isAllowedInModOptions(a.defName)
                 select a).TryRandomElement(out pawnKindDef);

                if (pawnKindDef != null)
                {
                    Pawn mechanoid = PawnGenerator.GeneratePawn(pawnKindDef, parms.faction);
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8, null);
                    GenSpawn.Spawn(mechanoid, loc, map, parms.spawnRotation);
                    mechanoid.health.AddHediff(WTH_DefOf.WTH_TargetingHacked);
                    mechanoid.health.AddHediff(WTH_DefOf.WTH_BackupBattery);
                    Need_Power powerNeed = (Need_Power) mechanoid.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power);
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
            return list;
        }
    }
}
