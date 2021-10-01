using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AMCells
{
    public static class Patches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static class FloatMenuMakerCarryAdder
        {
            [HarmonyPostfix]
            public static void AddFloatMenuOption(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
            {
                foreach (var localTargetInfo1 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    var localTargetInfo2 = localTargetInfo1;
                    var thing2 = (Pawn)localTargetInfo2.Thing;
                    if (!thing2.Downed ||
                        !pawn.CanReserveAndReach(thing2, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) ||
                        Building_AMCell.FindAMCellFor(thing2, pawn, true) == null)
                    {
                        continue;
                    }

                    string str3 = "Job_CarryToAMCell".Translate(localTargetInfo2.Thing.LabelCap);
                    var carryToAMCell = DefDatabase<JobDef>.GetNamed("Job_CarryToAMCell");

                    void Action1()
                    {
                        var buildingCryptosleepCasket = Building_AMCell.FindAMCellFor(thing2, pawn) ??
                                                        Building_AMCell.FindAMCellFor(thing2, pawn, true);
                        if (buildingCryptosleepCasket == null)
                        {
                            Messages.Message(
                                string.Concat("CannotCarryToAMCell".Translate(), ": ", "NoAMCell".Translate()), thing2,
                                MessageTypeDefOf.RejectInput);
                            return;
                        }

                        var job = new Job(carryToAMCell, thing2, buildingCryptosleepCasket) { count = 1 };
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }

                    var action = (Action)Action1;
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(
                        new FloatMenuOption(str3, action, MenuOptionPriority.Default, null, thing2), pawn, thing2));
                }
            }
        }
    }
}