using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Alerts
{
    class Alert_EmergencySignalCooldown : Alert
    {
        public Alert_EmergencySignalCooldown()
        {
            this.defaultLabel = "WTH_Alert_EmergencySignalCooldown_Label".Translate();
            this.defaultPriority = AlertPriority.Medium;
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
}
