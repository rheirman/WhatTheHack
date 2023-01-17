using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Alerts
{
    class Alert_MaintenanceLow : Alert
    {
        private IEnumerable<Pawn> LowMaintenancePawns
        {
            get
            {
                return from p in PawnsFinder.AllMaps_Spawned
                       where p.needs.TryGetNeed<Need_Maintenance>() != null
                       && (p.needs.TryGetNeed<Need_Maintenance>().CurCategory == MaintenanceCategory.LowMaintenance)
                       select p;
            }
        }

        public Alert_MaintenanceLow()
        {
            this.defaultLabel = "WTH_Alert_Maintenance_Low_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            foreach (Pawn current in this.LowMaintenancePawns)
            {
                stringBuilder.AppendLine("    " + current.Name);
            }
            stringBuilder.AppendLine();
            return string.Format("WTH_Alert_Maintenance_Low_Description".Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(this.LowMaintenancePawns.FirstOrDefault<Pawn>());
        }
    }
}
