using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    public static class MechGestatorRecipeUtility
    {
        private const string JusticePawnDefName = "MAP_Mech_Justice";
        private const string JusticeGestateDefName = "MAP_Gestate_Justice";
        private const string HermitGestateDefName = "MAP_Gestate_Hermit";

        public static bool IsJusticeDisabledForGestation(RecipeDef? recipe, Pawn? pawn)
        {
            if (recipe == null || pawn == null)
            {
                return false;
            }

            if (!IsJusticeOrHermitGestation(recipe))
            {
                return false;
            }

            return pawn.def?.defName == JusticePawnDefName;
        }

        public static bool IsJusticeOrHermitGestation(RecipeDef? recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            string defName = recipe.defName ?? string.Empty;
            return defName == JusticeGestateDefName || defName == HermitGestateDefName;
        }

        /// <summary>
        /// 正义不可培育该配方时，在账单配置中显示的禁用理由的翻译 key。
        /// 培育正义用「正义不能由机械体培育」，培育隐者用「隐者不能由机械体培育」。
        /// </summary>
        public static string GetJusticeDisabledReasonKey(RecipeDef? recipe)
        {
            if (recipe == null)
            {
                return "MechCommander.Bill.Reason.JusticeCannotGestate";
            }

            string defName = recipe.defName ?? string.Empty;
            return defName == HermitGestateDefName
                ? "MechCommander.Bill.Reason.HermitCannotGestate"
                : "MechCommander.Bill.Reason.JusticeCannotGestate";
        }
    }
}
