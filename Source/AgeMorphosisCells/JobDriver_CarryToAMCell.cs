using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AMCells;

public class JobDriver_CarryToAMCell : JobDriver
{
    private const TargetIndex TakeeInd = TargetIndex.A;

    private const TargetIndex DropPodInd = TargetIndex.B;

    protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    protected Building_AMCell DropPod => (Building_AMCell)job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        //return (!this.pawn.Reserve(this.Takee, this.job, 1, -1, null) ? false : this.pawn.Reserve(this.DropPod, this.job, 1, -1, null));

        var pawn1 = pawn;
        LocalTargetInfo target = Takee;
        var job1 = job;
        bool arg_58_0;
        if (pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed))
        {
            pawn1 = pawn;
            target = DropPod;
            job1 = job;
            arg_58_0 = pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed);
        }
        else
        {
            arg_58_0 = false;
        }

        return arg_58_0;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        this.FailOnAggroMentalState(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B)
            .FailOn(() => DropPod.GetDirectlyHeldThings().Count > 0)
            .FailOn(() => !Takee.Downed)
            .FailOn(() => !pawn.CanReach(Takee, PathEndMode.OnCell, Danger.Deadly))
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
        var prepare = Toils_General.Wait(500);
        prepare.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
        prepare.WithProgressBarToilDelay(TargetIndex.B);
        yield return prepare;
        yield return new Toil
        {
            initAction = delegate { DropPod.TryAcceptThing(Takee); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}