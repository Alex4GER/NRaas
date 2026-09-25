using NRaas.PregnancyControllerSpace.Helpers;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using Sims3.UI.Hud;

namespace NRaas.MasterControllerSpace.Helpers
{
    public class BuffAbductedEx : SharedTemperatureBuffs
    {
        public class BuffInstanceAbductedEx : BuffInstance, Common.IPreLoad
        {
            public void OnPreLoad()
            {
                BuffData info = new BuffData();
                info.mProductVersion = ProductVersion.EP8;
                ResourceKey resourceKey = ResourceKey.kInvalidResourceKey;
                resourceKey = ResourceKey.CreatePNGKey("moodlet_abducted", ResourceUtils.ProductVersionToGroupId(info.mProductVersion));
                if (!World.ResourceExists(resourceKey))
                {
                    resourceKey = ResourceKey.CreatePNGKey("moodlet_abducted", 0u);
                }
                info.mBuffGuid = BuffNames.Abducted;
                info.mBuffName = "Abducted";
                info.mDescription = "AbductedDescription";
                info.mBuffCategory = BuffCategory.None;
                info.mVersion = 1.0;
                info.mAxisEffected = MoodAxis.Uncomfortable;
                info.mPolarityOverride = Polarity.NoOverride;
                info.mEffectValue = -10;
                info.mDelayTimer = 0;
                info.mTimeoutSimMinutes = 600f;
                info.mSolveCommodity = CommodityKind.None;
                info.mSolveTime = float.MinValue;
                info.mNeededTraitList = new List<TraitNames>() { TraitNames.RobotHiddenTrait };
                info.mIncreasedEffectivenessList = new List<SocialManager.SocialEffectiveness>() { SocialManager.SocialEffectiveness.None };
                info.mReducedEffectivenessList = new List<SocialManager.SocialEffectiveness>() { SocialManager.SocialEffectiveness.None };
                info.mThumbKey = resourceKey;
                info.mThumbString = "moodlet_abducted";
                info.mDisallowedOccults = new List<OccultTypes>() { OccultTypes.None };
                info.mJazzStateSuffix = info.mBuffName;
                info.mAvailabilityFlags = CASAGSAvailabilityFlags.HumanAgeMask;

                BuffAbductedEx buff = new BuffAbductedEx(info);
                BuffInstanceAbductedEx buffInstance = buff.CreateBuffInstance() as BuffInstanceAbductedEx;

                if (GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary.ContainsKey((ulong)BuffNames.Abducted))
                {
                    if (GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary[(ulong)BuffNames.Abducted].mBuff.BuffVersion < buff.BuffVersion)
                    {
                        GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary[(ulong)BuffNames.Abducted] = buffInstance;
                    }
                }
                else
                {
                    GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary.Add((ulong)BuffNames.Abducted, buffInstance);
                    BuffManager.sBuffEnumValues.AddNewEnumValue(info.mBuffName, BuffNames.Abducted);
                }
            }

            public BuffInstanceAbductedEx()
            {
            }

            public BuffInstanceAbductedEx(Buff buff, BuffNames buffGuid, int effectValue, float timeoutCount)
                : base(buff, buffGuid, effectValue, timeoutCount)
            {
            }

            public override BuffInstance Clone()
            {
                return new BuffInstanceAbductedEx(mBuff, mBuffGuid, mEffectValue, mTimeoutCount);
            }
        }

        public override BuffInstance CreateBuffInstance()
        {
            return new BuffInstanceAbductedEx(this, base.BuffGuid, base.EffectValue, base.TimeoutSimMinutes);
        }

        public BuffAbductedEx(BuffData info)
            : base(info)
        {
        }

        public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
        {
            SimDescription simDescription = bm.Actor.SimDescription;
            if (!GameUtils.IsUniversityWorld() && simDescription.IsMale && simDescription.YoungAdultOrAbove && !simDescription.OccultManager.HasAnyOccultType() && RandomUtil.RandomChance01(BuffAbducted.kChanceMalePregnancy))
            {
                MalePregnancy p = MalePregnancy.Start(bm.Actor);
                p.mHourOfPregnancy = 14;
            }
            base.OnAddition(bm, bi, travelReaddition);
        }
    }
}