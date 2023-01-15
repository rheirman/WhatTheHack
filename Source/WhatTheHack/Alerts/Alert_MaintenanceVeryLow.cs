using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Alerts;

internal class Alert_MaintenanceVeryLow : Alert_Critical
{
    public Alert_MaintenanceVeryLow()
    {
        defaultLabel = "WTH_Alert_Maintenance_VeryLow_Label".Translate();
    }

    private IEnumerable<Pawn> VeryLowMaintenancePawns =>
        from p in PawnsFinder.AllMaps_Spawned
        where p.needs.TryGetNeed<Need_Maintenance>() != null
              && p.needs.TryGetNeed<Need_Maintenance>().CurCategory == MaintenanceCategory.VeryLowMaintenance
        select p;

    public override TaggedString GetExplanation()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();
        foreach (var current in VeryLowMaintenancePawns)
        {
            stringBuilder.AppendLine($"    {current.Name}");
        }

        stringBuilder.AppendLine();
        return string.Format("WTH_Alert_Maintenance_VeryLow_Description".Translate(), stringBuilder);
    }

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritIs(VeryLowMaintenancePawns.FirstOrDefault());
    }
}