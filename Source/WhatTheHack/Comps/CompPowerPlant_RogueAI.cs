using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps;

public class CompPowerPlant_RogueAI : CompPowerPlant
{
    public bool overcharging;

    public override float DesiredPowerOutput
    {
        get
        {
            if (overcharging)
            {
                return 100000;
            }

            if (parent is Building_RogueAI { goingRogue: true })
            {
                return Props.basePowerConsumption;
            }

            return -Props.basePowerConsumption;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref overcharging, "overcharging");
    }
}