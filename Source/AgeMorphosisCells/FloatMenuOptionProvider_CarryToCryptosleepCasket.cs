using RimWorld;
using Verse;
using Verse.AI;

namespace AMCells;

public class FloatMenuOptionProvider_CarryToCryptosleepCasket : FloatMenuOptionProvider
{
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => false;

    protected override bool RequiresManipulation => true;

    protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
    {
        if (!clickedPawn.Downed)
        {
            return null;
        }

        if (!context.FirstSelectedPawn.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null,
                true))
        {
            return null;
        }

        if (Building_AMCell.FindAmCellFor(clickedPawn, context.FirstSelectedPawn, true) == null)
        {
            return null;
        }

        return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption("Job_CarryToAMCell".Translate(clickedPawn.LabelCap), action, MenuOptionPriority.Default,
                null, clickedPawn), context.FirstSelectedPawn, clickedPawn);

        void action()
        {
            var buildingCryptosleepCasket = Building_AMCell.FindAmCellFor(clickedPawn, context.FirstSelectedPawn) ??
                                            Building_AMCell.FindAmCellFor(clickedPawn, context.FirstSelectedPawn, true);
            if (buildingCryptosleepCasket == null)
            {
                Messages.Message(
                    string.Concat("CannotCarryToAMCell".Translate(), ": ", "NoAMCell".Translate()), clickedPawn,
                    MessageTypeDefOf.RejectInput);
                return;
            }

            var carryToAMCell = DefDatabase<JobDef>.GetNamed("Job_CarryToAMCell");
            var job = new Job(carryToAMCell, clickedPawn, buildingCryptosleepCasket) { count = 1 };
            context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}