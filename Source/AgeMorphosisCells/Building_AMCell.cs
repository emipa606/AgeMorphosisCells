using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AMCells;

public class Building_AMCell : Building, ISuspendableThingHolder, IOpenable
{
    private CompPowerTrader compPowerTrader;

    private bool destroyedFlag;

    private ThingOwner innerContainer;

    private int targetAge = 16;

    public Building_AMCell()
    {
        innerContainer = new ThingOwner<Thing>(this, false);
    }


    private bool HasAnyContents => innerContainer.Count > 0;


    private float YearsRemaining
    {
        get
        {
            var fAge = 0f;
            foreach (var current in innerContainer)
            {
                if (current is Pawn pawn)
                {
                    fAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                }
            }

            return targetAge - fAge; // if positive = grow up; if negative = grow down;
        }
    }

    private bool FinishedJob => HasAnyContents && Math.Abs(YearsRemaining) <= 0.02;

    public int OpenTicks => 100;

    public bool CanOpen => HasAnyContents;

    public virtual void Open()
    {
        if (!HasAnyContents)
        {
            return;
        }

        EjectContents();
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public bool IsContentsSuspended => base.Suspended || compPowerTrader.PowerOn;


    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        compPowerTrader = GetComp<CompPowerTrader>();
        GetComp<CompFlickable>();
        GetComp<CompAffectedByFacilities>();
    }


    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref targetAge, "targetAge", 16);

        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }


    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        destroyedFlag = true;

        if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
        {
            if (mode != DestroyMode.Deconstruct)
            {
                var list = new List<Pawn>();
                foreach (var current in innerContainer)
                {
                    if (current is Pawn pawn)
                    {
                        list.Add(pawn);
                    }
                }

                foreach (var current2 in list)
                {
                    HealthUtility.DamageUntilDowned(current2);
                }
            }

            EjectContents();
        }

        innerContainer.ClearAndDestroyContents();

        base.Destroy(mode);
    }

    private bool accepts(Thing thing)
    {
        return innerContainer.CanAcceptAnyOf(thing);
    }

    public void TryAcceptThing(Thing thing)
    {
        if (!accepts(thing))
        {
            return;
        }

        if (thing.holdingOwner != null)
        {
            thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
        }
        else
        {
            innerContainer.TryAdd(thing);
        }
    }

    protected virtual void EjectContents()
    {
        innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
    }

    public override string GetInspectString()
    {
        var text = new StringBuilder(base.GetInspectString());
        var str = innerContainer.ContentsString;

        if (!text.ToString().NullOrEmpty())
        {
            text.Append("\n");
        }

        text.Append("CasketContains".Translate() + ": " + str);

        text.Append("\n");
        foreach (var current in innerContainer)
        {
            if (current is not Pawn pawn)
            {
                continue;
            }

            text.Append("CurrentAgeDesc".Translate() + pawn.ageTracker.AgeBiologicalYears);
            text.Append("\n");
        }

        text.Append("TargetAgeDesc".Translate() + targetAge.ToString() + "TargetAgeDescYrs".Translate());
        return text.ToString();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var groupKeyBase = 700000200;

        var gizmoList = new List<Gizmo>();
        foreach (var gizmo in base.GetGizmos())
        {
            gizmoList.Add(gizmo);
        }

        if (Faction != Faction.OfPlayer || destroyedFlag)
        {
            return gizmoList;
        }

        var raiseAgeGizmo = new Command_Action
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower"),
            defaultDesc = "ChangeTargetAgeDesc".Translate(),
            defaultLabel = "ChangeTargetAge".Translate(),
            activateSound = SoundDef.Named("Click"),
            action = () =>
            {
                Find.WindowStack.Add(new Dialog_SliderWithInfo("ChooseTargetAge".Translate(), 1, 100,
                    delegate(int x) { targetAge = x; }, targetAge));
            },
            groupKey = groupKeyBase + 1
        };
        gizmoList.Add(raiseAgeGizmo);

        return gizmoList;
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
    {
        foreach (var o in base.GetFloatMenuOptions(myPawn))
        {
            yield return o;
        }

        if (innerContainer.Count != 0)
        {
            yield break;
        }

        if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
        {
            var failer = new FloatMenuOption("CannotUseNoPath".Translate(), null);
            yield return failer;
        }
        else
        {
            var jobDef = DefDatabase<JobDef>.GetNamed("Job_EnterAMCell");
            string jobStr = "EnterAgeMorphosisCell".Translate();

            void jobAction()
            {
                var job = new Job(jobDef, this);
                myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction),
                myPawn, this);
        }
    }

    public static Building_AMCell FindAmCellFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
    {
        var allDefs =
            from def in DefDatabase<ThingDef>.AllDefs
            where typeof(Building_AMCell).IsAssignableFrom(def.thingClass)
            select def;
        using var enumerator = allDefs.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            var buildingAmCell1 = (Building_AMCell)GenClosest.ClosestThingReachable(p.Position, p.Map,
                ThingRequest.ForDef(current), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f,
                x => !((Building_AMCell)x).HasAnyContents &&
                     traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations));
            if (buildingAmCell1 == null)
            {
                continue;
            }

            return buildingAmCell1;
        }

        return null;
    }

    protected override void Tick()
    {
        if (destroyedFlag)
        {
            return;
        }

        base.Tick();

        doTickerWork();
    }


    private void doTickerWork()
    {
        if (compPowerTrader.PowerOn)
        {
            // Power is on -> do work
            // ----------------------
            foreach (var current in innerContainer)
            {
                if (current is not Pawn pawn)
                {
                    continue;
                }

                var grow = YearsRemaining > 0;
                float spdfactor = 1;
                /*
                   TODO: add worker Pawn stats after. Basically; 1 science = 20% speed; 5 sci = 100% speed; 20 sci = 400% speed.
                   100% speed is 1 year to 3 days (year is 60 days), so 20:1 ratio.
                   1 year is 3,600,000 ticks. 1 day is 60,000 ticks.
                    */
                if (AMCellsMod.Instance.Settings.DoMultiply)
                {
                    spdfactor *= AMCellsMod.Instance.Settings.Multiplyer;
                }
                else
                {
                    spdfactor /= AMCellsMod.Instance.Settings.Multiplyer;
                }

                var ticks = (int)(spdfactor * 20);
                if (grow)
                {
                    pawn.ageTracker.AgeBiologicalTicks += ticks;
                }
                else
                {
                    pawn.ageTracker.AgeBiologicalTicks -= ticks;
                }
            }
        }

        if (!FinishedJob)
        {
            return;
        }

        var completePawn = (Pawn)innerContainer.First();
        Open();
        Messages.Message("AMCellCompleted".Translate(completePawn.NameFullColored, targetAge), completePawn,
            MessageTypeDefOf.PositiveEvent);
    }
}