using Sims3.SimIFace;

namespace NRaas.PregnancyControllerSpace
{
    [Persistable]
    public class PersistedSettings
    {
    	[Tunable]
    	public static bool kUseFatnessForMalePregnancy = true;
    	[Tunable]
    	public static bool kKeepInactiveAlienBaby = false;
    	
    	public bool mUseFatnessForMalePregnancy = kUseFatnessForMalePregnancy;
    	
    	public bool mKeepInactiveAlienBaby = kKeepInactiveAlienBaby;
    }
}