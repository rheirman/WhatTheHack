using RimWorld.BaseGen;

namespace WhatTheHack.WorldIncidents;

public class SymbolResolver_MechanoidTemple : SymbolResolver
{
    public override void Resolve(ResolveParams rp)
    {
        BaseGen.symbolStack.Push("ensureCanHoldRoof", rp);
        var resolveParams = rp;
        resolveParams.rect = rp.rect.ContractedBy(1);
        BaseGen.symbolStack.Push("interior_mechanoidTemple", resolveParams);
        var resolveParams2 = rp;
        resolveParams2.wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, true);
        var clearEdificeOnly = rp.clearEdificeOnly;
        resolveParams2.clearEdificeOnly = !clearEdificeOnly.HasValue || clearEdificeOnly.Value;
        BaseGen.symbolStack.Push("emptyRoom", resolveParams2);
    }
}