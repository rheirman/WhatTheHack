using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatTheHack.Comps
{
    public class CompPowerPlant_RogueAI : CompPowerPlant
    {
        public bool overcharging = false;
        protected override float DesiredPowerOutput
        {
            get
            {
                if (!overcharging)
                {
                    return -base.Props.basePowerConsumption;
                }
                else
                {
                    return 100000;
                }
            }
        }
    }
}
