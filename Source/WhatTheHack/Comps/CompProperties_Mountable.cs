using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Comps
{
    class CompProperties_Mountable : CompProperties
    {
        public CompProperties_Mountable()
        {
            compClass = typeof(CompMountable);
        }
    }
}
