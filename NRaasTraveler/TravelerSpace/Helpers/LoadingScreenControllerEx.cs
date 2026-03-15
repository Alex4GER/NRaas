using System.Collections.Generic;
using Sims3.SimIFace;
using Sims3.UI;

namespace NRaas.TravelerSpace.Helpers
{
    public class LoadingScreenControllerEx
    {
        /* Contains the names of the worlds which do not need any special handling. */
        public static readonly List<WorldName> sVacationWorldNames = new List<WorldName>
        {
            WorldName.China,
            WorldName.Egypt,
            WorldName.France,
            WorldName.University,
            WorldName.FutureWorld
        };
        /* Traveler will use this function to create a travel loadscreen instead of the EA-default. */
        public static void LoadTravellingLoadingScreen(string travelWorldName, WorldName worldName, bool travellingHome, bool isFirstTimeTravelingToFuture)
        {
            if (!LoadingScreenController.IsLayoutLoaded())
            {
                LoadingScreenController.sTravelWorldName = travelWorldName;
                LoadingScreenController.sbLoadingSaveGame = false;
                LoadingScreenController.sWorldName = worldName;
                LoadingScreenController.sbTravellingHome = travellingHome;
                LoadingScreenController.ResetStatics();
                LoadingScreenController.sFirstTimeTravelingToFuture = isFirstTimeTravelingToFuture;
                if ((worldName == WorldName.China || worldName == WorldName.Egypt || worldName == WorldName.France) && !travellingHome)
                {
                    LoadingScreenController.sOverrideGameTipsFilename = "GameTipsEP1" + worldName.ToString();
                }
                else
                {
                    if (worldName == WorldName.University && !travellingHome)
                    {
                        LoadingScreenController.sOverrideGameTipsFilename = "GameTipsEP9" + worldName.ToString();
                    }
                    else
                    {
                        if (worldName == WorldName.FutureWorld && !travellingHome)
                        {
                            LoadingScreenController.sOverrideGameTipsFilename = "GameTipsEP11" + worldName.ToString();
                        }
                        else
                        {
                            LoadingScreenController.sOverrideGameTipsFilename = string.Empty;
                        }
                    }
                }
                if (travellingHome || !sVacationWorldNames.Contains(worldName))
                {   /* Play the standard loadloop audio when travelling home or to any world, which has not a specific loadloop. */
                    Responder.Instance.HudModel.PlayLoadLoopAudio(WorldName.Undefined);
                }
                else
                {   /* Play specific loadloop when travelling to China, Egypt, France, University or FutureWorld. */
                    Responder.Instance.HudModel.PlayLoadLoopAudio(worldName);
                }
                if (!LoadingScreenController.CreateInteractiveLoadingScreenIfEnabled() && LoadingScreenController.sLayout == null)
                {
                    ResourceKey resKey = ResourceKey.CreateUILayoutKey("TravelLoadingScreen", 0u);
                    LoadingScreenController.sLayout = UIManager.LoadLayoutAndAddToWindow(resKey, UICategory.Tooltips);
                }
            }
        }
    }
}
