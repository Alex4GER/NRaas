using Sims3.SimIFace;

namespace NRaas
{
    public class PregnancyController : Common
    {
        [Tunable, TunableComment("Scripting Mod Instantiator, value does not matter, only its existence")]
        protected static bool kInstantiator = false;

        static PregnancyController()
        {
            Bootstrap();
        }
    }
}