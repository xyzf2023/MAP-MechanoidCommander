using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    public class CompProperties_CommanderSkills : CompProperties
    {
        public BodyTypeDef? bodyType;
        public List<SkillLevelEntry> skillLevels = new List<SkillLevelEntry>();

        public CompProperties_CommanderSkills()
        {
            compClass = typeof(CompCommanderSkills);
        }
    }

    public class SkillLevelEntry
    {
        public SkillDef? skill;
        public int level = 0;
        public Passion passion = Passion.None;
    }
}
