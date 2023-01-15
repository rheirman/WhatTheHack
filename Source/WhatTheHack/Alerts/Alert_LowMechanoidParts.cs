using System.Linq;
using RimWorld;
using Verse;

namespace WhatTheHack.Alerts;

public class Alert_LowMechanoidParts : Alert
{
    private const float MechanoidPartsPerMechThreshold = 8f;

    public Alert_LowMechanoidParts()
    {
        defaultLabel = "WTH_Alert_LowMechanoidParts".Translate();
        defaultPriority = AlertPriority.High;
    }

    public override TaggedString GetExplanation()
    {
        var map = MapWithLowMechanoidParts();
        if (map == null)
        {
            return string.Empty;
        }

        var num = MechanoidPartsCount(map);
        if (num == 0)
        {
            return "WTH_Alert_NoMechanoidPartsDesc".Translate() + "\n\n" +
                   "WTH_Alert_LowMechanoidPartsSolution".Translate();
        }

        return "WTH_Alert_LowMechanoidPartsDesc".Translate(num) + "\n\n" +
               "WTH_Alert_LowMechanoidPartsSolution".Translate();
    }

    public override AlertReport GetReport()
    {
        if (!Base.maintenanceDecayEnabled)
        {
            return false;
        }

        return MapWithLowMechanoidParts() != null;
    }

    private Map MapWithLowMechanoidParts()
    {
        var maps = Find.Maps;
        foreach (var map in maps)
        {
            if (!map.IsPlayerHome)
            {
                continue;
            }

            var mechs = map.mapPawns.AllPawns.Where(p => p.Faction == Faction.OfPlayer && p.IsHacked());
            if (MechanoidPartsCount(map) < MechanoidPartsPerMechThreshold * mechs.Count())
            {
                return map;
            }
        }

        return null;
    }

    private int MechanoidPartsCount(Map map)
    {
        return map.resourceCounter.GetCount(WTH_DefOf.WTH_MechanoidParts);
    }
}