using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WhatTheHack.Comps;

public class CompMountable : ThingComp
{
    public float drawOffset;
    public Pawn mountedTo;

    public Pawn MountedTo => mountedTo;

    public bool Active => mountedTo != null && !parent.Destroyed;

    public void MountToPawn(Pawn pawn)
    {
        mountedTo = pawn;
        SetPowerComp();
        Configure();
        SetDrawOffset();
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        Uninstall();
        base.PostDestroy(mode, previousMap);
    }

    public void Uninstall()
    {
        if (mountedTo == null || mountedTo.health == null)
        {
            return;
        }

        if (mountedTo.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret))
        {
            mountedTo.health.RemoveHediff(
                mountedTo.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_MountedTurret));
        }

        Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mountedTo).turretMount = null;
        mountedTo = null;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!Active)
        {
            return;
        }

        Configure();
        LetMountedToWaitIfReserved();

        if (mountedTo.health != null && !mountedTo.health.hediffSet.HasHediff(WTH_DefOf.WTH_TurretModule))
        {
            Uninstall();
        }
    }

    private void LetMountedToWaitIfReserved()
    {
        var turret = (Building_TurretGun)parent;
        if (turret.Map == null || !turret.Map.reservationManager.IsReservedByAnyoneOf(turret, Faction.OfPlayer))
        {
            return;
        }

        if (mountedTo.CurJobDef == JobDefOf.Wait_Combat || mountedTo.CurJobDef == WTH_DefOf.WTH_Mechanoid_Rest)
        {
            return;
        }

        mountedTo.jobs.StartJob(new Job(JobDefOf.Wait_Combat) { expiryInterval = 10000 },
            JobCondition.InterruptForced);
        mountedTo.jobs.curDriver.AddFailCondition(() =>
            !turret.Map.reservationManager.IsReservedByAnyoneOf(turret, Faction.OfPlayer));
    }

    private void Configure()
    {
        if (mountedTo.Map == null && parent.Spawned || mountedTo.Downed ||
            mountedTo.Spawned && OutOfBounds(mountedTo, parent))
        {
            ToInventory();
        }
        else if (mountedTo.Map != parent.Map && mountedTo.Spawned &&
                 !OutOfBounds(mountedTo, parent.GetInnerIfMinified()))
        {
            OutOfInventory();
        }

        SetPowerComp();
        LinkToMountedTo();
        if (parent.Spawned)
        {
            parent.Position = mountedTo.Position;
        }

        if (parent.Faction != mountedTo.Faction)
        {
            parent.SetFaction(mountedTo.Faction);
        }
    }

    private void LinkToMountedTo()
    {
        if (Base.Instance.GetExtendedDataStorage() is { } store && mountedTo != null)
        {
            store.GetExtendedDataFor(mountedTo).turretMount = this;
        }
    }

    private bool OutOfBounds(Pawn mountedTo, Thing parent)
    {
        var cellRect = GenAdj.OccupiedRect(mountedTo.Position, mountedTo.Rotation, parent.def.Size + new IntVec2(1, 1));
        foreach (var intVec3 in cellRect)
        {
            if (!intVec3.InBounds(mountedTo.Map))
            {
                return true;
            }
        }

        return false;
    }


    public void OutOfInventory()
    {
        parent = (ThingWithComps)parent.SplitOff(1);
        GenSpawn.Spawn(parent, mountedTo.Position, mountedTo.Map, Rot4.North);
    }

    public void ToInventory()
    {
        parent = (ThingWithComps)parent.SplitOff(1);
        mountedTo.inventory.innerContainer.TryAdd(parent, 1);
    }

    private void SetPowerComp()
    {
        var compPower = parent.GetComp<CompPowerTrader>();
        if (compPower == null || !parent.Spawned)
        {
            return;
        }

        var compFlickable = parent.GetComp<CompFlickable>();
        if (compFlickable is { SwitchIsOn: true })
        {
            compPower.PowerOn = true;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref mountedTo, "mountedTo");
        Scribe_Values.Look(ref drawOffset, "drawOffset");
    }

    private void SetDrawOffset()
    {
        var curKindLifeStage = mountedTo.ageTracker.CurKindLifeStage;
        var unreadableTexture = curKindLifeStage.bodyGraphicData.Graphic.MatEast.mainTexture as Texture2D;
        var t = unreadableTexture.GetReadableTexture();
        var backHeight = GetBackHeight(t);
        var backHeightRelative = backHeight / (float)t.height;
        var textureHeight = curKindLifeStage.bodyGraphicData.drawSize.y;
        //If animal texture does not fit in a tile, take this into account
        var extraOffset = textureHeight > 1f ? (textureHeight - 1f) / 2f : 0;
        //Small extra offset, you don't want to draw pawn exactly on back
        extraOffset += 0.5f;
        drawOffset = (textureHeight * backHeightRelative) - extraOffset;
    }

    private static int GetBackHeight(Texture2D t)
    {
        var middle = t.width / 2;
        var backHeight = 0;
        var inBody = false;
        var threshold = 0.8f;


        for (var i = 0; i < t.height; i++)
        {
            var c = t.GetPixel(middle, i);
            if (inBody && c.a < threshold)
            {
                backHeight = i;
                break;
            }

            if (c.a >= threshold)
            {
                inBody = true;
            }
        }

        return backHeight;
    }
}