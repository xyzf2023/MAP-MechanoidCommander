using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    [HarmonyPatch]
    public static class ShuttleAllowPatches
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        private static bool IsJustice(Thing t)
        {
            Pawn? pawn = t as Pawn;
            return pawn?.def?.defName == JusticeDefName;
        }

        [HarmonyPatch(typeof(CompShuttle), "IsAllowed")]
        [HarmonyPostfix]
        public static void IsAllowed_Postfix(Thing t, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (!ModsConfig.OdysseyActive)
            {
                return;
            }

            if (IsJustice(t))
            {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(CompShuttle), "IsAllowedNow")]
        [HarmonyPostfix]
        public static void IsAllowedNow_Postfix(Thing t, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (!ModsConfig.OdysseyActive)
            {
                return;
            }

            if (IsJustice(t))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(CompShuttle), "get_HasPilot")]
    public static class Patch_CompShuttle_HasPilot
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPrefix]
        public static bool Prefix(CompShuttle __instance, ref bool __result)
        {
            if (!ModsConfig.OdysseyActive)
            {
                return true;
            }

            ThingOwner? innerContainer = __instance.Transporter?.innerContainer;
            if (innerContainer == null)
            {
                return true;
            }

            for (int i = 0; i < innerContainer.Count; i++)
            {
                Pawn? pawn = innerContainer[i] as Pawn;
                if (pawn == null || pawn.def?.defName != JusticeDefName)
                {
                    continue;
                }

                if (StatDefOf.PilotingAbility.Worker.IsDisabledFor(pawn))
                {
                    continue;
                }

                if (pawn.GetStatValue(StatDefOf.PilotingAbility, true, -1) > 0.1f)
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CompShuttle), "CompFloatMenuOptions")]
    public static class Patch_CompShuttle_CompFloatMenuOptions
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(CompShuttle __instance, Pawn selPawn, ref IEnumerable<FloatMenuOption> __result)
        {
            if (!ModsConfig.OdysseyActive || selPawn?.def?.defName != JusticeDefName)
            {
                return;
            }

            List<FloatMenuOption> options = __result?.ToList() ?? new List<FloatMenuOption>();
            if (Prefs.DevMode)
            {
                Log.Message("[「正义」] 登上穿梭机菜单检查：已有选项=" + options.Count +
                    " 可达=" + selPawn.CanReach(__instance.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn) +
                    " 目标=" + __instance.parent.LabelShortCap);
            }
            string label = "EnterShuttle".Translate();
            if (options.Any(opt => opt.Label == label))
            {
                __result = options;
                return;
            }

            if (!selPawn.CanReach(__instance.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                options.Add(new FloatMenuOption(label + " (" + "NoPath".Translate() + ")", null));
                __result = options;
                return;
            }

            options.Add(new FloatMenuOption(label, delegate
            {
                if (!__instance.Transporter.LoadingInProgressOrReadyToLaunch)
                {
                    TransporterUtility.InitiateLoading(Gen.YieldSingle<CompTransporter>(__instance.Transporter));
                }
                Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, __instance.parent);
                selPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
            }));

            __result = options;
        }
    }
}
