using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.SimIFace.CustomContent;
using System;
using System.Collections.Generic;
using Sims3.UI.Hud;

namespace NRaas.PregnancyControllerSpace.Helpers
{
    public class BuffMalePregnancyEx : BuffMalePregnancy
    {
        private static readonly uint kHashOriginalWeight = ResourceUtils.HashString32("NRaas.MasterControllerSpace.Helpers.BuffMalePregnancyEx.BuffInstanceMalePregnancy.OriginalWeight");
        private static readonly uint kHashAlienParentID = ResourceUtils.HashString32("NRaas.MasterControllerSpace.Helpers.BuffMalePregnancyEx.BuffInstanceMalePregnancy.AlienParentID");

        public new class BuffInstanceMalePregnancy : BuffMalePregnancy.BuffInstanceMalePregnancy, Common.IPreLoad
        {
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
                info.mTimeoutSimMinutes = -1f;
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

            public int PausedAtHour;

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
            MalePregnancy p = bm.Actor.SimDescription.Pregnancy as MalePregnancy;
            return base.ShouldAdd(bm, axisEffected, moodValue) && !bm.Actor.BuffManager.HasElement(BuffNames.Pregnant) && (!bm.Actor.SimDescription.IsPregnant || (p != null && p.BuffInstance == null)) && bm.Actor.SimDescription.TeenOrAbove;
        }

        public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
        {
            BuffMalePregnancyEx.BuffInstanceMalePregnancy buffInstanceMalePregnancy = bi as BuffMalePregnancyEx.BuffInstanceMalePregnancy;
            MalePregnancy p = bm.Actor.SimDescription.Pregnancy as MalePregnancy;
            int hourOfPregnancy = 24;
            if (p == null && bm.Actor.SimDescription.Pregnancy != null)
            {
                Pregnancy pregnancy = bm.Actor.SimDescription.Pregnancy;
                hourOfPregnancy = pregnancy.mHourOfPregnancy;
                pregnancy.ClearPregnancyData();
            }
            if (travelReaddition)
            {
                buffInstanceMalePregnancy.mTargetSim = bm.Actor.SimDescription;
                buffInstanceMalePregnancy.LastTimeUpdated = SimClock.CurrentTime();
                if (p == null)
                {
                    p = new MalePregnancy(bm.Actor);
                    p.BuffInstance = buffInstanceMalePregnancy;
                    p.DadDescriptionId = buffInstanceMalePregnancy.AlienParentID;
                    p.OriginalWeight = buffInstanceMalePregnancy.OriginalWeight;
                    p.mHourOfPregnancy = hourOfPregnancy;
                    p.Continue(bm.Actor, false);
                }
                else
                {
                    p.SetPregoBlendShape();
                }
                bm.Actor.AddAlarm(1f, TimeUnit.Minutes, new AlarmTimerCallback(p.RequestPregnantWalkStyle), "Request Pregnant Walkstyle", AlarmType.AlwaysPersisted);
            }
            else
            {
                SimDescription simDescription = bm.Actor.SimDescription;
                buffInstanceMalePregnancy.OriginalWeight = (p == null ? simDescription.Weight : p.OriginalWeight);
                if (p == null)
                {
                    p = MalePregnancy.Start(bm.Actor);
                    p.mHourOfPregnancy = hourOfPregnancy;
                }
                p.BuffInstance = buffInstanceMalePregnancy;
                buffInstanceMalePregnancy.AlienParentID = p.DadDescriptionId;
                p.OriginalWeight = buffInstanceMalePregnancy.OriginalWeight;
                bm.Actor.SimDescription.SetPregnancy(0.01f);
            }
        }

        public override void OnRemoval(BuffManager bm, BuffInstance bi)
        {
            BuffMalePregnancyEx.BuffInstanceMalePregnancy buffInstanceMalePregnancy = bi as BuffMalePregnancyEx.BuffInstanceMalePregnancy;
            MalePregnancy p = bm.Actor.SimDescription.Pregnancy as MalePregnancy;
            if (p != null)
            {
                p.BuffInstance = null;
            }
        }
    }
}