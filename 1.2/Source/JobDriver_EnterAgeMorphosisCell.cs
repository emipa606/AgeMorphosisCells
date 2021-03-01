using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace AMCells
{
    public class JobDriver_EnterAgeMorphosisCell : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil prepare = Toils_General.Wait(500);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return prepare;
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = CurToil.actor;
                    Building_AMCell pod = (Building_AMCell)actor.CurJob.targetA.Thing;
                    Action action = delegate
                    {
                        actor.DeSpawn();
                        pod.TryAcceptThing(actor, true);
                    };
                    if (!pod.def.building.isPlayerEjectable)
                    {
                        int freeColonistsSpawnedOrInPlayerEjectablePodsCount = this.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
                        if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate().AdjustedFor(actor), action, false, null));
                        }
                        else
                        {
                            action();
                        }
                    }
                    else
                    {
                        action();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
