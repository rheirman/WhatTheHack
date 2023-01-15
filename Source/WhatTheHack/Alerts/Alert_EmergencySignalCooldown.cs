using RimWorld;
using Verse;

namespace WhatTheHack.Alerts;

internal class Alert_EmergencySignalCooldown : Alert
{
    public Alert_EmergencySignalCooldown()
    {
        defaultLabel = "WTH_Alert_EmergencySignalCooldown_Label".Translate();
        defaultPriority = AlertPriority.Medium;
    }

    public override TaggedString GetExplanation()
    {
        return string.Format("WTH_Alert_EmergencySignalCooldown_Description".Translate());
    }

    public override AlertReport GetReport()
    {
        return ShouldAlert();
    }

    private bool ShouldAlert()
    {
        return Base.Instance.EmergencySignalRaidCoolingDown();
    }
}