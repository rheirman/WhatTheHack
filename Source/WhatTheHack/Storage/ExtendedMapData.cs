using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Storage;

public class ExtendedMapData : IExposable
{
    public Building_RogueAI rogueAI;

    public void ExposeData()
    {
        Scribe_References.Look(ref rogueAI, "rogueAI");
    }
}