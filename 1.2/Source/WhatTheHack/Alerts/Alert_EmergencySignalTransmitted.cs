using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Alerts
{
    class Alert_EmergencySignalTransmitted : Alert_Critical
    {

        public Alert_EmergencySignalTransmitted()
        {
            this.defaultLabel = "WTH_Alert_EmergencySignalTransmitted_Label".Translate();
            this.defaultPriority = AlertPriority.Critical;
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
}
