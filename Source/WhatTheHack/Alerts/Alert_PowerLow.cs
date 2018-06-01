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
                       where p.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power) != null 
                       && ((Need_Power) p.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power)).CurCategory >= PowerCategory.VeryLowPower 
                       && !(p.CurrentBed() is Building_MechanoidPlatform) 
                       select p;
            }
        }

        public Alert_PowerLow()
        {
            this.defaultLabel = "WTH_Alert_Power_Low_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override string GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn current in this.LowPowerPawns)
            {
                stringBuilder.AppendLine("    " + current.NameStringShort);
            }
            return string.Format("WTH_Alert_Power_Low_Label".Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(this.LowPowerPawns.FirstOrDefault<Pawn>());
        }
    }
}
