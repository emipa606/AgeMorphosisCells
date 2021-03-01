using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using RimWorld;
using Verse;
using UnityEngine;
using HugsLib.Settings;
using Verse.AI;
using HarmonyLib;

namespace AMCells
{
    public class SettingAMCells : ModBase
    {
        public override string ModIdentifier => "AgeMorphosisCells";

        private SettingHandle<int> multiplyer;
        private SettingHandle<bool> doMultiply;

        public static int settingMultiplyer = 1;
        public static bool settingDoMultiply = true;

        public override void DefsLoaded()
        {
            multiplyer = Settings.GetHandle<int>("multiplyer", "settings_multiplyer_title".Translate(), "settings_multiplyer_desc".Translate(), 1);
            doMultiply = Settings.GetHandle<bool>("doMultiply", "settings_doMultiply_title".Translate(), "settings_doMultiply_desc".Translate(), true);
            settingMultiplyer = multiplyer.Value;
            settingDoMultiply = doMultiply.Value;
        }

        public override void SettingsChanged()
        {
            base.SettingsChanged();
            if (multiplyer.Value < 0) multiplyer.Value = 0;
            settingMultiplyer = multiplyer.Value;
            settingDoMultiply = doMultiply.Value;
        }
    }

    public static class Patches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static class FloatMenuMakerCarryAdder
        {
            [HarmonyPostfix]
            public static void AddFloatMenuOption(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
            {
                string str2;
                Action action;
                Pawn pawn1;
                foreach (LocalTargetInfo localTargetInfo1 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    LocalTargetInfo localTargetInfo2 = localTargetInfo1;
                    Pawn thing2 = (Pawn)localTargetInfo2.Thing;
                    if (!thing2.Downed || !pawn.CanReserveAndReach(thing2, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) || Building_AMCell.FindAMCellFor(thing2, pawn, true) == null)
                    {
                        continue;
                    }
                    string str3 = TranslatorFormattedStringExtensions.Translate("Job_CarryToAMCell", localTargetInfo2.Thing.LabelCap);
                    JobDef carryToAMCell = DefDatabase<JobDef>.GetNamed("Job_CarryToAMCell");

                    Action action1 = () => {
                        Building_AMCell buildingCryptosleepCasket = Building_AMCell.FindAMCellFor(thing2, pawn, false) ?? Building_AMCell.FindAMCellFor(thing2, pawn, true);
                        if (buildingCryptosleepCasket == null)
                        {
                            Messages.Message(string.Concat("CannotCarryToAMCell".Translate(), ": ", "NoAMCell".Translate()), thing2, MessageTypeDefOf.RejectInput);
                            return;
                        }
                        Job job = new Job(carryToAMCell, thing2, buildingCryptosleepCasket)
                        {
                            count = 1
                        };
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    str2 = str3;
                    action = action1;
                    pawn1 = thing2;
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(str2, action, MenuOptionPriority.Default, null, pawn1, 0f, null, null), pawn, thing2, "ReservedBy"));
                }
            }
        }
    }
}
