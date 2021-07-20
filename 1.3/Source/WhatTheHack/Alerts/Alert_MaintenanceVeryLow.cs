using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Alerts
{
    class Alert_MaintenanceVeryLow : Alert_Critical
    {
        private IEnumerable<Pawn> VeryLowMaintenancePawns
        {
            get
            {
                return from p in PawnsFinder.AllMaps_Spawned
                       where p.needs.TryGetNeed<Need_Maintenance>() != null
                       && (p.needs.TryGetNeed<Need_Maintenance>().CurCategory == MaintenanceCategory.VeryLowMaintenance)
                       select p;
            }
        }

        public Alert_MaintenanceVeryLow()
        {
            this.defaultLabel = "WTH_Alert_Maintenance_VeryLow_Label".Translate();            
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            foreach (Pawn current in this.VeryLowMaintenancePawns)
            {
                stringBuilder.AppendLine("    " + current.Name);
            }
            stringBuilder.AppendLine();
            return string.Format("WTH_Alert_Maintenance_VeryLow_Description".Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(this.VeryLowMaintenancePawns.FirstOrDefault<Pawn>());
        }
    }
}
