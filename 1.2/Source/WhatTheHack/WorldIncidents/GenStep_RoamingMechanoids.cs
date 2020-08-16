using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace WhatTheHack.WorldIncidents
{
    class GenStep_RoamingMechanoids : GenStep
    {
        public FloatRange defaultPointsRange = new FloatRange(340f, 1000f);

        public override int SeedPart
        {
            get
            {
                return 341176078;
            }
        }
        public override void Generate(Map map, GenStepParams parms)
        {
            CellRect around;
            IntVec3 near;
            if (!SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out around, out near, map))
            {
                return;
            }
            List<Pawn> list = new List<Pawn>();
            foreach (Pawn current in this.GeneratePawns(parms, map))
            {
                IntVec3 loc;
                if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(around, near, map, out loc))
                {
                    Find.WorldPawns.PassToWorld(current, PawnDiscardDecideMode.Decide);
                    break;
                }
                GenSpawn.Spawn(current, loc, map, WipeMode.Vanish);
                list.Add(current);
            }
            if (!list.Any<Pawn>())
            {
                return;
            }
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony(Faction.OfMechanoids, Rand.Bool), map, list);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
        }

        private IEnumerable<Pawn> GeneratePawns(GenStepParams parms, Map map)
        {
            float points = (parms.sitePart == null) ? this.defaultPointsRange.RandomInRange : parms.sitePart.parms.threatPoints;
            PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
            pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
            pawnGroupMakerParms.tile = map.Tile;
            pawnGroupMakerParms.faction = Faction.OfMechanoids;
            pawnGroupMakerParms.points = Mathf.Max(points, 200f);
            
            if (parms.sitePart != null)
            {
                pawnGroupMakerParms.seed = new int?(SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms));
            }
            return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true);
        }
    }
}
