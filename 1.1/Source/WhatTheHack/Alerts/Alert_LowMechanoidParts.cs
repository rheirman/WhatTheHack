using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Alerts
{
    public class Alert_LowMechanoidParts : Alert
    {
        private const float MechanoidPartsPerMechThreshold = 8f;

        public Alert_LowMechanoidParts()
        {
            this.defaultLabel = "WTH_Alert_LowMechanoidParts".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            Map map = this.MapWithLowMechanoidParts();
            if (map == null)
            {
                return string.Empty;
            }
            int num = this.MechanoidPartsCount(map);
            if (num == 0)
            {
                return "WTH_Alert_NoMechanoidPartsDesc".Translate() + "\n\n" + "WTH_Alert_LowMechanoidPartsSolution".Translate();
            }
            return "WTH_Alert_LowMechanoidPartsDesc".Translate(num) + "\n\n" + "WTH_Alert_LowMechanoidPartsSolution".Translate();
        }

        public override AlertReport GetReport()
        {
            if (!Base.maintenanceDecayEnabled)
            {
                return false;
            }
            return this.MapWithLowMechanoidParts() != null;
        }

        private Map MapWithLowMechanoidParts()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map.IsPlayerHome)
                {
                    IEnumerable<Pawn> mechs = map.mapPawns.AllPawns.Where((Pawn p) => p.Faction == Faction.OfPlayer && p.IsHacked());
                    if((float)this.MechanoidPartsCount(map) < MechanoidPartsPerMechThreshold * mechs.Count())
                    {
                        return map;
                    }

                        
                }
            }
            return null;
        }

        private int MechanoidPartsCount(Map map)
        {
            return map.resourceCounter.GetCount(WTH_DefOf.WTH_MechanoidParts);
        }
    }
    
}
