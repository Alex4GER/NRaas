using Sims3.SimIFace;

namespace NRaas
{
    public class LoadingScreenExtensionModule
    {
        [Tunable, TunableComment("Scripting Mod Instantiator, value does not matter, only its existence")]
        protected static bool kInstantiator = false;

        static LoadingScreenExtensionModule()
        { }

        public LoadingScreenExtensionModule()
        { }
    }
}
