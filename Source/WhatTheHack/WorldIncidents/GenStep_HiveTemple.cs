using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace WhatTheHack.WorldIncidents;

public class GenStep_HiveTemple : GenStep
{
    private static readonly List<CellRect> possibleRects = new List<CellRect>();
    private readonly int size = new IntRange(16, 28).RandomInRange;

    public override int SeedPart => 735013949;

    public override void Generate(Map map, GenStepParams genStepParams)
    {
        if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect centralPoint))
        {
            centralPoint = CellRect.SingleCell(map.Center);
        }

        var faction = Faction.OfMechanoids;
        var resolveParams = new ResolveParams
        {
            rect = getRect(centralPoint, map),
            faction = faction
        };

        var thingMakerParams = new ThingSetMakerParams
        {
            totalMarketValueRange = new FloatRange(500, 1000),
            filter = new ThingFilter()
        };
        thingMakerParams.filter.SetDisallowAll();
        thingMakerParams.filter.SetAllow(WTH_DefOf.WTH_MechanoidData, true);

        resolveParams.thingSetMakerParams = thingMakerParams;
        var sizeFactor = size / 20f;

        resolveParams.mechanoidsCount = Math.Max(5,
            Mathf.RoundToInt(genStepParams.sitePart.parms.threatPoints * sizeFactor / 100f));

        BaseGen.globalSettings.map = map;
        BaseGen.globalSettings.minBuildings = 1;
        BaseGen.globalSettings.minBarracks = 1;
        BaseGen.symbolStack.Push("mechanoidTemple", resolveParams);
        BaseGen.Generate();
    }

    private CellRect getRect(CellRect centralPoint, Map map)
    {
        possibleRects.Add(new CellRect(centralPoint.minX - 1 - size, centralPoint.CenterCell.z - 8, size, size));
        possibleRects.Add(new CellRect(centralPoint.maxX + 1, centralPoint.CenterCell.z - 8, size, size));
        possibleRects.Add(new CellRect(centralPoint.CenterCell.x - 8, centralPoint.minZ - 1 - size, size, size));
        possibleRects.Add(new CellRect(centralPoint.CenterCell.x - 8, centralPoint.maxZ + 1, size, size));
        var mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
        possibleRects.RemoveAll(x => !x.FullyContainedWithin(mapRect));
        if (possibleRects.Any())
        {
            return possibleRects.RandomElement();
        }

        return centralPoint;
    }
}