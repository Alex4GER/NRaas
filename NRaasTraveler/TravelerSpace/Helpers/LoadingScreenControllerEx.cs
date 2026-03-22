using System;
using System.Collections.Generic;
using Sims3.Gameplay;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using Sims3.UI;

namespace NRaas.TravelerSpace.Helpers
{
	public class LoadingScreenControllerEx
	{
		public enum LoadingImageType : uint
		{
			None = 0u,
			Standard = 1u,
			LastFocusedLot = 2u,
			LastActiveHousehold = 4u
		}

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
				
				try
				{
					LoadingScreenControllerEx.HandleScreen();
				}
				catch (Exception exception)
				{
					Common.Exception("", exception);
				}
			}
		}
		
		public static void HandleScreen()
		{
			while (LoadingScreenController.Instance == null && !LoadingScreenController.IsLayoutLoaded())
			{
				Common.Sleep();
			}

			if (LoadingScreenController.sChosenLoadScreen != -1)
			{
				return;
			}

			string worldNameStr;
			string lastActiveLotName = string.Empty;
			string lastFocusedLotName = string.Empty;
			bool replaceTravelingScreen = false;
			
			if (GameStates.IsEditingOtherTown)
			{
				GameStates.EditOtherWorldData.EditOtherWorldState mState = GameStates.sEditOtherWorldData.mState;
				worldNameStr = (mState == GameStates.EditOtherWorldData.EditOtherWorldState.EditHomeWorld) ? GameStates.sEditOtherWorldData.mHomeWorldName : GameStates.sEditOtherWorldData.mWorldIStartedEditingInName;
				worldNameStr = worldNameStr.Remove(worldNameStr.Length - 11);
			}
			else if (GameStates.IsTravelling)
			{
				worldNameStr = Responder.Instance.HudModel.LocationName(GameStates.DestinationTravelWorld, true);
				if (LoadingScreenController.sbTravellingHome && GameStates.sTravelData != null)
				{
					worldNameStr = GameStates.sTravelData.mHomeWorld;
					worldNameStr = worldNameStr.Remove(worldNameStr.Length - 11);
				}
				else if (!sVacationWorldNames.Contains(GameStates.DestinationTravelWorld))
				{
					worldNameStr = WorldData.GetLocationName(GameStates.DestinationTravelWorld);
					
					Text text = LoadingScreenController.sInstance.GetChildByID(116085280u, true) as Text;
					text.Caption = Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/TravelLoadingScreen:TravelingTo", new object[]
					{
						worldNameStr
					});
					replaceTravelingScreen = true;
				}
			}
			else if (LoadingScreenController.sbLoadingSaveGame)
			{
				worldNameStr = LoadingScreenController.sSaveGameMetadata.mWorldFile;
				if (!string.IsNullOrEmpty(Traveler.Settings.mLastActiveLot))
				{
					lastActiveLotName = Traveler.Settings.mLastActiveLot.ToLower().Replace(" ", "_");
				}
				if (!string.IsNullOrEmpty(Traveler.Settings.mLastFocusedLot))
				{
					lastFocusedLotName = Traveler.Settings.mLastFocusedLot.ToLower().Replace(" ", "_");
				}
			}
			else
			{
				worldNameStr = LoadingScreenController.sWorldFileMetadata.mWorldFile;
				worldNameStr = worldNameStr.Remove(worldNameStr.Length - 6);
			}

			WorldName worldName = LoadingScreenController.sWorldName;

			string screenImageResourceName = string.Empty;
			ProductVersion version = ProductVersion.BaseGame;

			if (worldName != WorldName.China)
			{
				if (worldName != WorldName.Egypt)
				{
					if (worldName != WorldName.France)
					{
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
											if (worldName != WorldName.University)
											{
												if (worldName != WorldName.IslaParadiso)
												{
													if (worldName == WorldName.FutureWorld)
													{
														screenImageResourceName = "world_loading_future";
														version = ProductVersion.EP11;
													}
												}
												else
												{
													screenImageResourceName = "ep10_world_loading_screen";
													version = ProductVersion.EP10;
												}
											}
											else
											{
												screenImageResourceName = "world_loading_university";
												version = ProductVersion.EP9;
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
					}
					else
					{
						screenImageResourceName = "world_loading_paris";
					}
				}
				else
				{
					screenImageResourceName = "world_loading_cairo";
				}
			}
			else
			{
				screenImageResourceName = "world_loading_beijing";
			}

			bool ignoreEAWorldScreen = !string.IsNullOrEmpty(screenImageResourceName) && !replaceTravelingScreen;

			if (string.IsNullOrEmpty(screenImageResourceName))
			{
				worldNameStr = worldNameStr.ToLower();

				if (worldNameStr != "sunset valley")
				{
					if (worldNameStr != "riverview")
					{
						if (worldNameStr != "barnacle bay")
						{
							if (worldNameStr != "hidden springs")
							{
								if (worldNameStr != "lunar lakes")
								{
									if (worldNameStr != "lucky palms")
									{
										if (worldNameStr != "sunlit tides")
										{
											if (worldNameStr != "monte vista")
											{
												if (worldNameStr != "aurora skies")
												{
													if (worldNameStr != "dragon valley")
													{
														if (worldNameStr != "midnight hollow")
														{
															if (worldNameStr != "roaring heights")
															{
																screenImageResourceName = "world_loading_" + worldNameStr.Replace(' ', '_');
															}
															else
															{
																screenImageResourceName = "world_loading_rh";
															}
														}
														else
														{
															screenImageResourceName = "world_loading_mh";
														}
													}
													else
													{
														screenImageResourceName = "world_loading_dv";
													}
												}
												else
												{
													screenImageResourceName = "world_loading_as";
												}
											}
											else
											{
												screenImageResourceName = "world_loading_mv";
											}
										}
										else
										{
											screenImageResourceName = "world_loading_st";
										}
									}
									else
									{
										screenImageResourceName = "world_loading_lp";
									}
								}
								else
								{
									screenImageResourceName = "world_loading_ll";
								}
							}
							else
							{
								screenImageResourceName = "world_loading_hs";
							}
						}
						else
						{
							screenImageResourceName = "world_loading_bb";
						}
					}
					else
					{
						screenImageResourceName = "world_loading_riverview";
					}
				}
				else
				{
					screenImageResourceName = "basegame_world_loading";
				}
			}

			List<UIImage> list_loadingImages = new List<UIImage>();
			List<UIImage> list_travelingImages = new List<UIImage>();
			UIImage lastFocusedLotImage = null;
			UIImage lastActiveLotImage = null;
			UIImage screenImage = null;

			for (int num = 0; num <= 12; num++)
			{
				if (num == 0 && ignoreEAWorldScreen)
				{
					list_loadingImages.Add(null);
					continue;
				}
				if (num < 6)
				{
					string resourceName = (num == 0) ? screenImageResourceName : (screenImageResourceName + num);
					UIImage image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(resourceName, ResourceUtils.ProductVersionToGroupId(version)));
					if (image != null)
					{
						list_loadingImages.Add(image);
					}
				}
				else if (num >= 6 && num < 11 && GameStates.IsTravelling)
				{
					string resourceName = screenImageResourceName.Replace("loading", "traveling");
					if ((num - 6) != 0)
					{
						resourceName += (num - 6);
					}
					UIImage image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(resourceName, ResourceUtils.ProductVersionToGroupId(version)));
					if (image != null)
					{
						list_travelingImages.Add(image);
					}
				}
				if (num == 11)
				{
					string resourceName = screenImageResourceName + "_" + lastFocusedLotName;
					lastFocusedLotImage = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(resourceName, ResourceUtils.ProductVersionToGroupId(version)));
				}
				if (num == 12)
				{
					string resourceName = screenImageResourceName + "_" + lastActiveLotName;
					lastActiveLotImage = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(resourceName, ResourceUtils.ProductVersionToGroupId(version)));
				}
			}

			if (Traveler.Settings.mLoadScreenImageType == LoadingImageType.LastFocusedLot && lastFocusedLotImage != null)
			{
				screenImage = lastFocusedLotImage;
			}
			else if (Traveler.Settings.mLoadScreenImageType == LoadingImageType.LastActiveHousehold && lastActiveLotImage != null)
			{
				screenImage = lastActiveLotImage;
			}

			if (screenImage == null)
			{
				if (list_travelingImages.Count > 0)
				{
					screenImage = RandomUtil.GetRandomObjectFromList(list_travelingImages);
				}
				else if (list_loadingImages.Count > 0)
				{
					screenImage = RandomUtil.GetRandomObjectFromList(list_loadingImages);
				}
			}
			
			if (screenImage != null)
			{
				(LoadingScreenController.Instance.Drawable as ImageDrawable).Image = screenImage;
				LoadingScreenController.Instance.Invalidate();
			}
		}
	}
}
