using RimWorld;
using Verse;

namespace WhatTheHack.Alerts;

internal class Alert_EmergencySignalTransmitted : Alert_Critical
{
    public Alert_EmergencySignalTransmitted()
    {
        defaultLabel = "WTH_Alert_EmergencySignalTransmitted_Label".Translate();
        defaultPriority = AlertPriority.Critical;
    }

    public override TaggedString GetExplanation()
    {
        return string.Format("WTH_Alert_EmergencySignalTransmitted_Description".Translate());
    }

    public override AlertReport GetReport()
    {
        return ShouldAlert();
    }

    private bool ShouldAlert()
    {
        return Base.Instance.EmergencySignalRaidInbound();
    }
}