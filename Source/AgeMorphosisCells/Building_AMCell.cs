using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AMCells
{
    public class Building_AMCell : Building, IThingHolder, IOpenable
    {
        protected CompAffectedByFacilities compAffectedByFac;
        protected CompFlickable compFlickable;
        protected CompPowerTrader compPowerTrader;

        private bool destroyedFlag;

        protected ThingOwner innerContainer;

        protected int targetAge = 16;

        public Building_AMCell()
        {
            innerContainer = new ThingOwner<Thing>(this, false);
        }

        public bool HasAnyContents => innerContainer.Count > 0;

        public Thing ContainedThing => innerContainer.Count != 0 ? innerContainer[0] : null;

        public bool IsVitalMonitorsConnected => compAffectedByFac.LinkedFacilitiesListForReading.Count > 0;


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

        private bool HasJob => HasAnyContents && Math.Abs(YearsRemaining) >= 0.02;
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


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>();
            compFlickable = GetComp<CompFlickable>();
            compAffectedByFac = GetComp<CompAffectedByFacilities>();
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

        public virtual bool Accepts(Thing thing)
        {
            return innerContainer.CanAcceptAnyOf(thing);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!Accepts(thing))
            {
                return false;
            }

            bool add;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
                add = true;
            }
            else
            {
                add = innerContainer.TryAdd(thing);
            }

            if (add)
            {
                return true;
            }

            return false;
        }

        public virtual void EjectContents()
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

            if (Faction != Faction.OfPlayer)
            {
                return gizmoList;
            }

            if (destroyedFlag)
            {
                return gizmoList;
            }

            var raiseAgeGizmo = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower"),
                defaultDesc = "ChangeTargetAgeDesc".Translate(),
                defaultLabel = "ChangeTargetAge".Translate(),
                activateSound = SoundDef.Named("Click"),
                //raiseAgeGizmo.action = new Action(RaiseBy1);
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

                void JobAction()
                {
                    var job = new Job(jobDef, this);
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }

                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, JobAction),
                    myPawn, this);
            }
        }

        public static Building_AMCell FindAMCellFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            var allDefs =
                from def in DefDatabase<ThingDef>.AllDefs
                where typeof(Building_AMCell).IsAssignableFrom(def.thingClass)
                select def;
            using var enumerator = allDefs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var Building_AMCell1 = (Building_AMCell)GenClosest.ClosestThingReachable(p.Position, p.Map,
                    ThingRequest.ForDef(current), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f,
                    x => !((Building_AMCell)x).HasAnyContents &&
                         traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations));
                if (Building_AMCell1 == null)
                {
                    continue;
                }

                return Building_AMCell1;
            }

            return null;
        }

        public override void TickRare()
        {
            if (destroyedFlag)
            {
                return;
            }

            base.TickRare();
            DoTickerWork(250);
        }

        public override void Tick()
        {
            if (destroyedFlag)
            {
                return;
            }

            base.Tick();

            DoTickerWork(1);
        }


        private void DoTickerWork(int tickerAmount)
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
                    if (SettingAMCells.settingDoMultiply)
                    {
                        spdfactor *= SettingAMCells.settingMultiplyer;
                    }
                    else
                    {
                        spdfactor /= SettingAMCells.settingMultiplyer;
                    }

                    var ticks = (int)(spdfactor * 20);
                    var yearsWannaChange = (float)ticks / 3600000;
                    if (YearsRemaining < yearsWannaChange)
                    {
                        pawn.ageTracker.AgeBiologicalTicks = targetAge * 3600000;
                    }
                    else
                    {
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
            }

            //this.innerContainer.ThingOwnerTick(true);
            if (FinishedJob)
            {
                Open();
            }
        }
    }
}