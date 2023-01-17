using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack
{
    [StaticConstructorOnStartup]
    public class Command_SetMaintenanceThreshold : Command
    {
        public Need_Maintenance maintenanceNeed;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            Func<int, string> textGetter = ((int x) => "WTH_SetMaintenanceThreshold".Translate(x));
            Dialog_Slider window = new Dialog_Slider(textGetter, 0, 50, delegate (int value)
            {
                maintenanceNeed.maintenanceThreshold = value/100f;
            }, (int) (maintenanceNeed.maintenanceThreshold * 100f));
            Find.WindowStack.Add(window);
        }
    }
}
