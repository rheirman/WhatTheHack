using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatTheHack.Comps
{
    class CompProperties_Hibernatable_MechanoidBeacon : CompProperties_Hibernatable
    {
        public int coolDownDaysAfterSuccess = 15;

        public CompProperties_Hibernatable_MechanoidBeacon()
        {
            this.compClass = typeof(CompHibernatable_MechanoidBeacon);
        }
    }
}
