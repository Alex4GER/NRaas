using NRaas.MasterControllerSpace.Helpers;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.UI;
using System;

namespace NRaas.MasterControllerSpace.Sims.Advanced.MalePregnancy
{
    public class DiscontinuePregnancy : SimFromList, IPregnancyOption
    {
        public override string GetTitlePrefix()
        {
            return "DiscontinueMalePregnancy";
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
                    return buffInstance.TimeoutCount > 1f;
                }
                if (me.IsVisuallyPregnant)
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool Run(SimDescription me, bool singleSelection)
        {
            if (!ApplyAll)
            {
                if (!AcceptCancelDialog.Show(Common.Localize(GetTitlePrefix() + ":Prompt", me.IsFemale, new object[] { me })))
                {
                    return false;
                }
            }

            if (me.CreatedSim.BuffManager.HasElement(BuffNames.MalePregnancy))
            {
                me.CreatedSim.BuffManager.RemoveElement(BuffNames.MalePregnancy);
            }
            if (me.IsVisuallyPregnant)
            {
                BuffMalePregnancyEx.SetPregnancy(me, 0f);
            }
            Common.Notify(Common.Localize(GetTitlePrefix() + ":Success", me.IsFemale, new object[] { me }));
            return true;
        }
    }
}
