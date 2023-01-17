using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack
{
    public class DefModExtension_Ability : DefModExtension
    {
        public int warmupTicks = 0;
        public float powerDrain = 0f;
        public float fuelDrain = 0f;
        public float failChance = 0f;
        public HediffDef hediffSelf;
        public HediffDef hediffTarget;
    }
}
