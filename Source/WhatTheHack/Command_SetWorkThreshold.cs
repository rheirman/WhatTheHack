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
    public class Command_SetWorkThreshold : Command
    {
        public Need_Power powerNeed;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            Func<int, string> textGetter = ((int x) => "WTH_SetCanStartWorkThreshold".Translate(x));
            Dialog_Slider window = new Dialog_Slider(textGetter, 0, 100, delegate (int value)
            {
                powerNeed.canStartWorkThreshold = value/100f;
                if(powerNeed.canStartWorkThreshold < powerNeed.PercentageThreshLowPower + 0.05f)
                {
                    Messages.Message("WTH_Warning_WorkThreshold".Translate(), MessageTypeDefOf.CautionInput);
                }
            }, (int) (powerNeed.canStartWorkThreshold * 100f));
            Find.WindowStack.Add(window);
        }
    }
}
