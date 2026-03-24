using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.Options;
using NRaas.MasterControllerSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.DynamicChallenges;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Collections;
using Sims3.UI.CAS;

namespace NRaas.MasterControllerSpace.Sims.Basic
{
	public class StartMalePregnancy : SimFromList, IBasicOption
	{
		private sealed class ShowPregnancy : Interaction<Sim, Sim>
		{
			[DoesntRequireTuning]
			private sealed class Definition : InteractionDefinition<Sim, Sim, ShowPregnancy>
			{
				public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
				{
					return "NeverSeen";
				}

				public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
				{
					return !a.SimDescription.IsVisuallyPregnant;
				}
			}

			public static readonly InteractionDefinition Singleton = new Definition();

			public override bool Run()
			{
				SimDescription simDescription = Actor.SimDescription;
				if (simDescription.IsVisuallyPregnant)
				{
					return true;
				}
				simDescription.IsVisuallyPregnant = true;
				simDescription.mCurrentShape.Pregnant = 0.01f;

				OutfitCategories[] listOfCategories = simDescription.ListOfCategories;
				foreach (OutfitCategories outfitCategories in listOfCategories)
				{
					ArrayList arrayList = simDescription.mOutfits[outfitCategories] as ArrayList;
					if (simDescription.AllowOutfitCategoryForPregnancy(outfitCategories) && arrayList.Count > 0)
					{
						SimOutfit outfit = arrayList[0] as SimOutfit;
						BuildPregnantOutfit(outfit, outfitCategories);
						outfit = null;
					}
				}
				PutOnPregnantOutfit();
				simDescription.RefreshBodyShape();
				BuffMalePregnancyEx.AddBuff(Actor, Origin.FromPregnancy);
				return true;
			}

			private void MakePartsCategoryAppropriateForMaternity(SimBuilder builder, OutfitCategories category, SimDescriptionCore simDesc)
			{
				OutfitCategories outfitCategories = (OutfitCategories)OutfitCategoriesExtended.ValidForMaternity | category;
				foreach (BodyTypes value in Enum.GetValues(typeof(BodyTypes)))
				{
					switch (value)
					{
						case BodyTypes.Accessories:
						case BodyTypes.Armband:
						case BodyTypes.Bracelet:
						case BodyTypes.Earrings:
						case BodyTypes.FullBody:
						case BodyTypes.Glasses:
						case BodyTypes.Gloves:
						case BodyTypes.Hair:
						case BodyTypes.LeftEarring:
						case BodyTypes.LeftGarter:
						case BodyTypes.LowerBody:
						case BodyTypes.Necklace:
						case BodyTypes.RightEarring:
						case BodyTypes.RightGarter:
						case BodyTypes.Ring:
						case BodyTypes.Shoes:
						case BodyTypes.Socks:
						case BodyTypes.UpperBody:
							break;
						default:
							continue;
					}

					List<CASPart> wornParts = builder.GetWornParts(value);
					foreach (CASPart item in wornParts)
					{
						CASPart part;
						if (value == BodyTypes.Hair)
						{
							if ((item.CategoryFlags & (uint)OutfitCategoriesExtended.IsHat) != (uint)OutfitCategoriesExtended.IsHat) continue;

							part = new CASPart(ResourceKey.FromString("034AEECB:00000000:609478AEDB054FA7"));
						}
						else
						{
							part = OutfitUtils.FindReplacementPartForCategory(builder, outfitCategories, item, simDesc);
						}

						if (item.Key != part.Key)
						{
							builder.RemovePart(item);
							if (part.Key.InstanceId != 0)
							{
								OutfitUtils.AddPartAndPreset(builder, part, randomizePreset: true);
								OutfitUtils.AdjustPresetForHairColor(builder, part, simDesc);
							}
						}
					}
				}
				OutfitUtils.AddMissingParts(builder, outfitCategories | (OutfitCategories)OutfitCategoriesExtended.ValidForRandom, simDesc);
			}

			private void BuildPregnantOutfit(SimOutfit outfit, OutfitCategories category)
			{
				SimDescription simDescription = Actor.SimDescription;
				if (simDescription.AllowOutfitCategoryForPregnancy(category))
				{
					List<ResourceKey> keys = new List<ResourceKey>();
					switch (category)
					{
						case OutfitCategories.Naked:
						case OutfitCategories.Singed:
							break;
						case OutfitCategories.Everyday:
							if (GameUtils.IsInstalled(ProductVersion.EP4))
							{
								keys.Add(ResourceKey.CreateOutfitKeyFromProductVersion("amPregnantEveryday", ProductVersion.EP4));
							}
							else
							{
								keys.Add(ResourceKey.CreateOutfitKeyFromProductVersion("amPregnantOuterwear", ProductVersion.BaseGame));
							}
							break;
						case OutfitCategories.Sleepwear:
							keys.Add(ResourceKey.CreateOutfitKeyFromProductVersion("amPregnantSleepwear", GameUtils.IsInstalled(ProductVersion.EP4) ? ProductVersion.EP4 : ProductVersion.BaseGame));
							break;
						case OutfitCategories.Swimwear:
							keys.Add(ResourceKey.CreateOutfitKeyFromProductVersion("amPregnantSwimwear", GameUtils.IsInstalled(ProductVersion.EP4) ? ProductVersion.EP4 : ProductVersion.BaseGame));
							break;
						case OutfitCategories.Formalwear:
						case OutfitCategories.Outerwear:
							keys.Add(ResourceKey.CreateOutfitKeyFromProductVersion("amPregnantOuterwear", ProductVersion.BaseGame));
							break;
						default:
							if (GameUtils.IsInstalled(ProductVersion.EP4))
							{
								keys.Add(ResourceKey.CreateOutfitKeyFromProductVersion("amPregnantEveryday", ProductVersion.EP4));
							}
							keys.Add(ResourceKey.CreateOutfitKeyFromProductVersion("amPregnantOuterwear", ProductVersion.BaseGame));
							break;
					}

					ResourceKey key = ResourceKey.kInvalidResourceKey;
					if (keys.Count > 0)
					{
						if (keys.Count == 1)
						{
							key = keys[0];
						}
						else
						{
							key = RandomUtil.GetRandomObjectFromList(keys);
						}
					}

					SimOutfit resultOutfit = outfit;
					if (key != ResourceKey.kInvalidResourceKey)
					{
						SimOutfit uniform = new SimOutfit(key);
						if (uniform.IsValid)
						{
							OutfitUtils.TryApplyUniformToOutfit(outfit, uniform, simDescription, "CreateOutfitForSim", out resultOutfit);
						}
					}

					if (simDescription.mMaternityOutfits[category] == null)
					{
						simDescription.mMaternityOutfits[category] = new ArrayList();
					}
					ArrayList arrayList = simDescription.mMaternityOutfits[category] as ArrayList;
					if (arrayList.Count == 0)
					{
						SimBuilder simBuilder = new SimBuilder();
						simBuilder.UseCompression = true;
						OutfitUtils.SetOutfit(simBuilder, resultOutfit, simDescription);
						MakePartsCategoryAppropriateForMaternity(simBuilder, category, simDescription);
						key = simBuilder.CacheOutfit("maternity" + category.ToString() + "_" + resultOutfit.Key.ToString());
						arrayList.Add(new SimOutfit(key));
					}
				}
			}

			private void PutOnPregnantOutfit()
			{
				bool playClothesSpin = true;
				SimOutfit simOutfit = null;
				SimDescription simDescription = Actor.SimDescription;
				OutfitCategories outfitCategories = Actor.CurrentOutfitCategory;
				if (simDescription.AllowOutfitCategoryForPregnancy(outfitCategories))
				{
					simOutfit = Actor.CurrentOutfit;
				}
				else
				{
					outfitCategories = OutfitCategories.Everyday;
				}
				ArrayList arrayList = simDescription.mOutfits[outfitCategories] as ArrayList;
				if ((simOutfit == null || !simOutfit.IsValid) && arrayList.Count > 0)
				{
					simOutfit = arrayList[0] as SimOutfit;
				}
				BuildPregnantOutfit(simOutfit, outfitCategories);
				if (playClothesSpin)
				{
					int num = 0;
					CASPart[] parts = simOutfit.Parts;
					for (int i = 0; i < parts.Length; i++)
					{
						CASPart cASPart = parts[i];
						switch (cASPart.BodyType)
						{
							case BodyTypes.FullBody:
								if ((cASPart.CategoryFlags & 0x100000) != 0)
								{
									num += 2;
								}
								break;
							case BodyTypes.UpperBody:
							case BodyTypes.LowerBody:
								if ((cASPart.CategoryFlags & 0x100000) != 0)
								{
									num++;
								}
								break;
						}
					}
					if (num == 2)
					{
						playClothesSpin = false;
					}
				}
				simOutfit = null;
				if (playClothesSpin)
				{
					Actor.SwitchToOutfitWithSpin(Sim.ClothesChangeReason.Force, outfitCategories, ignoreCurrentCategory: true);
				}
				else
				{
					Actor.SwitchToOutfitWithoutSpin(outfitCategories);
				}
			}
		}

		public override string GetTitlePrefix()
		{
			return "StartMalePregnancy";
		}

		protected override int GetMaxSelection()
		{
			return 0;
		}

		protected override bool CanApplyAll()
		{
			return true;
		}

		protected override bool PrivateAllow(SimDescription me)
		{
			if (me.CreatedSim == null) return false;

			if (!GameUtils.IsInstalled(ProductVersion.EP8)) return false;

			string reason = null;
			if (!Allow(me.CreatedSim, ref reason))
			{
				Common.DebugNotify("Reason: " + reason);
				return false;
			}

			return base.PrivateAllow(me);
		}

		public static bool Allow(Sim man, ref string reason)
		{
			if ((man == null) || (man.InteractionQueue == null))
			{
				reason = Common.Localize("Pollinate:Uninstantiated");
				return false;
			}
			else if (!man.IsHuman)
			{
				reason = Common.Localize("Pollinate:NotHuman", man.IsFemale, new object[] { man });
				return false;
			}
			else if (man.IsFemale)
			{
				reason = Common.Localize("AddMalePregnancy:IsFemale", man.IsFemale, new object[] { man });
				return false;
			}
			else if (man.LotHome == null)
			{
				reason = Common.Localize("Pollinate:Homeless", man.IsFemale, new object[] { man });
				return false;
			}
			else if (SimTypes.IsSpecial(man.Household))
			{
				reason = Common.Localize("Pollinate:Service", man.IsFemale, new object[] { man });
				return false;
			}
			else if (!man.SimDescription.YoungAdult && !man.SimDescription.Adult)
			{
				reason = Common.Localize("Pollinate:TooYoung", man.IsFemale, new object[] { man });
				return false;
			}
			else if (man.SimDescription.IsPregnant || man.SimDescription.IsVisuallyPregnant)
			{
				reason = Common.Localize("Pollinate:AlreadyPregnant", man.IsFemale, new object[] { man });
				return false;
			}
			else if (man.BuffManager.HasTransformBuff())
			{
				reason = Common.Localize("Pollinate:TransformBuff", man.IsFemale, new object[] { man });
				return false;
			}
			else if ((man.SimDescription.AgingState == null) || (man.SimDescription.AgingState.IsAgingInProgress()))
			{
				reason = Common.Localize("Pollinate:TooOld", man.IsFemale, new object[] { man });
				return false;
			}

			return true;
		}

		protected override bool Run(SimDescription me, bool singleSelection)
		{
			if (me == null) return true;

			Sim man = me.CreatedSim;
			if (man == null) return true;

			if (!ApplyAll)
			{
				if (!AcceptCancelDialog.Show(Common.Localize(GetTitlePrefix() + ":Prompt", me.IsFemale, new object[] { me })))
				{
					return false;
				}
			}

			string reason = null;
			if (!Allow(man, ref reason))
			{
				Common.Notify(reason);
				return true;
			}

			InteractionInstance interactionInstance = ShowPregnancy.Singleton.CreateInstance(man, man, new InteractionPriority(InteractionPriorityLevel.ESRB), isAutonomous: false, cancellableByPlayer: false);
			interactionInstance.Hidden = true;
			man.InteractionQueue.AddNext(interactionInstance);

			return true;
		}
	}
}
