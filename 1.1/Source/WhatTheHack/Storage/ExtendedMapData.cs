using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Storage
{
    public class ExtendedMapData : IExposable
    {
        public Building_RogueAI rogueAI = null;
        public void ExposeData()
        {
            Scribe_References.Look(ref rogueAI, "rogueAI");
        }
    }
}