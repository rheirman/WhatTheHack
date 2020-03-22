using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps
{
    public class CompPowerPlant_RogueAI : CompPowerPlant
    {
        public bool overcharging = false;
        protected override float DesiredPowerOutput
        {
            get
            {
                if (overcharging)
                {
                    return 100000;
                }
                else if (parent is Building_RogueAI rogueAI && rogueAI.goingRogue)
                {
                    return base.Props.basePowerConsumption;
                }
                else
                {
                    return -base.Props.basePowerConsumption;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref overcharging, "overcharging");
        }
    }
}
