using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MAP_MechCommander
{
    public class MechCommanderModSettings : ModSettings
    {
        public HashSet<string> hediffBlacklist = new HashSet<string>();
        public bool addJusticeToWorkTab = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hediffBlacklist, "hediffBlacklist", LookMode.Value);
            Scribe_Values.Look(ref addJusticeToWorkTab, "addJusticeToWorkTab", false);
            if (hediffBlacklist == null)
            {
                hediffBlacklist = new HashSet<string>();
            }
        }

        public void CleanupForCurrentDefs()
        {
            HashSet<string> candidates = JusticeHediffSyncUtility
                .GetCandidateHediffDefsSorted()
                .Select(d => d.defName)
                .ToHashSet();

            hediffBlacklist.RemoveWhere(defName => !candidates.Contains(defName));
        }

        public bool IsBlacklisted(HediffDef def)
        {
            return def != null && hediffBlacklist.Contains(def.defName);
        }

        public void SetBlacklisted(HediffDef def, bool blocked)
        {
            if (def == null)
            {
                return;
            }

            if (blocked)
            {
                hediffBlacklist.Add(def.defName);
            }
            else
            {
                hediffBlacklist.Remove(def.defName);
            }
        }
    }
}
