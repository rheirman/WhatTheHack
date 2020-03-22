using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack
{
    //In vanilla, centipedes always die due to the damagethreshold before being downed.This patch makes sure they are downed quicker
    class DefModExtension_PawnCapacity : DefModExtension
    {
        public float minForCapableMoving;
    }
}
