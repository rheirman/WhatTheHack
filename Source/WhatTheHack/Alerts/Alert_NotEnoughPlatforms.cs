using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Alerts;

internal class Alert_NotEnoughPlatforms : Alert
{
    public Alert_NotEnoughPlatforms()
    {
        defaultLabel = "WTH_Alert_NotEnoughPlatforms_Label".Translate();
        defaultExplanation = "WTH_Alert_NotEnoughPlatforms_Description".Translate();
    }

    public override AlertReport GetReport()
    {
        return ShouldAlert();
    }

    private bool ShouldAlert()
    {
        var maps = Find.Maps;
        foreach (var map in maps)
        {
            if (NeedPlatforms(map))
            {
                return true;
            }
        }

        return false;
    }

    private bool NeedPlatforms(Map map)
    {
        var hackedMechanoids = map.mapPawns.AllPawnsSpawned.FindAll(p => p.IsHacked() && p.Faction == Faction.OfPlayer);
        if (!map.IsPlayerHome)
        {
            return false;
        }

        if (hackedMechanoids.NullOrEmpty() || !map.listerBuildings.allBuildingsColonist.Any())
        {
            return false;
        }

        return map.listerBuildings.allBuildingsColonist.FindAll(b => b is Building_BaseMechanoidPlatform).Count <
               hackedMechanoids.Count;
    }
}