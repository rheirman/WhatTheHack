using RimWorld;
using UnityEngine;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack;

[StaticConstructorOnStartup]
public class Command_SetWorkThreshold : Command
{
    public Need_Power powerNeed;

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);

        string TextGetter(int x)
        {
            return "WTH_SetCanStartWorkThreshold".Translate(x);
        }

        var window = new Dialog_Slider(TextGetter, 0, 100, delegate(int value)
        {
            powerNeed.canStartWorkThreshold = value / 100f;
            if (powerNeed.canStartWorkThreshold < powerNeed.PercentageThreshLowPower + 0.05f)
            {
                Messages.Message("WTH_Warning_WorkThreshold".Translate(), MessageTypeDefOf.CautionInput);
            }
        }, (int)(powerNeed.canStartWorkThreshold * 100f));
        Find.WindowStack.Add(window);
    }
}