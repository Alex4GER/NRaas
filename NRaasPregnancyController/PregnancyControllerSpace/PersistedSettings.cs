using Sims3.SimIFace;

namespace NRaas.PregnancyControllerSpace
{
    [Persistable]
    public class PersistedSettings
    {
    	[Tunable]
    	public static bool kUseFatnessForMalePregnancy;
    	
    	public bool mUseFatnessForMalePregnancy = kUseFatnessForMalePregnancy;
    }
}