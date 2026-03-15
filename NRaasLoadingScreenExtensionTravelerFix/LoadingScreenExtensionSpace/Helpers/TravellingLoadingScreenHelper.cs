extern alias SP;

using WorldData = SP::NRaas.TravelerSpace.Helpers.WorldData;
using LoadingScreenControllerEx = SP::NRaas.TravelerSpace.Helpers.LoadingScreenControllerEx;
using Sims3.SimIFace;
using Sims3.Gameplay;
using Sims3.UI;

namespace NRaas.LoadingScreenExtensionSpace.Helpers
{
    public class TravellingLoadingScreenHelper : Common.IStartupApp
    {
        public void OnStartupApp()
        {
            WorldLoadingScreenHelper.StartingVacation += OnStartingVacation;
            WorldLoadingScreenHelper.LoadingScreenInstanceCreated += OnLoadingScreenInstanceCreated;
        }

        private static void OnStartingVacation(ref string text)
        {
            text = WorldData.GetLocationName(GameStates.DestinationTravelWorld);
            text = text.ToLower();
        }

        private static void OnLoadingScreenInstanceCreated(LoadingScreenController controller)
        {
            if (!GameStates.IsTravelling) return;
            if (LoadingScreenController.sbTravellingHome) return;

            WorldName worldName = GameStates.DestinationTravelWorld;

            if (LoadingScreenControllerEx.sVacationWorldNames.Contains(worldName)) return;

            FixCaption(WorldData.GetLocationName(worldName));
            FixLoadingScreenImage(controller, worldName);
        }

        private static void FixCaption(string worldLocationName)
        {
            if (LoadingScreenController.sChosenLoadScreen != -1) return;

            Text text = LoadingScreenController.sInstance.GetChildByID(116085280u, true) as Text;
            text.Caption = Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/TravelLoadingScreen:TravelingTo", new object[]
            {
                worldLocationName
            });
        }
        
        private static void FixLoadingScreenImage(LoadingScreenController controller, WorldName worldName)
        {
            string screenImageResourceName = string.Empty;
            ProductVersion version = ProductVersion.BaseGame;
            if (worldName != WorldName.TwinBrook)
            {
                if (worldName != WorldName.NewDowntownWorld)
                {
                    if (worldName != WorldName.AppaloosaPlains)
                    {
                        if (worldName != WorldName.StarlightShores)
                        {
                            if (worldName != WorldName.MoonlightFalls)
                            {
                                if (worldName == WorldName.IslaParadiso)
                                {
                                    screenImageResourceName = "ep10_world_loading_screen";
                                    version = ProductVersion.EP10;
                                }
                            }
                            else
                            {
                                screenImageResourceName = "world_loading_EP7World";
                                version = ProductVersion.EP7;
                            }
                        }
                        else
                        {
                            screenImageResourceName = "world_loading_EP6World";
                            version = ProductVersion.EP6;
                        }
                    }
                    else
                    {
                        screenImageResourceName = "ep5_world_loading_screen";
                        version = ProductVersion.EP5;
                    }
                }
                else
                {
                    screenImageResourceName = "world_loading_bridgeport";
                    version = ProductVersion.EP3;
                }
            }
            else
            {
                screenImageResourceName = "world_loading_twinbrook";
            }

            if (!string.IsNullOrEmpty(screenImageResourceName))
            {
                WorldLoadingScreenHelper.ReplaceScreen(controller,
                    UIManager.LoadUIImage(ResourceKey.CreatePNGKey(screenImageResourceName, ResourceUtils.ProductVersionToGroupId(version))));
            }
        }
    }
}
