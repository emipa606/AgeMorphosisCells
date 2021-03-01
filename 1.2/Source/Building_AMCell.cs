
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;       

using Verse;              

using RimWorld;
using Verse.AI;

namespace AMCells
{

    public class Building_AMCell : Building, IThingHolder, IOpenable
    {

        protected ThingOwner innerContainer;
        protected CompFlickable compFlickable;
        protected CompPowerTrader compPowerTrader;
        protected CompAffectedByFacilities compAffectedByFac;

        protected int targetAge = 16;

        private bool destroyedFlag = false;

        public bool CanOpen => this.HasAnyContents;

        public bool HasAnyContents => innerContainer.Count > 0;

        public Thing ContainedThing => (innerContainer.Count != 0) ? innerContainer[0] : null;

        public Building_AMCell() => innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);

        public ThingOwner GetDirectlyHeldThings() => this.innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren) => ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());

        public virtual void Open()
        {
            if (!this.HasAnyContents)
            {
                return;
            }
            this.EjectContents();
        }

        public bool IsVitalMonitorsConnected => this.compAffectedByFac.LinkedFacilitiesListForReading.Count>0;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
           
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
            this.compFlickable = base.GetComp<CompFlickable>();
            this.compAffectedByFac = base.GetComp<CompAffectedByFacilities>();
        }


        private float YearsRemaining
        {
            get
            {
                float fAge = 0f;
                foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
                {
                    Pawn pawn = current as Pawn;
                    if (pawn != null)
                    {
                        fAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                    }
                }
                return targetAge - fAge; // if positive = grow up; if negative = grow down;
            }
        }

        private bool HasJob => HasAnyContents && Math.Abs(YearsRemaining) >= 0.02;
        private bool FinishedJob => HasAnyContents && Math.Abs(YearsRemaining) <= 0.02;


        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref targetAge, "targetAge", 16);

            Scribe_Deep.Look(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });

        }


        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            
            destroyedFlag = true;

            if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                if (mode != DestroyMode.Deconstruct)
                {
                    List<Pawn> list = new List<Pawn>();
                    foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
                    {
                        Pawn pawn = current as Pawn;
                        if (pawn != null)
                        {
                            list.Add(pawn);
                        }
                    }
                    foreach (Pawn current2 in list)
                    {
                        HealthUtility.DamageUntilDowned(current2);
                    }
                }
                this.EjectContents();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);

            base.Destroy(mode);
        }

        public virtual bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
                flag = true;
            }
            else
            {
                flag = this.innerContainer.TryAdd(thing, true);
            }
            if (flag)
            {
                return true;
            }
            return false;
        }

        public virtual void EjectContents()
        {
            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near);
        }

        #region ticker

        public override void TickRare()
        {
            if (destroyedFlag) 
                return;
            base.TickRare();
            DoTickerWork(250);
        }

        public override void Tick()
        {
            if (destroyedFlag) 
                return;

            base.Tick();
            
            DoTickerWork(1);
        }


        private void DoTickerWork(int tickerAmount)
        {

            
            if (compPowerTrader.PowerOn)
            {
                // Power is on -> do work
                // ----------------------
                Pawn p;
                foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
                {
                    Pawn pawn = current as Pawn;
                    if (pawn != null)
                    {
                        p = pawn;
                        bool grow = YearsRemaining > 0;
                        float spdfactor = 1;
                        /*
                       TODO: add worker Pawn stats after. Basically; 1 science = 20% speed; 5 sci = 100% speed; 20 sci = 400% speed.
                       100% speed is 1 year to 3 days (year is 60 days), so 20:1 ratio.
                       1 year is 3,600,000 ticks. 1 day is 60,000 ticks.
                        */
                        if (SettingAMCells.settingDoMultiply) spdfactor *= SettingAMCells.settingMultiplyer; else spdfactor /= SettingAMCells.settingMultiplyer;
                        int ticks = (int)(spdfactor * 20);
                        float yearsWannaChange = ticks / 3600000;
                        if (YearsRemaining < yearsWannaChange)
                        {
                            p.ageTracker.AgeBiologicalTicks = targetAge * 3600000;
                        }
                        else
                        {
                            if (grow) p.ageTracker.AgeBiologicalTicks += ticks; else p.ageTracker.AgeBiologicalTicks -= ticks;
                        }
                    }
                }
                
            }
            else
            {
                // Power off

                
            }
            //this.innerContainer.ThingOwnerTick(true);
            if (this.FinishedJob) Open();
        }

#endregion

        public override string GetInspectString()
        {
            StringBuilder text = new StringBuilder(base.GetInspectString());
            string str;
            str = this.innerContainer.ContentsString;
            
            if (!text.ToString().NullOrEmpty())
            {
                text.Append("\n");
            }

            text.Append("CasketContains".Translate() + ": " + str);

            text.Append("\n");
            foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
            {
                Pawn pawn = current as Pawn;
                if (pawn != null)
                {
                    text.Append("CurrentAgeDesc".Translate() + pawn.ageTracker.AgeBiologicalYears);
                    text.Append("\n");
                }
            }

            text.Append("TargetAgeDesc".Translate() + targetAge.ToString() + "TargetAgeDescYrs".Translate());
            return text.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000200;

            List<Gizmo> gizmoList = new List<Gizmo>();
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                gizmoList.Add(gizmo);
            }
            if (this.Faction != Faction.OfPlayer)
            {
                return gizmoList;
            }

            if (this.destroyedFlag == false)
            {
                Command_Action raiseAgeGizmo = new Command_Action();
                raiseAgeGizmo.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower", true);
                raiseAgeGizmo.defaultDesc = "ChangeTargetAgeDesc".Translate();
                raiseAgeGizmo.defaultLabel = "ChangeTargetAge".Translate();
                raiseAgeGizmo.activateSound = SoundDef.Named("Click");
                //raiseAgeGizmo.action = new Action(RaiseBy1);
                raiseAgeGizmo.action = () =>
                {
                    Find.WindowStack.Add(new Dialog_SliderWithInfo("ChooseTargetAge".Translate(), 1, 100, delegate (int x)
                    {
                        this.targetAge = x;
                    }, this.targetAge));
                };
                raiseAgeGizmo.groupKey = groupKeyBase + 1;
                gizmoList.Add(raiseAgeGizmo);
            }
            
            return gizmoList;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
            {
                yield return o;
            }
            if (this.innerContainer.Count == 0)
            {
                if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return failer;
                }
                else
                {
                    JobDef jobDef = DefDatabase<JobDef>.GetNamed("Job_EnterAMCell");
                    string jobStr = "EnterAgeMorphosisCell".Translate(); ;
                    Action jobAction = delegate
                    {
                        Job job = new Job(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
                }
            }
        }

        public static Building_AMCell FindAMCellFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            Building_AMCell building_AMCell;
            IEnumerable<ThingDef> allDefs =
                from def in DefDatabase<ThingDef>.AllDefs
                where typeof(Building_AMCell).IsAssignableFrom(def.thingClass)
                select def;
            using (IEnumerator<ThingDef> enumerator = allDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ThingDef current = enumerator.Current;
                    Building_AMCell Building_AMCell1 = (Building_AMCell)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(current), PathEndMode.InteractionCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Thing x) => (((Building_AMCell)x).HasAnyContents ? false : traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations)), null, 0, -1, false, RegionType.Set_Passable, false);
                    if (Building_AMCell1 == null)
                    {
                        continue;
                    }
                    building_AMCell = Building_AMCell1;
                    return building_AMCell;
                }
                return null;
            }
            return building_AMCell;
        }

    }
}
