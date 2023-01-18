using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Alerts;

internal class Alert_MaintenanceLow : Alert
{
    public Alert_MaintenanceLow()
    {
        defaultLabel = "WTH_Alert_Maintenance_Low_Label".Translate();
        defaultPriority = AlertPriority.High;
    }

    private IEnumerable<Pawn> LowMaintenancePawns =>
        from p in PawnsFinder.AllMaps_Spawned
        where p.needs.TryGetNeed<Need_Maintenance>() != null
              && p.needs.TryGetNeed<Need_Maintenance>().CurCategory == MaintenanceCategory.LowMaintenance
        select p;

    public override TaggedString GetExplanation()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();
        foreach (var current in LowMaintenancePawns)
        {
            stringBuilder.AppendLine($"    {current.Name}");
        }

        stringBuilder.AppendLine();
        return string.Format("WTH_Alert_Maintenance_Low_Description".Translate(), stringBuilder);
    }

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritIs(LowMaintenancePawns.FirstOrDefault());
    }
}