using NRaas.CommonSpace.Options;
using NRaas.CommonSpace.Selection;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.GameEntry;
using System;
using System.Collections.Generic;

namespace NRaas.TravelerSpace.Helpers
{
    public class EditTownPuckEx
    {
        public static void ReplaceItems()
        {
            EditTownPuck This;
            while ((This = EditTownPuck.Instance) == null)
            {
                Common.Sleep();
            }
            This.mReturnToLiveButton.Visible = false;
            This.mReturnToLiveButton.Dispose();
            This.mReturnToLiveButton = (This.GetChildByID(1404022080u, true) as Button);
            This.mReturnToLiveButton.Click += new UIEventHandler<UIButtonClickEventArgs>(OnReturnToLive);
            This.mReturnToLiveButton.Visible = true;

            Dictionary<WorldName, string> worlds = new Dictionary<WorldName, string>();
            WorldData.GetWorlds(worlds);

            List<WorldName> availableLocations = new List<WorldName>();
            int numItems = 0;
            foreach (WorldName world in worlds.Keys)
            {
                if (world == GameUtils.GetCurrentWorld()) continue;

                string saveFileName;
                if (WorldData.GetSaveFileName(world, out saveFileName, true))
                {
                    if (saveFileName == GameStates.GetCurrentWorldName(true)) continue;

                    if (GameStates.sEditOtherWorldData != null)
                    {
                        if (saveFileName == GameStates.sEditOtherWorldData.mHomeWorldName) continue;
                    }
                    else if (GameStates.sTravelData != null)
                    {
                        if (saveFileName == GameStates.sTravelData.mHomeWorld) continue;
                    }
                }

                if (LoadingScreenControllerEx.sVacationWorldNames.Contains(world))
                {
                    numItems++;
                }
                else
                {
                    if (Traveler.Settings.GetHiddenWorlds(world)) continue;
                }
                availableLocations.Add(world);
            }
            numItems = availableLocations.Count > numItems ? 1 : 0;

            WindowBase childByID = This.GetChildByID(1404022083u, true);
            childByID.Visible = false;
            ItemGrid grid = This.GetChildByID(1404022084u, true) as ItemGrid;
            grid.Clear();

            ResourceKey layoutKey = ResourceKey.CreateUILayoutKey("LocationGridItem", 0u);
            if (!This.mModel.IsPlaceLotsWizardFlow)
            {
                if ((GameStates.sTravelData != null && GameStates.GetCurrentWorldName(true) != GameStates.sTravelData.mHomeWorld)
                    || (GameStates.sEditOtherWorldData != null && GameStates.GetCurrentWorldName(true) != GameStates.sEditOtherWorldData.mHomeWorldName))
                {
                    AddGridItem(grid, layoutKey, WorldName.Undefined);
                    numItems++;
                }
                if (availableLocations.Count > 0)
                {
                    foreach (WorldName current in availableLocations.ToArray())
                    {
                        if (LoadingScreenControllerEx.sVacationWorldNames.Contains(current) && numItems < 6)
                        {
                            AddGridItem(grid, layoutKey, current);
                            numItems++;
                            availableLocations.Remove(current);
                        }
                    }
                    if (availableLocations.Count > 0)
                    {
                        AddDefaultGridItem(grid, layoutKey, availableLocations);
                    }
                }
                if (numItems > 0)
                {
                    childByID.Visible = true;
                }
            }
        }

        private static void OnReturnToLive(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            EditTownPuck This = EditTownPuck.Instance;
            if (This == null) return;

            eventArgs.Handled = true;
            if (!This.mExitingGameEntry)
            {
                This.mExitingGameEntry = true;
                if (This.mIsInPloppablesMode)
                {
                    This.ExitNeighborhoodPloppablesMode(false);
                }
                Common.FunctionTask.Perform(ReturnToLive);
            }
        }

        private static void ReturnToLive()
        {
            EditTownPuck This = EditTownPuck.Instance;
            if (This == null) return;

            if (Responder.Instance.HudModel != null && Responder.Instance.OptionsModel != null && !Responder.Instance.OptionsModel.SaveGameInProgress && Responder.Instance.HudModel.IsGameEntryState())
            {
                if (GameUtils.IsOnVacation() && !This.mModel.IsAnyLotBaseCampEP1())
                {
                    ILocalizationModel localizationModel = Responder.Instance.LocalizationModel;
                    string titleText = localizationModel.LocalizeString("Ui/Caption/Global:Failed", new object[0]);
                    string messageText = localizationModel.LocalizeString("Ui/Caption/GameEntry/EditTown/EP01:NeedBasecamp", new object[0]);
                    SimpleMessageDialog.Show(titleText, messageText, ModalDialog.PauseMode.PauseSimulator, new Vector2(-1f, -1f), "ui_error", "ui_hardwindow_close");
                }
                else
                {
                    if (GameUtils.IsFutureWorld() && !This.mModel.IsAnyLotBaseCampFutureEP11())
                    {
                        ILocalizationModel localizationModel2 = Responder.Instance.LocalizationModel;
                        string titleText2 = localizationModel2.LocalizeString("Ui/Caption/Global:Failed", new object[0]);
                        string messageText2 = localizationModel2.LocalizeString("Ui/Caption/GameEntry/EditTown/EP11:NeedBasecampFuture", new object[0]);
                        SimpleMessageDialog.Show(titleText2, messageText2, ModalDialog.PauseMode.PauseSimulator, new Vector2(-1f, -1f), "ui_error", "ui_hardwindow_close");
                    }
                    else
                    {
                        if (!This.mModel.IsPlaceLotsWizardFlow || AcceptCancelDialog.Show(Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/GameEntry/PlaceEPLotsWizard:CancelPrompt", new object[0])))
                        {
                            This.HidePanels();
                            This.UpdateBackButton(true);
                            if (!EditTownModelEx.ExitEditTown(Responder.Instance.EditTownModel as EditTownModel, false))
                            {
                                This.UpdateBackButton(false);
                            }
                            else
                            {
                                This.mModel.IsPlaceLotsWizardFlow = false;
                            }
                        }
                    }
                }
            }
            This.mExitingGameEntry = false;
        }

        private static void OnGridItemMouseDown(WindowBase sender, UIMouseEventArgs eventArgs)
        {
            EditTownPuck This = EditTownPuck.Instance;
            if (This == null) return;

            if (!This.mExitingGameEntry)
            {
                This.mExitingGameEntry = true;
                Simulator.AddObject(new OneShotFunctionWithParams(new FunctionWithParam(GotoWorldTask), sender));
            }
        }

        private static void GotoWorldTask(object inObject)
        {
            EditTownPuck This = EditTownPuck.Instance;
            if (This == null) return;

            WindowBase windowBase = inObject as WindowBase;
            if (Responder.Instance.HudModel != null && Responder.Instance.OptionsModel != null && windowBase != null && !Responder.Instance.OptionsModel.SaveGameInProgress && Responder.Instance.HudModel.IsGameEntryState())
            {
                if (GameUtils.IsOnVacation() && !This.mModel.IsAnyLotBaseCampEP1())
                {
                    ILocalizationModel localizationModel = Responder.Instance.LocalizationModel;
                    string titleText = localizationModel.LocalizeString("Ui/Caption/Global:Failed", new object[0]);
                    string messageText = localizationModel.LocalizeString("Ui/Caption/GameEntry/EditTown/EP01:NeedBasecamp", new object[0]);
                    SimpleMessageDialog.Show(titleText, messageText, ModalDialog.PauseMode.PauseSimulator, new Vector2(-1f, -1f), "ui_error", "ui_hardwindow_close");
                }
                else
                {
                    if (GameUtils.IsFutureWorld() && !This.mModel.IsAnyLotBaseCampFutureEP11())
                    {
                        ILocalizationModel localizationModel2 = Responder.Instance.LocalizationModel;
                        string titleText2 = localizationModel2.LocalizeString("Ui/Caption/Global:Failed", new object[0]);
                        string messageText2 = localizationModel2.LocalizeString("Ui/Caption/GameEntry/EditTown/EP11:NeedBasecampFuture", new object[0]);
                        SimpleMessageDialog.Show(titleText2, messageText2, ModalDialog.PauseMode.PauseSimulator, new Vector2(-1f, -1f), "ui_error", "ui_hardwindow_close");
                    }
                    else
                    {
                        This.HidePanels();
                        This.UpdateBackButton(true);
                        WorldName world = WorldName.Undefined;
                        string worldName = null;
                        ICollection<WorldName> worlds = windowBase.Tag as ICollection<WorldName>;
                        if (worlds != null)
                        {
                            List<WorldItem> options = new List<WorldItem>();
                            foreach (WorldName value in worlds)
                            {
                                ResourceKey iconKey;
                                if (LoadingScreenControllerEx.sVacationWorldNames.Contains(value))
                                {
                                    iconKey = ResourceKey.CreatePNGKey(Responder.Instance.HudModel.LocationIconName(value), 0u);
                                }
                                else
                                {
                                    WorldType worldType = GameUtils.GetWorldType(value);
                                    switch (worldType)
                                    {
                                        case WorldType.Vacation:
                                            iconKey = ResourceKey.CreatePNGKey("glb_i_vacation", 0u);
                                            break;
                                        case WorldType.Downtown:
                                            iconKey = ResourceKey.CreatePNGKey("glb_i_downtown", 0u);
                                            break;
                                        case WorldType.University:
                                            iconKey = ResourceKey.CreatePNGKey("glb_i_university", 0u);
                                            break;
                                        case WorldType.Future:
                                            iconKey = ResourceKey.CreatePNGKey("hud_mt_i_future_world", 0u);
                                            break;
                                        default:
                                            if (value == WorldName.Undefined)
                                            {
                                                iconKey = ResourceKey.CreatePNGKey("glb_i_home", 0u);
                                            }
                                            else
                                            {
                                                iconKey = ResourceKey.CreatePNGKey("glb_i_suburb", 0u);
                                            }
                                            break;
                                    }
                                }
                                options.Add(new WorldItem(value, iconKey));
                            }

                            WorldItem selection = new CommonSelection<WorldItem>(Common.Localize("EditTownPuckEx:SelectTownCaption"), options).SelectSingle();
                            if (selection != null)
                            {
                                world = selection.Value;
                                worldName = selection.Name;
                            }
                        }
                        else
                        {
                            world = (WorldName)windowBase.Tag;
                            worldName = windowBase.TooltipText;
                        }

                        if (worldName != null && AcceptCancelDialog.Show(Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/EditTown/Puck:EditLocationPrompt", new object[]
                        {
                            worldName
                        })) && EditTownController.PromptForNonEmptyClipboard())
                        {
                            GameStatesEx.EditWorld(world, false);
                            LotManager.sWorldHasDiveLots = false;
                            foreach (Lot allLot in LotManager.AllLots)
                            {
                                allLot.CalculateMetaAutonomyTypeAndConsiderAddingToPublicMetaObjects();
                                allLot.UpdateVirtualResidentialSlots();
                                if (!LotManager.sWorldHasDiveLots && allLot.CommercialLotSubType == CommercialLotSubType.kEP10_Diving)
                                {
                                    LotManager.sWorldHasDiveLots = true;
                                }
                            }
                            if (!World.IsEditInGameFromWBMode())
                            {
                                Household.FindSuitableServiceAndTownieAccomodations();
                            }
                            if (Household.ActiveHousehold != null)
                            {
                                foreach (Sim allActor in Household.ActiveHousehold.AllActors)
                                {
                                    if (allActor.MapTagManager != null)
                                    {
                                        allActor.MapTagManager.Reset();
                                    }
                                }
                            }
                            LotManager.ForceReplanOfAllSimRoutes();
                        }
                        else
                        {
                            This.UpdateBackButton(false);
                        }
                    }
                }
            }
            This.mExitingGameEntry = false;
        }

        private static void AddGridItem(ItemGrid grid, ResourceKey layoutKey, WorldName world)
        {
            Window window = UIManager.LoadLayout(layoutKey).GetWindowByExportID(1) as Window;
            if (window != null)
            {
                grid.AddItem(new ItemGridCellItem(window, null));
                Window window2 = window.GetChildByID(2u, false) as Window;
                if (window2 != null)
                {
                    ImageDrawable imageDrawable = window2.Drawable as ImageDrawable;
                    string text = Responder.Instance.HudModel.LocationIconName(world);
                    if (imageDrawable != null && text != null)
                    {
                        imageDrawable.Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(text, 0u));
                    }
                    window.MouseDown += new UIEventHandler<UIMouseEventArgs>(OnGridItemMouseDown);
                    string value = Responder.Instance.HudModel.LocationName(world);
                    if (!string.IsNullOrEmpty(value))
                    {
                        window.TooltipText = Responder.Instance.HudModel.LocationName(world, true);
                    }
                    window.Tag = world;
                }
            }
        }

        private static void AddDefaultGridItem(ItemGrid grid, ResourceKey layoutKey, ICollection<WorldName> worlds)
        {
            Window window = UIManager.LoadLayout(layoutKey).GetWindowByExportID(1) as Window;
            if (window != null)
            {
                grid.AddItem(new ItemGridCellItem(window, null));
                Window window2 = window.GetChildByID(2u, false) as Window;
                if (window2 != null)
                {
                    ImageDrawable imageDrawable = window2.Drawable as ImageDrawable;
                    if (imageDrawable != null)
                    {
                        imageDrawable.Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey("glb_i_other", 0u));
                    }
                    window.MouseDown += new UIEventHandler<UIMouseEventArgs>(OnGridItemMouseDown);
                    window.TooltipText = Common.Localize("EditTownPuckEx:DefaultGridItemTooltipText");
                    window.Tag = worlds;
                }
            }
        }

        private class WorldItem : InteractionOptionItem<IActor, GameObject, GameHitParameters<GameObject>>, ICloseDialogOption
        {
            protected WorldName mValue;

            public WorldItem()
            { }

            public WorldItem(WorldName value, ResourceKey iconKey)
                : base(LoadingScreenControllerEx.sVacationWorldNames.Contains(value) ? Responder.Instance.HudModel.LocationName(value, true) : WorldData.GetLocationName(value), 0, iconKey)
            {
                mValue = value;
            }

            public override string Name
            {
                get { return mName; }
            }

            public virtual WorldName Value
            {
                get
                {
                    return mValue;
                }
            }

            public override string DisplayValue
            {
                get { return string.Empty; }
            }

            public override string GetTitlePrefix()
            {
                return "WorldItem";
            }

            protected override OptionResult Run(GameHitParameters<GameObject> parameters)
            {
                return OptionResult.SuccessClose;
            }
        }
    }
}
