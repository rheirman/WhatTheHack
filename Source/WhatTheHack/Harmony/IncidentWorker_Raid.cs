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
                yield return instruction;

                if (instructionsList[i].operand == AccessTools.Method(typeof(IncidentWorker_Raid), "GetLetterLabel")) //Identifier for which IL line to inject to
                {
                    //Start of injection
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 3);//load generated pawns as parameter
                    yield return new CodeInstruction(OpCodes.Ldarg_1);//load incidentparms as parameter
                    yield return new CodeInstruction(OpCodes.Call, typeof(IncidentWorker_Raid_TryExecuteWorker).GetMethod("SpawnHackedMechanoids"));//Injected code
                    //yield return new CodeInstruction(OpCodes.Stloc_2);
                }
            }
        }
        public static void SpawnHackedMechanoids(ref List<Pawn> list, IncidentParms parms)
        {
            if (list.Count == 0 || !(parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeWalkIn))
            {
                return;
            }
            if(parms.faction == Faction.OfMechanoids)
            {
                return;
            }
            Random rand = new Random(DateTime.Now.Millisecond);
            if(rand.Next(0, 100) > Base.hackedMechChance)
            {
                return;
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
        }
    }
}
