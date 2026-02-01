using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    /// <summary>
    /// 机械体工作设置相关工具。用于在刚创建时修正 workSettings，避免因 IsColonyMech 尚未为 true 导致 EnableAndInitialize 启用全部工作类型。
    /// </summary>
    public static class MechWorkSettingsUtility
    {
        /// <summary>
        /// 将 pawn 的 workSettings 限制为仅启用 RaceProps.mechEnabledWorkTypes 中的工作类型。
        /// 仅在 Biotech 启用、pawn 为机械体且 mechEnabledWorkTypes 非空时执行；否则无操作。
        /// </summary>
        public static void RestrictToMechEnabledWorkTypes(Pawn pawn)
        {
            if (pawn?.workSettings == null || !pawn.workSettings.EverWork)
            {
                return;
            }

            if (!ModsConfig.BiotechActive || !pawn.RaceProps.IsMechanoid || pawn.RaceProps.mechEnabledWorkTypes.NullOrEmpty())
            {
                return;
            }

            List<WorkTypeDef> allowed = pawn.RaceProps.mechEnabledWorkTypes;
            List<WorkTypeDef> allWorkTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            for (int i = 0; i < allWorkTypes.Count; i++)
            {
                WorkTypeDef w = allWorkTypes[i];
                if (!allowed.Contains(w))
                {
                    pawn.workSettings.Disable(w);
                }
            }

            // 清除「禁用工作类型」缓存，使 UI 按 mechEnabledWorkTypes 重新计算并显示为完全禁用（灰显/隐藏），与读档后一致
            pawn.Notify_DisabledWorkTypesChanged();
        }
    }
}
