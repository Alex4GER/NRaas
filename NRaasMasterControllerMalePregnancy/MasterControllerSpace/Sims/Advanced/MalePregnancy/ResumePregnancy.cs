using NRaas.MasterControllerSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

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

            if (me.IsHuman && me.IsMale && me.IsVisuallyPregnant)
            {
                if (me.CreatedSim.BuffManager.HasElement(BuffNames.MalePregnancy))
                {
                    BuffInstance buffInstance = me.CreatedSim.BuffManager.GetElement(BuffNames.MalePregnancy);
                    return buffInstance.mTimeoutPaused && buffInstance.TimeoutCount > 1f;
                }
                else if (me.IsWearingMaternityOutfit())
                {
                	return GameUtils.IsInstalled(ProductVersion.EP8);
                }
            }

            return false;
        }

        protected override bool Run(SimDescription me, bool singleSelection)
        {
            if (me.IsVisuallyPregnant)
            {
                if (me.CreatedSim.BuffManager.HasElement(BuffNames.MalePregnancy))
                {
                    me.CreatedSim.BuffManager.UnpauseBuff(BuffNames.MalePregnancy);
                }
                else
                {
                    BuffMalePregnancyEx.AddBuff(me.CreatedSim, Origin.FromPregnancy);
                }
            }

            return true;
        }
    }
}
