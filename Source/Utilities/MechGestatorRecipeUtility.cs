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
    }
}
