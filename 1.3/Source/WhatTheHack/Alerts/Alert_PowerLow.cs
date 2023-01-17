using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Needs;

namespace WhatTheHack.Alerts
{
    class Alert_PowerLow : Alert
    {
        private IEnumerable<Pawn> LowPowerPawns
        {
            get
            {
                return from p in PawnsFinder.AllMaps_Spawned
                       where p.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power) != null 
                       && p.Faction == Faction.OfPlayer 
                       && ((Need_Power) p.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power)).CurCategory >= PowerCategory.VeryLowPower 
                       && !(p.CurrentBed() is Building_BaseMechanoidPlatform) 
                       select p;
            }
        }

        public Alert_PowerLow()
        {
            this.defaultLabel = "WTH_Alert_Power_Low_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            foreach (Pawn current in this.LowPowerPawns)
            {
                stringBuilder.AppendLine("    " + current.Name);
            }
            stringBuilder.AppendLine();
            return string.Format("WTH_Alert_Power_Low_Description".Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(this.LowPowerPawns.FirstOrDefault<Pawn>());
        }
    }
}
