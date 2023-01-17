using RimWorld;
using RimWorld.BaseGen;
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
    public class GenStep_HiveTemple : GenStep
    {
        private int size = new IntRange(16, 28).RandomInRange;

        private static List<CellRect> possibleRects = new List<CellRect>();

        public override int SeedPart => 735013949;

        public override void Generate(Map map, GenStepParams genStepParams)
        {
            if (!MapGenerator.TryGetVar<CellRect>(name: "RectOfInterest", var: out CellRect centralPoint))
            {
                centralPoint = CellRect.SingleCell(c: map.Center);
            }

            Faction faction = Faction.OfMechanoids;
            ResolveParams resolveParams = new ResolveParams();
            resolveParams.rect = this.getRect(centralPoint: centralPoint, map: map);
            resolveParams.faction = faction;

            ThingSetMakerParams thingMakerParams = new ThingSetMakerParams();
            thingMakerParams.totalMarketValueRange = new FloatRange(500, 1000);
            thingMakerParams.filter = new ThingFilter();
            thingMakerParams.filter.SetDisallowAll();
            thingMakerParams.filter.SetAllow(WTH_DefOf.WTH_MechanoidData, true);

            resolveParams.thingSetMakerParams = thingMakerParams;
            float sizeFactor = size / 20f;

            resolveParams.mechanoidsCount =  Math.Max(5, Mathf.RoundToInt((genStepParams.sitePart.parms.threatPoints * sizeFactor) / 100f));

            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push(symbol: "mechanoidTemple", resolveParams: resolveParams);
            BaseGen.Generate();
        }

        private CellRect getRect(CellRect centralPoint, Map map)
        {
            possibleRects.Add(item: new CellRect(minX: centralPoint.minX - 1 - size, minZ: centralPoint.CenterCell.z - 8, width: size, height: size));
            possibleRects.Add(item: new CellRect(minX: centralPoint.maxX + 1, minZ: centralPoint.CenterCell.z - 8, width: size, height: size));
            possibleRects.Add(item: new CellRect(minX: centralPoint.CenterCell.x - 8, minZ: centralPoint.minZ - 1 - size, width: size, height: size));
            possibleRects.Add(item: new CellRect(minX: centralPoint.CenterCell.x - 8, minZ: centralPoint.maxZ + 1, width: size, height: size));
            CellRect mapRect = new CellRect(minX: 0, minZ: 0, width: map.Size.x, height: map.Size.z);
            possibleRects.RemoveAll(match: (CellRect x) => !x.FullyContainedWithin(within: mapRect));
            if (possibleRects.Any<CellRect>())
            {
                return possibleRects.RandomElement<CellRect>();
            }
            return centralPoint;
        }
    }
}
