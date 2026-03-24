using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

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

            if (me.IsHuman && me.IsMale && me.IsVisuallyPregnant)
            {
                if (me.CreatedSim.BuffManager.HasElement(BuffNames.MalePregnancy))
                {
                    BuffInstance buffInstance = me.CreatedSim.BuffManager.GetElement(BuffNames.MalePregnancy);
                    return buffInstance.TimeoutCount > 1f;
                }
                else if (me.IsWearingMaternityOutfit())
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

            if (me.IsVisuallyPregnant)
            {
                if (me.CreatedSim.BuffManager.HasElement(BuffNames.MalePregnancy))
                {
                    me.CreatedSim.BuffManager.RemoveElement(BuffNames.MalePregnancy);
                }
                me.SetPregnancy(0.0f);
                Common.Notify(Common.Localize(GetTitlePrefix() + ":Success", me.IsFemale, new object[] { me }));
            }
            return true;
        }
    }
}
