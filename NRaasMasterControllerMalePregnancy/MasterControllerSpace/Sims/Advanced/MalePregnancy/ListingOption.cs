using NRaas.CommonSpace.Options;
using System;

namespace NRaas.MasterControllerSpace.Sims.Advanced.MalePregnancy
{
    public class ListingOption : OptionList<IPregnancyOption>, IAdvancedOption
    {
        public override string GetTitlePrefix()
        {
            return "MalePregnancyInteraction";
        }

        public override ITitlePrefixOption ParentListingOption
        {
            get { return null; }
        }
    }
}
