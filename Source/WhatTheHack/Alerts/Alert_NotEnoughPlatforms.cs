using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Alerts
{
    class Alert_NotEnoughPlatforms : Alert
    {
        public Alert_NotEnoughPlatforms()
        {
            this.defaultLabel = "WTH_Alert_NotEnoughPlatforms_Label".Translate();
            this.defaultExplanation = "WTH_Alert_NotEnoughPlatforms_Description".Translate();
        }
        public override AlertReport GetReport()
        {
            return this.ShouldAlert();
        }
        private bool ShouldAlert()
        {
            List<Map> maps = Find.Maps;
            foreach (Map map in maps)
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
            List<Pawn> hackedMechanoids = map.mapPawns.AllPawnsSpawned.FindAll((Pawn p) => p.IsHacked() && p.Faction == Faction.OfPlayer);
            if (!map.IsPlayerHome)
            {
                return false;
            }
            if (hackedMechanoids.NullOrEmpty() || !map.listerBuildings.allBuildingsColonist.Any<Building>())
            {
                return false;
            }
            return map.listerBuildings.allBuildingsColonist.FindAll((Building b) => b is Building_BaseMechanoidPlatform).Count < hackedMechanoids.Count;
        }
    }
}
