using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack
{
    public static class Extensions
    {
        public static bool IsHacked(this Pawn pawn)
        {
            if (pawn.health != null && pawn.health.hediffSet.HasHediff(WTH_DefOf.TargetingHacked))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
