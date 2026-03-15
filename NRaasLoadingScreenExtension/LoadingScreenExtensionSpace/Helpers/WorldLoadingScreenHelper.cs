using NRaas.CommonSpace.Tasks;
using System;
using System.Collections.Generic;
using Sims3.Gameplay.Utilities;
using Sims3.UI;
using Sims3.UI.GameEntry;
using Sims3.Gameplay;
using Sims3.SimIFace;

namespace NRaas.LoadingScreenExtensionSpace.Helpers
{
	public class WorldLoadingScreenHelper : Common.IStartupApp
	{
		public delegate void ScreenSearchTaskDelegate(ref string text);
		public delegate void ScreenHandleTaskDelegate(LoadingScreenController controller);

		private static readonly Dictionary<string, string> sLoadingScreenDictionary = new Dictionary<string, string>();
		private static string sWorldName = null;
		private static UIImage sScreenImage = null;
		private static GameStates.EditOtherWorldData.EditOtherWorldState sLastEditOtherWorldState;

		private static ScreenSearchTaskDelegate sInMainMenuStateEventHandler;
		private static ScreenSearchTaskDelegate sEditingOtherTownEventHandler;
		private static ScreenSearchTaskDelegate sEditTownEventHandler;
		private static ScreenSearchTaskDelegate sMovingWorldsEventHandler;
		private static ScreenSearchTaskDelegate sStartingVacationEventHandler;
		private static ScreenSearchTaskDelegate sOnVacationEventHandler;

		private static ScreenHandleTaskDelegate sLoadingScreenInstanceCreatedEventHandler;
		private static ScreenHandleTaskDelegate sLoadingScreenInstanceDisposedEventHandler;

		public static event ScreenSearchTaskDelegate InMainMenuState
		{
			add { sInMainMenuStateEventHandler += value; }
			remove { sInMainMenuStateEventHandler -= value; }
		}

		public static event ScreenSearchTaskDelegate EditingOtherTown
		{
			add { sEditingOtherTownEventHandler += value; }
			remove { sEditingOtherTownEventHandler -= value; }
		}

		public static event ScreenSearchTaskDelegate EditTown
		{
			add { sEditTownEventHandler += value; }
			remove { sEditTownEventHandler -= value; }
		}

		public static event ScreenSearchTaskDelegate MovingWorlds
		{
			add { sMovingWorldsEventHandler += value; }
			remove { sMovingWorldsEventHandler -= value; }
		}

		public static event ScreenSearchTaskDelegate StartingVacation
		{
			add { sStartingVacationEventHandler += value; }
			remove { sStartingVacationEventHandler -= value; }
		}

		public static event ScreenSearchTaskDelegate OnVacation
		{
			add { sOnVacationEventHandler += value; }
			remove { sOnVacationEventHandler -= value; }
		}

		public static event ScreenHandleTaskDelegate LoadingScreenInstanceCreated
		{
			add { sLoadingScreenInstanceCreatedEventHandler += value; }
			remove { sLoadingScreenInstanceCreatedEventHandler -= value; }
		}
		
		public static event ScreenHandleTaskDelegate LoadingScreenInstanceDisposed
		{
			add { sLoadingScreenInstanceDisposedEventHandler += value; }
			remove { sLoadingScreenInstanceDisposedEventHandler -= value; }
		}

		public void OnStartupApp()
		{
			ParseCustomData("WorldLoadingScreens");

			sInMainMenuStateEventHandler += OnInMainMenuState;
			sEditingOtherTownEventHandler += OnEditingOtherTown;
			sEditTownEventHandler += OnEditTown;
			sMovingWorldsEventHandler += OnMovingWorlds;
			sOnVacationEventHandler += OnOnVacation;
			sLoadingScreenInstanceCreatedEventHandler += OnLoadingScreenInstanceCreated;
			sLoadingScreenInstanceDisposedEventHandler += OnLoadingScreenInstanceDisposed;

			RepeatingTask.Create<ScreenSearchTask>();
			RepeatingTask.Create<ScreenHandleTask>();
		}

		private static void OnInMainMenuState(ref string worldName)
		{
			WindowBase windowBase = UIManager.GetWindowFromPoint(UIManager.GetCursorPosition());
			MainMenu mainMenu;
			while (true)
			{
				if (GameStates.IsInMainMenuState)
				{
					mainMenu = (windowBase as MainMenu);
					if (mainMenu != null)
					{
						break;
					}

					try
					{
						windowBase = UIManager.GetParentWindow(windowBase);
						if (windowBase == null)
						{
							throw new NullReferenceException();
						}
					}
					catch (NullReferenceException)
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			if (mainMenu.mbSavedGameMode && mainMenu.mSaveItem != null)
			{
				worldName = mainMenu.mSaveItem.mWorldFile.ToLower();
			}
			else
			{
				if (!mainMenu.mbSavedGameMode && mainMenu.mWorldFile != null)
				{
					worldName = mainMenu.mWorldFile.mWorldFile.ToLower();
					worldName = worldName.Remove(worldName.Length - 6);
				}
			}
		}

		private static void OnEditingOtherTown(ref string worldName)
		{
			GameStates.EditOtherWorldData.EditOtherWorldState mState = GameStates.sEditOtherWorldData.mState;
			if (mState != GameStates.EditOtherWorldData.EditOtherWorldState.EditOtherWorld && sLastEditOtherWorldState != mState)
			{
				worldName = (mState == GameStates.EditOtherWorldData.EditOtherWorldState.EditHomeWorld) ? GameStates.sEditOtherWorldData.mHomeWorldName : GameStates.sEditOtherWorldData.mWorldIStartedEditingInName;
				worldName = worldName.ToLower().Remove(worldName.Length - 11);
				sLastEditOtherWorldState = GameStates.sEditOtherWorldData.mState;
			}
		}

		private static void OnEditTown(ref string worldName)
		{
			if (sScreenImage != null)
			{
				ResetStatics();
			}
		}

		private static void OnMovingWorlds(ref string worldName)
		{
			worldName = GameStates.MovingWorldName.ToLower();
			worldName = worldName.Remove(worldName.Length - 6);
		}

		private static void OnOnVacation(ref string worldName)
		{
			worldName = GameStates.sTravelData.mHomeWorld.ToLower();
			worldName = worldName.Remove(worldName.Length - 11);
		}

		private static void OnLoadingScreenInstanceCreated(LoadingScreenController controller)
		{
			ReplaceScreen(controller);
		}
		private static void OnLoadingScreenInstanceDisposed(LoadingScreenController controller)
		{
			ResetStatics();
		}

		public static void ParseCustomData(string xmlDbResourcename)
		{
			XmlDbData xmlDbData = XmlDbData.ReadData(xmlDbResourcename);
			if (xmlDbData == null)
			{
				return;
			}
			XmlDbTable xmlDbTable;
			xmlDbData.Tables.TryGetValue("World", out xmlDbTable);
			if (xmlDbTable != null)
			{
				foreach (XmlDbRow row in xmlDbTable.Rows)
				{
					string key = row.GetString("WorldName").ToLower();
					string @string = row.GetString("LoadingScreenResourceName");
					try
					{
						sLoadingScreenDictionary.Add(key, @string);
					}
					catch (ArgumentException e)
					{
						Common.Exception("Another entry for " + key + " was found in " + xmlDbResourcename + ".xml. The mod will use the first entry for this world.", e);
						continue;
					}
					catch (Exception)
					{
						throw;
					}
				}
			}
		}

		public static void ParseCustomData(Dictionary<string, string> screenDataDictionary)
		{
			foreach (KeyValuePair<string, string> screenData in screenDataDictionary)
			{
				try
				{
					sLoadingScreenDictionary.Add(screenData.Key, screenData.Value);
				}
				catch (ArgumentException)
				{
					continue;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		public static void ReplaceScreen(LoadingScreenController controller, UIImage screenImage = null)
		{
			if (LoadingScreenController.sChosenLoadScreen != -1) return;

			if (screenImage != null)
			{
				(controller.Drawable as ImageDrawable).Image = screenImage;
				controller.Invalidate();
			}
			else
			{
				if (sScreenImage != null)
				{
					(controller.Drawable as ImageDrawable).Image = sScreenImage;
					controller.Invalidate();
				}
			}
		}

		private static void ResetStatics()
		{
			sWorldName = null;
			sScreenImage = null;
		}

		private static void GetScreenImage(string worldName)
		{
			if (string.IsNullOrEmpty(worldName)) return;

			if (!Equals(worldName, sWorldName))
			{
				sWorldName = worldName;
				string imageResourceName = string.Empty;
				if (sLoadingScreenDictionary.TryGetValue(sWorldName, out imageResourceName))
				{
					sScreenImage = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(imageResourceName, 0u));
				}
				else
				{
					sScreenImage = null;
				}
			}
		}

		public class ScreenSearchTask : RepeatingTask
		{
			protected override int Delay
			{
				get { return 1; }
			}
			protected override bool OnPerform()
			{
				if (!ScreenHandleTask.LoadingScreenActive)
				{
					string worldName = string.Empty;
					if (GameStates.IsInMainMenuState)
					{
						if (sInMainMenuStateEventHandler != null)
						{
							WorldLoadingScreenHelper.sInMainMenuStateEventHandler(ref worldName);
						}
					}
					else
					{
						if (GameStates.IsEditingOtherTown)
						{
							if (sEditingOtherTownEventHandler != null)
							{
								WorldLoadingScreenHelper.sEditingOtherTownEventHandler(ref worldName);
							}
						}
						else
						{
							if (GameStates.IsEditTownState)
							{
								if (sEditTownEventHandler != null)
								{
									WorldLoadingScreenHelper.sEditTownEventHandler(ref worldName);
								}
							}
							else
							{
								if (GameStates.IsMovingWorlds)
								{
									if (sMovingWorldsEventHandler != null)
									{
										WorldLoadingScreenHelper.sMovingWorldsEventHandler(ref worldName);
									}
								}
								else
								{
									if (GameStates.IsStartingVacation)
									{
										if (sStartingVacationEventHandler != null)
										{
											WorldLoadingScreenHelper.sStartingVacationEventHandler(ref worldName);
										}
									}
									else
									{
										if (GameStates.IsOnVacation)
										{
											if (sOnVacationEventHandler != null)
											{
												WorldLoadingScreenHelper.sOnVacationEventHandler(ref worldName);
											}
										}
									}
								}
							}
						}
					}
					WorldLoadingScreenHelper.GetScreenImage(worldName);
				}
				return true;
			}
		}

		public class ScreenHandleTask : RepeatingTask
		{
			private static bool sLoadingScreenActive = false;

			public static bool LoadingScreenActive
			{
				get { return sLoadingScreenActive; }
			}
			protected override int Delay
			{
				get { return 0; }
			}
			protected override bool OnPerform()
			{
				LoadingScreenController controller = LoadingScreenController.Instance;
				if (controller != null)
				{
					if (!sLoadingScreenActive)
					{
						sLoadingScreenActive = true;

						if (sLoadingScreenInstanceCreatedEventHandler != null)
						{
							WorldLoadingScreenHelper.sLoadingScreenInstanceCreatedEventHandler(controller);
						}
					}
				}
				else
				{
					if (sLoadingScreenActive)
					{
						try
						{
							if (sLoadingScreenInstanceDisposedEventHandler != null)
							{
								WorldLoadingScreenHelper.sLoadingScreenInstanceDisposedEventHandler(controller);
							}
						}
						finally
						{
							sLoadingScreenActive = false;
						}
					}
				}
				return true;
			}
		}
	}
}
