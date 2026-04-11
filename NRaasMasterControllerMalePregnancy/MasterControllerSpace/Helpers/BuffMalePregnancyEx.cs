using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.SimIFace.CustomContent;
using System;
using System.Collections.Generic;
using System.Collections;
using Sims3.UI.Hud;

namespace NRaas.MasterControllerSpace.Helpers
{
	public class BuffMalePregnancyEx : BuffMalePregnancy
	{
		private static readonly uint kHashOriginalWeight = ResourceUtils.HashString32("NRaas.MasterControllerSpace.Helpers.BuffMalePregnancyEx.BuffInstanceMalePregnancy.OriginalWeight");
		private static readonly uint kHashAlienParentID = ResourceUtils.HashString32("NRaas.MasterControllerSpace.Helpers.BuffMalePregnancyEx.BuffInstanceMalePregnancy.AlienParentID");

		public new class BuffInstanceMalePregnancy : BuffMalePregnancy.BuffInstanceMalePregnancy, Common.IStartupApp, Common.IPreLoad
		{	
			public static BuffMalePregnancyInfo sMalePregnancyInfo = new BuffMalePregnancyInfo();
			
			public void OnStartupApp()
			{
				ParseUniformData();
			}

			public void OnPreLoad()
			{
				BuffData info = new BuffData();
				info.mProductVersion = ProductVersion.EP8;
				ResourceKey resourceKey = ResourceKey.kInvalidResourceKey;
				resourceKey = ResourceKey.CreatePNGKey("moodlet_malepregnancy", ResourceUtils.ProductVersionToGroupId(info.mProductVersion));
				if (!World.ResourceExists(resourceKey))
				{
					resourceKey = ResourceKey.CreatePNGKey("moodlet_malepregnancy", 0u);
				}
				info.mBuffGuid = BuffNames.MalePregnancy;
				info.mBuffName = "MalePregnancy";
				info.mDescription = "MalePregnancyDescription";
				info.mBuffCategory = BuffCategory.None;
				info.mVersion = 1.0;
				info.mAxisEffected = MoodAxis.Uncomfortable;
				info.mPolarityOverride = Polarity.NoOverride;
				info.mEffectValue = -5;
				info.mDelayTimer = 0;
				info.mTimeoutSimMinutes = 2880f;
				info.mSolveCommodity = CommodityKind.None;
				info.mSolveTime = float.MinValue;
				info.mNeededTraitList = new List<TraitNames>() { TraitNames.RobotHiddenTrait };
				info.mIncreasedEffectivenessList = new List<SocialManager.SocialEffectiveness>() { SocialManager.SocialEffectiveness.None };
				info.mReducedEffectivenessList = new List<SocialManager.SocialEffectiveness>() { SocialManager.SocialEffectiveness.None };
				info.mThumbKey = resourceKey;
				info.mThumbString = "moodlet_malepregnancy";
				info.SetFlags(BuffData.FlagField.Travel, true);
				info.mDisallowedOccults = new List<OccultTypes>() { OccultTypes.Mummy, OccultTypes.Frankenstein };
				info.mJazzStateSuffix = info.mBuffName;
				info.mAvailabilityFlags = CASAGSAvailabilityFlags.HumanAgeMask;

				BuffMalePregnancyEx buff = new BuffMalePregnancyEx(info);
				BuffInstanceMalePregnancy buffInstance = buff.CreateBuffInstance() as BuffInstanceMalePregnancy;

				if (GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary.ContainsKey((ulong)BuffNames.MalePregnancy))
				{
					if (GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary[(ulong)BuffNames.MalePregnancy].mBuff.BuffVersion < buff.BuffVersion)
					{
						GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary[(ulong)BuffNames.MalePregnancy] = buffInstance;
					}
				}
				else
				{
					GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary.Add((ulong)BuffNames.MalePregnancy, buffInstance);
					BuffManager.sBuffEnumValues.AddNewEnumValue(info.mBuffName, BuffNames.MalePregnancy);
				}
			}

			public BuffInstanceMalePregnancy()
			{
			}

			public BuffInstanceMalePregnancy(Buff buff, BuffNames buffGuid, int effectValue, float timeoutCount)
				: base(buff, buffGuid, effectValue, timeoutCount)
			{
			}

			public override BuffInstance Clone()
			{
				return new BuffInstanceMalePregnancy(mBuff, mBuffGuid, mEffectValue, mTimeoutCount);
			}

			public override bool ExportContent(ResKeyTable resKeyTable, ObjectIdTable objIdTable, IPropertyStreamWriter writer)
			{
				base.ExportContent(resKeyTable, objIdTable, writer);
				writer.WriteUint64(BuffMalePregnancyEx.kHashAlienParentID, AlienParentID);
				writer.WriteFloat(BuffMalePregnancyEx.kHashOriginalWeight, OriginalWeight);
				return true;
			}

			public override void ImportContentSpecial(ResKeyTable resKeyTable, ObjectIdTable objIdTable, IPropertyStreamReader reader)
			{
				reader.ReadUint64(BuffMalePregnancyEx.kHashAlienParentID, out AlienParentID, 0L);
				reader.ReadFloat(BuffMalePregnancyEx.kHashOriginalWeight, out OriginalWeight, 0f);
			}

			public override void OnTimeOutUpdated()
			{
				if (base.TimeoutCount <= 1f)
				{
					if (base.TimeoutCount <= 0f)
					{
						this.mTimeoutCount = 1f;
					}
					Sim createdSim = this.TargetSim.CreatedSim;
					createdSim.BuffManager.PauseBuff(BuffNames.MalePregnancy);
					InteractionInstance interactionInstance = AlienUtils.MaleHaveBabyHome.MaleHaveBabyHomeSingleton.CreateInstance(createdSim.LotHome, createdSim, new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
					(interactionInstance as AlienUtils.MaleHaveBabyHome).AlienParentID = this.AlienParentID;
					createdSim.InteractionQueue.AddNext(interactionInstance);
					return;
				}
				if (SimClock.ElapsedTime(TimeUnit.Minutes, this.LastTimeUpdated) >= (float)60)
				{
					UpdateBodyShape();
					if (RandomUtil.RandomChance01(Pregnancy.kChanceOfBackache))
					{
						Sim createdSim = TargetSim.CreatedSim;
						createdSim.AddAlarm(1f, TimeUnit.Seconds, () => createdSim.BuffManager.AddElement(BuffNames.Backache, Origin.FromPregnancy), "Add backage", AlarmType.DeleteOnReset);

					}
					this.LastTimeUpdated = SimClock.CurrentTime();
				}
				if (!this.mPregnantWalkRequested && this.TargetSim.Weight >= (BuffMalePregnancy.kFatnessMax * 0.5f))
				{
					this.TargetSim.CreatedSim.RequestWalkStyle(Sim.WalkStyle.Pregnant);
					this.mPregnantWalkRequested = true;
				}
			}

			public void UpdateBodyShape()
			{
				int num = (int)(mBuff.TimeoutSimMinutes - TimeoutCount);
				num = Math.Min(num, (int)mBuff.TimeoutSimMinutes);
				float num2 = (float)num / mBuff.TimeoutSimMinutes;
				if (num2 == 0f)
				{
					num2 = 0.01f;
				}
				SetPregnancy(TargetSim, num2, false);
				TargetSim.SetBodyShape((BuffMalePregnancy.kFatnessMax-OriginalWeight)*(float)Math.Sqrt(num2)+OriginalWeight, TargetSim.Fitness);
			}
		}
		
		public sealed class BuffMalePregnancyInfo
		{
			public struct OutfitInfo
			{
				public string mOutfitName;
	
				public CASAgeGenderFlags mAgeGenderOfUniform;
	
				public ProductVersion mDataVersion;
				
				public OutfitCategories mCategories;
				
				public uint mVersion;
			}
			
			private List<OutfitInfo> mOutfitsInfo;
			
			public BuffMalePregnancyInfo()
			{
				mOutfitsInfo = new List<OutfitInfo>();
			}
			
			public void AddUniform(string name, CASAgeGenderFlags flags, ProductVersion productVersion, OutfitCategories outfitCategories, uint version)
			{
				OutfitInfo item = new OutfitInfo
				{
					mOutfitName = name,
					mAgeGenderOfUniform = flags,
					mDataVersion = productVersion,
					mCategories = outfitCategories,
					mVersion = version
				};
				if (GameUtils.IsInstalled(productVersion))
				{
					mOutfitsInfo.Add(item);
				}
			}
	
			public void GetUniformNameAndVersion(OutfitCategories outfitCategory, CASAgeGenderFlags flags, out OutfitInfo outfitInfo)
			{
				outfitInfo.mAgeGenderOfUniform = CASAgeGenderFlags.None;
				outfitInfo.mOutfitName = string.Empty;
				outfitInfo.mDataVersion = ProductVersion.Undefined;
				outfitInfo.mCategories = OutfitCategories.None;
				outfitInfo.mVersion = 0u;
				List<OutfitInfo> list = new List<OutfitInfo>();
				foreach (OutfitInfo item in mOutfitsInfo)
				{
					if ((outfitCategory & item.mCategories) != OutfitCategories.None && (flags & item.mAgeGenderOfUniform & CASAgeGenderFlags.AgeMask) != CASAgeGenderFlags.None && (flags & item.mAgeGenderOfUniform & CASAgeGenderFlags.GenderMask) != CASAgeGenderFlags.None)
					{
						if (list.Count > 0)
						{
							if (item.mVersion < list[list.Count - 1].mVersion)
							{
								continue;
							}
							if (item.mVersion > list[list.Count - 1].mVersion)
							{
								list.Clear();
							}
						}
						list.Add(item);
					}
				}
				if (list.Count > 0)
				{
					outfitInfo = RandomUtil.GetRandomObjectFromList(list);
				}
			}
		}
		
		public static void ParseUniformData()
		{
			XmlDbData xmlDbData = XmlDbData.ReadData("BuffMalePregnancyExUniforms");
			XmlDbTable value = null;
			xmlDbData.Tables.TryGetValue("Uniform", out value);
			foreach (XmlDbRow row in value.Rows)
			{
				string uniformID = row.GetString("UniformID");
				if (!string.IsNullOrEmpty(uniformID))
				{
					CASAgeGenderFlags cASAgeGenderFlags = ParserFunctions.ParseAllowableAges(row, "UniformAge") | CASAgeGenderFlags.Male;
					ProductVersion productVersion = ProductVersion.BaseGame;
					ParserFunctions.TryParseEnum(row.GetString("UniformDataVersion"), out productVersion, ProductVersion.BaseGame);
					OutfitCategories outfitCategories = OutfitCategories.None;
					string outfitCategoriesText = row.GetString("OutfitCategories");
					if (!string.IsNullOrEmpty(outfitCategoriesText))
					{
						List<OutfitCategories> outfitCategoriesList = new List<OutfitCategories>();
						ParserFunctions.TryParseCommaSeparatedList(outfitCategoriesText, out outfitCategoriesList, OutfitCategories.None);
						foreach (OutfitCategories category in outfitCategoriesList)
						{
							outfitCategories |= category;
						}
					}
					if (outfitCategories != OutfitCategories.None)
					{
						uint version = row.GetUInt("Version", 0u);
						BuffInstanceMalePregnancy.sMalePregnancyInfo.AddUniform(uniformID, cASAgeGenderFlags, productVersion, outfitCategories, version);
					}
				}
			}
		}
		
		public static void ShowPregnancy(SimDescription simDesc)
		{
			if (simDesc.mCurrentShape.Pregnant == 0f)
			{
				SetPregnancy(simDesc, 0.01f, true);
			}
		}

		public static void SetPregnancy(SimDescription simDesc, float percentPregnant)
		{
			SetPregnancy(simDesc, percentPregnant, false);
		}

		public static void SetPregnancy(SimDescription simDesc, float percentPregnant, bool playClothesSpin)
		{
			simDesc.mCurrentShape.Pregnant = percentPregnant;
			if (simDesc.mSim == null)
			{
				return;
			}
			if (percentPregnant == 0f)
			{
				simDesc.RefreshBodyShape();
				simDesc.IsVisuallyPregnant = false;
				return;
			}
			if (!simDesc.IsVisuallyPregnant)
			{
				simDesc.IsVisuallyPregnant = true;
				if (simDesc.CreatedSim != null)
				{
					BuffTransformation transformBuff = simDesc.CreatedSim.BuffManager.TransformBuff;
					if (transformBuff != null)
					{
						simDesc.CreatedSim.BuffManager.RemoveElement(transformBuff.BuffGuid);
					}
				}
				OutfitCategories[] listOfCategories = simDesc.ListOfCategories;
				for (int i = 0; i < listOfCategories.Length; i++)
				{
					OutfitCategories outfitCategories = listOfCategories[i];
					if (simDesc.AllowOutfitCategoryForPregnancy(outfitCategories))
					{
						ArrayList arrayList = simDesc.mOutfits[outfitCategories] as ArrayList;
						if (arrayList != null && arrayList.Count > 0)
						{
							SimOutfit outfit = arrayList[0] as SimOutfit;
							BuildPregnantOutfit(simDesc, outfit, outfitCategories);
						}
					}
				}
				PutOnPregnantOutfit(simDesc, playClothesSpin);
				simDesc.RefreshBodyShape();
			}
		}

		private static void BuildPregnantOutfit(SimDescription simDescription, SimOutfit outfit, OutfitCategories category)
		{
			if (simDescription.AllowOutfitCategoryForPregnancy(category))
			{
				BuffMalePregnancyEx.BuffMalePregnancyInfo.OutfitInfo outfitInfo = default(BuffMalePregnancyEx.BuffMalePregnancyInfo.OutfitInfo);
				BuffMalePregnancyEx.BuffInstanceMalePregnancy.sMalePregnancyInfo.GetUniformNameAndVersion(category, simDescription.AgeGenderSpecies, out outfitInfo);
				
				SimOutfit uniform = null;
				if ((category & outfitInfo.mCategories) != OutfitCategories.None)
				{
					ResourceKey key = ResourceKey.CreateOutfitKeyFromProductVersion(outfitInfo.mOutfitName, outfitInfo.mDataVersion);
					uniform = new SimOutfit(key);
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
					SimOutfit resultOutfit = outfit;
					if (uniform != null)
					{
						OutfitUtils.TryApplyUniformToOutfit(outfit, uniform, simDescription, "CreateMaternityOutfitForSim", out resultOutfit, simBuilder);
					}
					OutfitUtils.SetOutfit(simBuilder, resultOutfit, simDescription);
					MakePartsCategoryAppropriateForMaternity(simBuilder, category, simDescription);
					ResourceKey key = simBuilder.CacheOutfit("maternity" + category.ToString() + "_" + resultOutfit.Key.ToString());
					arrayList.Add(new SimOutfit(key));
				}
			}
		}

		private static void PutOnPregnantOutfit(SimDescription simDesc, bool playClothesSpin)
		{
			SimOutfit simOutfit = null;
			OutfitCategories outfitCategories = simDesc.mSim.CurrentOutfitCategory;
			if (simDesc.AllowOutfitCategoryForPregnancy(outfitCategories))
			{
				simOutfit = simDesc.mSim.CurrentOutfit;
			}
			else
			{
				outfitCategories = OutfitCategories.Everyday;
			}
			if (simOutfit == null || !simOutfit.IsValid)
			{
				ArrayList arrayList = simDesc.mOutfits[outfitCategories] as ArrayList;
				if (arrayList != null && arrayList.Count > 0)
				{
					simOutfit = (arrayList[0] as SimOutfit);
				}
			}
			BuildPregnantOutfit(simDesc, simOutfit, outfitCategories);
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
			if (playClothesSpin)
			{
				simDesc.mSim.SwitchToOutfitWithSpin(Sim.ClothesChangeReason.Force, outfitCategories, true);
			}
			else
			{
				simDesc.mSim.SwitchToOutfitWithoutSpin(outfitCategories);
			}
		}
		
		private static void MakePartsCategoryAppropriateForMaternity(SimBuilder builder, OutfitCategories category, SimDescriptionCore simDesc)
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

		public override BuffInstance CreateBuffInstance()
		{
			return new BuffInstanceMalePregnancy(this, base.BuffGuid, base.EffectValue, base.TimeoutSimMinutes);
		}

		public BuffMalePregnancyEx(BuffData info)
			: base(info)
		{
		}

		public override bool ShouldAdd(BuffManager bm, MoodAxis axisEffected, int moodValue)
		{
			return base.ShouldAdd(bm, axisEffected, moodValue) && !bm.Actor.SimDescription.IsPregnant && bm.Actor.SimDescription.YoungAdultOrAdult;
		}

		public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
		{
			BuffMalePregnancyEx.BuffInstanceMalePregnancy buffInstanceMalePregnancy = bi as BuffMalePregnancyEx.BuffInstanceMalePregnancy;
			if (travelReaddition)
			{
				buffInstanceMalePregnancy.mTargetSim = bm.Actor.SimDescription;
				buffInstanceMalePregnancy.LastTimeUpdated = SimClock.CurrentTime();
				buffInstanceMalePregnancy.UpdateBodyShape();
			}
			else
			{
				base.OnAddition(bm, bi, travelReaddition);
				SetPregnancy(bm.Actor.SimDescription, 0.01f);
			}
		}
		
		public override void OnRemoval(BuffManager bm, BuffInstance bi)
		{
			base.OnRemoval(bm, bi);
			SetPregnancy(bm.Actor.SimDescription, 0f);
		}
	}
}