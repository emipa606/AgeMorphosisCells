using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AMCells;

public class JobDriver_EnterAgeMorphosisCell : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var pawn1 = pawn;
        var targetA = job.targetA;
        var job1 = job;
        return pawn1.Reserve(targetA, job1, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        var prepare = Toils_General.Wait(500);
        prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        prepare.WithProgressBarToilDelay(TargetIndex.A);
        yield return prepare;
        yield return new Toil
        {
            initAction = delegate
            {
                var actor = CurToil.actor;
                var pod = (Building_AMCell)actor.CurJob.targetA.Thing;

                if (!pod.def.building.isPlayerEjectable)
                {
                    var freeColonistsSpawnedOrInPlayerEjectablePodsCount =
                        Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
                    if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                    {
                        Find.WindowStack.Add(
                            Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate().AdjustedFor(actor),
                                action));
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

                return;

                void action()
                {
                    actor.DeSpawn();
                    pod.TryAcceptThing(actor);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}