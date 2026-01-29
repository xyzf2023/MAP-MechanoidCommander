using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    // 允许指挥官满足机械师判定
    [HarmonyPatch(typeof(MechanitorUtility), "ShouldBeMechanitor")]
    public static class Patch_ShouldBeMechanitor
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (__result || pawn == null)
            {
                return;
            }

            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if (pawn.Faction == null || !pawn.Faction.IsPlayerSafe())
            {
                return;
            }

            if (CompMechanitorMarker.PawnHasMarker(pawn))
            {
                __result = true;
            }

            if (!__result && pawn.RaceProps.IsMechanoid && BandwidthComponentUtility.HasBandwidthComponent(pawn))
            {
                __result = true;
            }
        }
    }

    // 允许指挥官通过机械师判定（避免显示空原因）
    [HarmonyPatch(typeof(MechanitorUtility), "IsMechanitor")]
    public static class Patch_IsMechanitor
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result || pawn == null)
            {
                return;
            }

            if (CompMechanitorMarker.PawnHasMarker(pawn) && pawn.Faction == Faction.OfPlayer)
            {
                BandwidthComponentUtility.EnsureMechanitorTracker(pawn);
                __result = true;
            }
        }
    }

    // 为指挥官补齐机械师/关系组件
    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class Patch_AddAndRemoveDynamicComponents
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn)
        {
            if (pawn == null || !ModsConfig.BiotechActive)
            {
                return;
            }

            if (!pawn.RaceProps.IsMechanoid)
            {
                return;
            }

            bool shouldBeMechanitor = MechanitorUtility.ShouldBeMechanitor(pawn);
            if (shouldBeMechanitor)
            {
                if (pawn.mechanitor == null)
                {
                    pawn.mechanitor = new Pawn_MechanitorTracker(pawn);
                }

                if (pawn.relations == null)
                {
                    pawn.relations = new Pawn_RelationsTracker(pawn);
                }
            }
            else if (pawn.mechanitor != null)
            {
                pawn.mechanitor = null;
            }
        }
    }

    // 阻止指挥官相互控制
    [HarmonyPatch(typeof(MechanitorUtility), "CanControlMech")]
    public static class Patch_MechanitorUtility_CanControlMech
    {
        private const string JusticeDefName = "MAP_Mech_Justice";
        private const string HermitDefName = "MAP_Mech_Hermit";

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Pawn mech, ref AcceptanceReport __result)
        {
            if (!__result.Accepted || pawn == null || mech == null)
            {
                return;
            }

            if (CompMechanitorMarker.PawnHasMarker(mech))
            {
                if (pawn.RaceProps.IsMechanoid)
                {
                    string? defName = mech.def?.defName;
                    if (defName == JusticeDefName)
                    {
                        __result = new AcceptanceReport("MechCommander.Messages.Justice.CannotBeControlledByMech".Translate());
                    }
                    else if (defName == HermitDefName)
                    {
                        __result = new AcceptanceReport("MechCommander.Messages.Hermit.CannotBeControlledByMech".Translate());
                    }
                    else
                    {
                        __result = new AcceptanceReport("MechCommander.Messages.Justice.CannotBeControlledByMech".Translate());
                    }
                }
                return;
            }

            if (mech.RaceProps.IsMechanoid && BandwidthComponentUtility.HasBandwidthComponent(mech)
                && pawn.RaceProps.IsMechanoid)
            {
                if (CompMechanitorMarker.PawnHasMarker(mech))
                {
                    __result = new AcceptanceReport("MechCommander.Messages.Justice.CannotBeControlledByMech".Translate());
                }
                else
                {
                    __result = new AcceptanceReport("MechCommander.Messages.BandwidthLink.CannotBeControlledByMech".Translate());
                }
            }
        }
    }

    // 指挥官始终视为在机控范围内
    [HarmonyPatch(typeof(MechanitorUtility), "InMechanitorCommandRange")]
    public static class Patch_MechanitorUtility_InMechanitorCommandRange
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(Pawn mech, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (mech != null && CompMechanitorMarker.PawnHasMarker(mech))
            {
                __result = true;
                return;
            }

            if (mech != null && BandwidthComponentUtility.HasBandwidthComponent(mech))
            {
                __result = true;
                return;
            }

            Pawn? overseer = mech?.GetOverseer();
            if (overseer?.def?.defName == JusticeDefName)
            {
                __result = true;
            }
        }
    }

    // 指挥官隐藏机控范围绘制
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), "DrawCommandRadius")]
    public static class Patch_Pawn_MechanitorTracker_DrawCommandRadius
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPrefix]
        public static bool Prefix(Pawn_MechanitorTracker __instance)
        {
            Pawn? pawn = __instance?.Pawn;
            if (pawn?.def?.defName == JusticeDefName)
            {
                return false;
            }

            if (pawn != null && BandwidthComponentUtility.HasBandwidthComponent(pawn))
            {
                return false;
            }

            return true;
        }
    }

    // 指挥官无法行动时仍允许其机械体被征召
    [HarmonyPatch(typeof(MechanitorUtility), "CanDraftMech")]
    public static class Patch_MechanitorUtility_CanDraftMech
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(Pawn mech, ref AcceptanceReport __result)
        {
            if (__result.Accepted || mech == null || !mech.IsColonyMech)
            {
                return;
            }

            if (mech.needs?.energy != null && mech.needs.energy.IsLowEnergySelfShutdown)
            {
                return;
            }

            Pawn overseer = mech.GetOverseer();
            if (overseer == null || overseer.def?.defName != JusticeDefName)
            {
                return;
            }

            if (overseer.mechanitor != null && overseer.mechanitor.ControlledPawns.Contains(mech))
            {
                __result = true;
            }
        }
    }

    // 为指挥官添加DEV分配按钮
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), "GetGizmos")]
    public static class Patch_Pawn_MechanitorTracker_GetGizmos
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_MechanitorTracker __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!DebugSettings.ShowDevGizmos)
            {
                return;
            }

            Pawn mechanitor = __instance.Pawn;
            if (mechanitor == null)
            {
                return;
            }

            IEnumerable<Gizmo> extra = new[]
            {
                new Command_Action
                {
                    defaultLabel = "MechCommander.Dev.AssignFriendlyMechs".Translate(),
                    action = delegate
                    {
                        List<Pawn> selectedPawns = Find.Selector.SelectedPawns.ToList();

                        List<Pawn> selectedMechs = selectedPawns
                            .Where(p => p.RaceProps.IsMechanoid && p != mechanitor && !CompMechanitorMarker.PawnHasMarker(p))
                            .ToList();

                        if (!selectedMechs.Any())
                        {
                            Log.Warning("[「正义」] DEV分配：未选中有效机械体，改为列表选择...");
                            Map map = mechanitor.Map;
                            if (map == null)
                            {
                                return;
                            }

                            List<Pawn> colonyMechs = map.mapPawns.AllPawnsSpawned
                                .Where(p => p.RaceProps.IsMechanoid
                                    && p.IsColonyMech
                                    && p != mechanitor
                                    && !CompMechanitorMarker.PawnHasMarker(p)
                                    && p.GetOverseer() != mechanitor)
                                .ToList();

                            if (!colonyMechs.Any())
                            {
                                Log.Warning("[「正义」] DEV分配：地图上未找到殖民地机械体。");
                                return;
                            }

                            List<FloatMenuOption> menu = new List<FloatMenuOption>();
                            for (int i = 0; i < colonyMechs.Count; i++)
                            {
                                Pawn target = colonyMechs[i];
                                menu.Add(new FloatMenuOption("MechCommander.Dev.AssignOption".Translate(target.LabelShortCap), delegate
                                {
                                    Pawn overseer = target.GetOverseer();
                                    if (overseer != null)
                                    {
                                        overseer.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, target);
                                    }
                                    target.SetFaction(Faction.OfPlayer, null);
                                    mechanitor.relations.AddDirectRelation(PawnRelationDefOf.Overseer, target);
                                }));
                            }

                            Find.WindowStack.Add(new FloatMenu(menu));
                            return;
                        }

                        Log.Message("[「正义」] DEV分配：监管者=" + mechanitor.LabelShortCap + "，数量=" + selectedMechs.Count);
                        for (int i = 0; i < selectedMechs.Count; i++)
                        {
                            Pawn mech = selectedMechs[i];
                            Log.Message("[「正义」] DEV分配：目标=" + mech.LabelShortCap + "，旧监管者=" + (mech.GetOverseer() != null ? mech.GetOverseer().LabelShortCap : "无"));
                            Pawn overseer = mech.GetOverseer();
                            if (overseer != null)
                            {
                                overseer.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, mech);
                            }
                            mech.SetFaction(Faction.OfPlayer, null);
                            mechanitor.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
                        }
                    }
                }
            };

            __result = __result.Concat(extra);
        }
    }

    // 放行指挥官的机控右键菜单
    [HarmonyPatch(typeof(FloatMenuOptionProvider), "SelectedPawnValid")]
    public static class Patch_FloatMenuOptionProvider_SelectedPawnValid_Mechanitor
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, FloatMenuContext context, ref bool __result, FloatMenuOptionProvider __instance)
        {
            if (__result || pawn == null || !pawn.RaceProps.IsMechanoid)
            {
                return;
            }

            if (__instance is not FloatMenuOptionProvider_Mechanitor)
            {
                return;
            }

            if (!CompMechanitorMarker.PawnHasMarker(pawn) && !BandwidthComponentUtility.HasBandwidthComponent(pawn))
            {
                return;
            }

            var traverse = Traverse.Create(__instance);
            try
            {
                bool drafted = traverse.Property("Drafted").GetValue<bool>();
                bool undrafted = traverse.Property("Undrafted").GetValue<bool>();
                bool requiresManipulation = traverse.Property("RequiresManipulation").GetValue<bool>();

                bool draftedOk = drafted || !pawn.Drafted;
                bool undraftedOk = undrafted || pawn.Drafted;
                bool manipulationOk = !requiresManipulation || (pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Manipulation) ?? false);

                if (draftedOk && undraftedOk && manipulationOk)
                {
                    __result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[「正义」] 右键菜单补丁失败: " + ex.Message);
            }
        }
    }

    // 避免控制组为空导致机械族列表报错
    [HarmonyPatch(typeof(PawnColumnWorker_ControlGroup), "DoCell")]
    public static class Patch_PawnColumnWorker_ControlGroup_DoCell
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn)
        {
            if (pawn == null || pawn.IsGestating())
            {
                return true;
            }

            Pawn overseer = pawn.GetOverseer();
            if (overseer == null)
            {
                return true;
            }

            if ((CompMechanitorMarker.PawnHasMarker(pawn) || BandwidthComponentUtility.HasBandwidthComponent(pawn))
                && pawn.GetMechControlGroup() == null && overseer.mechanitor != null)
            {
                overseer.mechanitor.AssignPawnControlGroup(pawn, null);
            }

            if (CompMechanitorMarker.PawnHasMarker(pawn) || BandwidthComponentUtility.HasBandwidthComponent(pawn))
            {
                return pawn.GetMechControlGroup() != null;
            }

            return true;
        }
    }
}
