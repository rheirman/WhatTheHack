using RimWorld.BaseGen;
using Verse;

namespace WhatTheHack.WorldIncidents;

public class SymbolResolver_Interior_MechanoidTemple : SymbolResolver
{
    private static readonly IntRange MechanoidCountRange = new IntRange(1, 5);

    public override void Resolve(ResolveParams rp)
    {
        var list = WTH_DefOf.WTH_MapGen_MechanoidTempleContents.root.Generate();
        for (var i = 0; i < list.Count; i++)
        {
            var resolveParams = rp;
            resolveParams.singleThingToSpawn = list[i];
            BaseGen.symbolStack.Push("thing", resolveParams);
        }

        var resolveParams2 = rp;
        var mechanoidsCount = rp.mechanoidsCount;
        resolveParams2.mechanoidsCount =
            !mechanoidsCount.HasValue ? MechanoidCountRange.RandomInRange : mechanoidsCount.Value;
        BaseGen.symbolStack.Push("randomMechanoidGroup", resolveParams2);

        var ancientTempleEntranceHeight = rp.ancientCryptosleepCasketGroupID;
        var num = !ancientTempleEntranceHeight.HasValue ? 0 : ancientTempleEntranceHeight.Value;
        var resolveParams4 = rp;
        resolveParams4.rect.minZ += num;
        BaseGen.symbolStack.Push("ancientShrinesGroup", resolveParams4);
    }
}