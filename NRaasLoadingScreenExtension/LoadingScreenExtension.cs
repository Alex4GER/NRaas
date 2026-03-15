using Sims3.SimIFace;
using NRaas.CommonSpace.Booters;

namespace NRaas
{
    public class LoadingScreenExtension : Common
    {
        [Tunable, TunableComment("Scripting Mod Instantiator, value does not matter, only its existence")]
        protected static bool kInstantiator = false;

        static LoadingScreenExtension()
        {
            sEnableLoadLog = true;
            Bootstrap();

            BooterHelper.Add(new ScoringBooter("MethodFile", "NRaas.LoadingScreenExtensionModule", false));
        }

        public LoadingScreenExtension()
        { }
    }
}
