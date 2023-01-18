using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Needs;

namespace WhatTheHack.Alerts;

internal class Alert_PowerLow : Alert
{
    public Alert_PowerLow()
    {
        defaultLabel = "WTH_Alert_Power_Low_Label".Translate();
        defaultPriority = AlertPriority.High;
    }

    private IEnumerable<Pawn> LowPowerPawns =>
        from p in PawnsFinder.AllMaps_Spawned
        where p.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power) != null
              && p.Faction == Faction.OfPlayer
              && ((Need_Power)p.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power)).CurCategory >=
              PowerCategory.VeryLowPower
              && p.CurrentBed() is not Building_BaseMechanoidPlatform
        select p;

    public override TaggedString GetExplanation()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();
        foreach (var current in LowPowerPawns)
        {
            stringBuilder.AppendLine($"    {current.Name}");
        }

        stringBuilder.AppendLine();
        return string.Format("WTH_Alert_Power_Low_Description".Translate(), stringBuilder);
    }

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritIs(LowPowerPawns.FirstOrDefault());
    }
}