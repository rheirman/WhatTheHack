using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.WorldIncidents
{
    public class SymbolResolver_Interior_MechanoidTemple : SymbolResolver
    {
        private static readonly IntRange MechanoidCountRange = new IntRange(1, 5);

        public override void Resolve(ResolveParams rp)
        {
            List<Thing> list = WTH_DefOf.WTH_MapGen_MechanoidTempleContents.root.Generate();
            for (int i = 0; i < list.Count; i++)
            {
                ResolveParams resolveParams = rp;
                resolveParams.singleThingToSpawn = list[i];
                BaseGen.symbolStack.Push("thing", resolveParams);
            }

            ResolveParams resolveParams2 = rp;
            int? mechanoidsCount = rp.mechanoidsCount;
            resolveParams2.mechanoidsCount = new int?((!mechanoidsCount.HasValue) ? SymbolResolver_Interior_MechanoidTemple.MechanoidCountRange.RandomInRange : mechanoidsCount.Value);
            BaseGen.symbolStack.Push("randomMechanoidGroup", resolveParams2);

            int? ancientTempleEntranceHeight = rp.ancientCryptosleepCasketGroupID;
            int num = (!ancientTempleEntranceHeight.HasValue) ? 0 : ancientTempleEntranceHeight.Value;
            ResolveParams resolveParams4 = rp;
            resolveParams4.rect.minZ = resolveParams4.rect.minZ + num;
            BaseGen.symbolStack.Push("ancientShrinesGroup", resolveParams4);
        }
    }
}
