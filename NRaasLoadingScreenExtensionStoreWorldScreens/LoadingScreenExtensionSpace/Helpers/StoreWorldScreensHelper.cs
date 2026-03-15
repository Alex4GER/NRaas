using System.Collections.Generic;

namespace NRaas.LoadingScreenExtensionSpace.Helpers
{
    public class StoreWorldScreensHelper : Common.IStartupApp
    {
        private static Dictionary<string, string> sStoreWorldLoadingScreens = new Dictionary<string, string>
        {
			{
				"sunset valley",
				"basegame_world_loading"
			},
			{
				"riverview",
				"world_loading_riverview"
			},
			{
				"barnacle bay",
				"world_loading_bb"
			},
			{
				"hidden springs",
				"world_loading_hs"
			},
			{
				"lunar lakes",
				"world_loading_ll"
			},
			{
				"lucky palms",
				"world_loading_lp"
			},
			{
				"sunlit tides",
				"world_loading_st"
			},
			{
				"monte vista",
				"world_loading_mv"
			},
			{
				"aurora skies",
				"world_loading_as"
			},
			{
				"dragon valley",
				"world_loading_dv"
			},
			{
				"midnight hollow",
				"world_loading_mh"
			},
			{
				"roaring heights",
				"world_loading_rh"
			}
		};

		public void OnStartupApp()
        {
			WorldLoadingScreenHelper.ParseCustomData(sStoreWorldLoadingScreens);
			sStoreWorldLoadingScreens.Clear();
        }
    }
}
