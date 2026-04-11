using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using System;

namespace NRaas.MasterControllerSpace.Sims.Advanced.MalePregnancy
{
    public class ResumePregnancy : SimFromList, IPregnancyOption
    {
        public override string GetTitlePrefix()
        {
            return "ResumeMalePregnancy";
        }

        protected override int GetMaxSelection()
        {
            return 0;
        }

        protected override bool CanApplyAll()
        {
            return true;
        }

        protected override bool PrivateAllow(SimDescription me)
        {
            if (!base.PrivateAllow(me)) return false;

            if (me.CreatedSim == null) return false;

            if (me.IsPregnant) return false;

            if (me.IsHuman && me.IsMale)
            {
                if (me.CreatedSim.BuffManager.HasElement(BuffNames.MalePregnancy))
                {
                    BuffInstance buffInstance = me.CreatedSim.BuffManager.GetElement(BuffNames.MalePregnancy);
                    return buffInstance.mTimeoutPaused && buffInstance.TimeoutCount > 1f;
                }
                if (me.IsVisuallyPregnant)
                {
                    return GameUtils.IsInstalled(ProductVersion.EP8);
                }
            }

            return false;
        }

        protected override bool Run(SimDescription me, bool singleSelection)
        {
            if (me.CreatedSim.BuffManager.HasElement(BuffNames.MalePregnancy))
            {
                me.CreatedSim.BuffManager.UnpauseBuff(BuffNames.MalePregnancy);
            }
            else
            {
                me.CreatedSim.BuffManager.AddElement(BuffNames.MalePregnancy, Origin.FromPregnancy);
            }

            return true;
        }
    }
}
