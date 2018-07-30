using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack
{
    public class DefModExtension_Recipe : DefModExtension
    {
        public float surgerySuccessChanceFactor;
        public float deathOnFailedSurgeryChance;
        public bool requireBed;
    }
}
