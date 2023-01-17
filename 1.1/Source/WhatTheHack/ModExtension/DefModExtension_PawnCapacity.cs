using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack
{
    //In vanilla, centipedes always die due to the damagethreshold before being downed.This patch makes sure they are downed quicker
    //This mod extension is obsolete. I'm keeping it in to prevent errors with mods that still refer to it. 
    class DefModExtension_PawnCapacity : DefModExtension
    {
        public float minForCapableMoving;
    }
}
