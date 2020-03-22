using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatTheHack.WorldIncidents
{
    public class SymbolResolver_MechanoidTemple : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            BaseGen.symbolStack.Push("ensureCanHoldRoof", rp);
            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect.ContractedBy(1);
            BaseGen.symbolStack.Push("interior_mechanoidTemple", resolveParams);
            ResolveParams resolveParams2 = rp;
            resolveParams2.wallStuff = (rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, true));
            bool? clearEdificeOnly = rp.clearEdificeOnly;
            resolveParams2.clearEdificeOnly = new bool?(!clearEdificeOnly.HasValue || clearEdificeOnly.Value);
            BaseGen.symbolStack.Push("emptyRoom", resolveParams2);
        }
    }
}
