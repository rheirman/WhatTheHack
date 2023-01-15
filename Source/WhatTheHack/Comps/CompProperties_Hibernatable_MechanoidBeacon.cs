using RimWorld;

namespace WhatTheHack.Comps;

internal class CompProperties_Hibernatable_MechanoidBeacon : CompProperties_Hibernatable
{
    public int coolDownDaysAfterSuccess = 15;

    public CompProperties_Hibernatable_MechanoidBeacon()
    {
        compClass = typeof(CompHibernatable_MechanoidBeacon);
    }
}