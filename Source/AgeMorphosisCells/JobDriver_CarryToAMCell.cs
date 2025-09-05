using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AMCells;

public class JobDriver_CarryToAMCell : JobDriver
{
    private const TargetIndex TakeeInd = TargetIndex.A;

    private const TargetIndex DropPodInd = TargetIndex.B;

    private Pawn Takee => (Pawn)job.GetTarget(TakeeInd).Thing;

    private Building_AMCell DropPod => (Building_AMCell)job.GetTarget(DropPodInd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        //return (!this.pawn.Reserve(this.Takee, this.job, 1, -1, null) ? false : this.pawn.Reserve(this.DropPod, this.job, 1, -1, null));

        var pawn1 = pawn;
        LocalTargetInfo target = Takee;
        var job1 = job;
        bool arg580;
        if (pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed))
        {
            pawn1 = pawn;
            target = DropPod;
            job1 = job;
            arg580 = pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed);
        }
        else
        {
            arg580 = false;
        }

        return arg580;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TakeeInd);
        this.FailOnDestroyedOrNull(DropPodInd);
        this.FailOnAggroMentalState(TakeeInd);
        yield return Toils_Reserve.Reserve(TakeeInd);
        yield return Toils_Reserve.Reserve(DropPodInd);
        yield return Toils_Goto.GotoThing(TakeeInd, PathEndMode.OnCell)
            .FailOnDestroyedNullOrForbidden(TakeeInd)
            .FailOnDespawnedNullOrForbidden(DropPodInd)
            .FailOn(() => DropPod.GetDirectlyHeldThings().Count > 0)
            .FailOn(() => !Takee.Downed)
            .FailOn(() => !pawn.CanReach(Takee, PathEndMode.OnCell, Danger.Deadly))
            .FailOnSomeonePhysicallyInteracting(TakeeInd);
        yield return Toils_Haul.StartCarryThing(TakeeInd);
        yield return Toils_Goto.GotoThing(DropPodInd, PathEndMode.InteractionCell);
        var prepare = Toils_General.Wait(500);
        prepare.FailOnCannotTouch(DropPodInd, PathEndMode.InteractionCell);
        prepare.WithProgressBarToilDelay(DropPodInd);
        yield return prepare;
        yield return new Toil
        {
            initAction = delegate { DropPod.TryAcceptThing(Takee); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}