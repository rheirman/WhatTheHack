using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace WhatTheHack.WorldIncidents;

internal class GenStep_RoamingMechanoids : GenStep
{
    public FloatRange defaultPointsRange = new FloatRange(340f, 1000f);

    public override int SeedPart => 341176078;

    public override void Generate(Map map, GenStepParams parms)
    {
        if (!SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out var around, out var near, map))
        {
            return;
        }

        var list = new List<Pawn>();
        foreach (var current in GeneratePawns(parms, map))
        {
            if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(around, near, map, out var loc))
            {
                Find.WorldPawns.PassToWorld(current);
                break;
            }

            GenSpawn.Spawn(current, loc, map);
            list.Add(current);
        }

        if (!list.Any())
        {
            return;
        }

        LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony(Faction.OfMechanoids, Rand.Bool), map,
            list);
        for (var i = 0; i < list.Count; i++)
        {
            list[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }

    private IEnumerable<Pawn> GeneratePawns(GenStepParams parms, Map map)
    {
        var points = parms.sitePart == null ? defaultPointsRange.RandomInRange : parms.sitePart.parms.threatPoints;
        var pawnGroupMakerParms = new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = map.Tile,
            faction = Faction.OfMechanoids,
            points = Mathf.Max(points, 200f)
        };

        if (parms.sitePart != null)
        {
            pawnGroupMakerParms.seed = SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
        }

        return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
    }
}