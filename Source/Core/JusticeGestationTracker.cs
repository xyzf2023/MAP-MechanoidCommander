using Verse;

namespace MAP_MechCommander
{
    public class JusticeGestationTracker : GameComponent
    {
        private bool letterSent;

        public JusticeGestationTracker(Game game)
        {
        }

        public bool LetterSent => letterSent;

        public void MarkSent()
        {
            letterSent = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref letterSent, "justiceGestationLetterSent", false);
        }
    }
}
