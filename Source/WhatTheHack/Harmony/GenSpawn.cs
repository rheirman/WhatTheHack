using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(GenSpawn), "Spawn")]
    [HarmonyPatch(new Type[]{typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode),typeof(bool)})]
    class GenSpawn_Spawn
    {
        //Only initialize the refeulcomp of mechanoids that have a repairmodule. 
        static void Postfix(ref Thing newThing)
        {
            if(!(newThing is ThingWithComps thingWithComps)){
                return;
            }
            if (thingWithComps is Pawn && ((Pawn)thingWithComps).RaceProps.IsMechanoid && thingWithComps.def.comps.Any<CompProperties>())
            {
                Base.RemoveComps(ref thingWithComps);
            }
        }
    }

}