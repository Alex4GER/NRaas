using NRaas.CommonSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;

namespace NRaas.PregnancyControllerSpace.Helpers
{
    public class PregnancyEx : Pregnancy
    {
        public new class InternalHaveBabyHome : Interaction<Sim, Sim>
        {
            public class Definition : Pregnancy.InternalHaveBabyHome.Definition
            {
                public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
                {
                    InteractionInstance result = new InternalHaveBabyHome();
                    result.Init(ref parameters);
                    return result;
                }
            }

            public int TotalCount;

            public int BabyIndex;

            public bool UseFatness = false;

            public float OriginalWeight = 0f;

            public static InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                this.mCurrentStateMachine.SetActor("y", this.Target);
                this.mCurrentStateMachine.EnterState("x", "Enter");
                base.AnimateSim("HaveBaby");
                this.Actor.SimDescription.SetPregnancy(1f - (float)this.BabyIndex / (float)this.TotalCount);
                if (this.UseFatness)
                {
                    this.Actor.SimDescription.SetBodyShape(kFatnessMax - (kFatnessMax - OriginalWeight) * ((float)this.BabyIndex / (float)this.TotalCount), this.Actor.SimDescription.Fitness);
                }
                base.AnimateSim("Exit");
                MakeBabyVisible(this.Target);
                ChildUtils.CarryChild(this.Actor, this.Target, true);
                return true;
            }

            public override void Cleanup()
            {
                if (this.Actor.Posture != null && this.Actor.Posture.Container != this.Target)
                {
                    this.Target.UnParent();
                    this.Target.AttemptToPutInSafeLocation(true);
                }
                if (this.BabyIndex == this.TotalCount && !this.Actor.HasBeenDestroyed)
                {
                    if (this.Actor.SimDescription.IsVisuallyPregnant && this.UseFatness)
                    {
                        this.Actor.SimDescription.SetBodyShape(OriginalWeight, this.Actor.SimDescription.Fitness);
                    }
                    this.Actor.SimDescription.SetPregnancy(0f);
                }
                base.Cleanup();
            }

            public override ThumbnailKey GetIconKey()
            {
                return new ThumbnailKey(new ResourceKey(Pregnancy.InternalHaveBabyHome.kIconNameHash, 796721156u, 0u), (ThumbnailSize)1);
            }
        }

        protected static float kFatnessMax = BuffMalePregnancy.kFatnessMax;

        protected float mOriginalWeight;

        protected float mWeight;

        protected bool mUseFatness;

        public bool IsPaused
        {
            get
            {
                return mMom.IsSelectable && (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld());
            }
        }

        public float OriginalWeight
        {
            get
            {
                return mOriginalWeight;
            }
        }

        public bool UseFatness
        {
            get
            {
                return mUseFatness;
            }
        }

        public PregnancyEx()
        {
        }

        public PregnancyEx(Sim mom, SimDescription dadDesc)
            : base(mom, dadDesc)
        {
            this.mUseFatness = PregnancyController.Settings.mUseFatnessForMalePregnancy && mom.IsMale;
        }

        public virtual void MergeAndStart(Sim woman)
        {
            PregnancyEx pregnancy = new PregnancyEx(woman, null);
            StartFromMerge(woman, this, pregnancy);
        }

        public static void StartFromMerge(Sim woman, Pregnancy incomingPreg, PregnancyEx pregnancy)
        {
            AgingManager.Singleton.CancelAgingAlarmsForSim(woman.SimDescription.AgingState);
            pregnancy.PreggersAlarm = woman.AddAlarmRepeating(1f, TimeUnit.Hours, new AlarmTimerCallback(pregnancy.HourlyCallback), 1f, TimeUnit.Hours, "Hourly Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
            pregnancy.DadDescriptionId = incomingPreg.DadDescriptionId;
            pregnancy.mDadDeathType = incomingPreg.mDadDeathType;
            pregnancy.CheckForDad();
            woman.SimDescription.Pregnancy = pregnancy;
        }

        public void MergeData(PregnancyEx incomingPreg)
        {
            base.MergeData(incomingPreg);
            this.mOriginalWeight = incomingPreg.mOriginalWeight;
            this.mUseFatness = incomingPreg.mUseFatness;
        }

        public override void ClearPregnancyData()
        {
            if (this.mContractionBroadcast != null)
            {
                this.mContractionBroadcast.Dispose();
            }
            if (this.mMom != null)
            {
                this.mMom.RemoveAlarm(this.PreggersAlarm);
                this.mMom.RemoveAlarm(this.mContractionsAlarm);
                if (this.mUseFatness && this.mMom.SimDescription.IsVisuallyPregnant)
                {
                    this.mMom.SimDescription.SetBodyShape(mOriginalWeight, this.mMom.SimDescription.Fitness);
                }
                this.mMom.SimDescription.SetPregnancy(0f);
                this.mMom.RemoveInteractionByType(TakeToHospital.Singleton);
                this.UnrequestPregnantWalkStyle();
                if (!this.mMom.HasBeenDestroyed)
                {
                    this.mMom.BuffManager.RemoveElement(BuffNames.BabyIsComing);
                    if (!this.mMom.SimDescription.IsVampire)
                    {
                        Motive motive = this.mMom.Motives.GetMotive(CommodityKind.Hunger);
                        if (motive != null)
                        {
                            motive.PregnantMotiveDecay = false;
                        }
                    }
                    Motive motive2 = this.mMom.Motives.GetMotive(CommodityKind.Bladder);
                    if (motive2 != null)
                    {
                        motive2.PregnantMotiveDecay = false;
                    }
                }
                this.mMom.SimDescription.NullOutPregnancy();
            }
        }

        public override bool Continue(Sim mom, bool momJustCreated)
        {
            if (mom == null)
            {
                return false;
            }
            if (momJustCreated)
            {
                this.mHasRequestedWalkStyle = false;
            }
            mMom = mom;
            mDad = null;
            SimDescription simDescription = SimDescription.Find(DadDescriptionId);
            if (simDescription != null && simDescription.IsMale && simDescription.YoungAdultOrAbove)
            {
                mDad = simDescription.CreatedSim;
            }
            mTimeMoodSampled = SimClock.CurrentTime();
            mMom.MoodManager.MoodChanged += new MoodManager.MoodChangedCallback(MoodManager_MoodChanged);
            ActiveTopic.AddToSim(mom, "Pregnancy");
            if (momJustCreated)
            {
                PreggersAlarm = mom.AddAlarmRepeating(1f, TimeUnit.Hours, HourlyCallback, 1f, TimeUnit.Hours, "Hourly Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
            }
            mom.SimDescription.Pregnancy = this;
            if (mHourOfPregnancy >= kHourToShowPregnantBuff)
            {
                if (mHourOfPregnancy < kHourToStartContractions)
                {
                    mMom.BuffManager.AddElement(BuffNames.Pregnant, Origin.FromPregnancy);
                }
                ActiveTopic.AddToSim(mMom, "Announce Pregnancy");
                if (mDad != null && !mDad.HasBeenDestroyed)
                {
                    ActiveTopic.AddToSim(this.mDad, "Pregnant", mMom.SimDescription);
                }
            }
            if ((mUseFatness && mHourOfPregnancy >= kHourToShowPregnantBuff && mWeight >= (kFatnessMax * 0.5f)) || mHourOfPregnancy >= kHourToStartWalkingPregnant)
            {
                ActiveTopic.AddToSim(this.mMom, "Pregnant", mMom.SimDescription);
                RequestPregnantWalkStyle();
            }
            if (mHourOfPregnancy >= kHourToStartContractions)
            {
                mMom.BuffManager.AddElement(BuffNames.BabyIsComing, Origin.FromPregnancy);
                if (mContractionBroadcast != null)
                {
                    mContractionBroadcast.Dispose();
                }
                mContractionBroadcast = new ReactionBroadcaster(this.mMom, kContractionBroadcasterParams, new ReactionBroadcaster.BroadcastCallback(this.StartReaction), new ReactionBroadcaster.BroadcastCallback(this.CancelReaction));
                mMom.AddInteraction(TakeToHospital.Singleton);
                InteractionInstance entry = HaveContraction.Singleton.CreateInstance(mMom, mMom, new InteractionPriority(InteractionPriorityLevel.High, 10f), false, false);
                mMom.InteractionQueue.Add(entry);
                mContractionsAlarm = mMom.AddAlarmRepeating(5f, TimeUnit.Minutes, base.TriggerContraction, 5f, TimeUnit.Minutes, "Trigger Contractions Alarm", AlarmType.AlwaysPersisted);
            }
            if (mHourOfPregnancy == kHoursOfPregnancy)
            {
                mMom.AddAlarm(1f, TimeUnit.Minutes, new AlarmTimerCallback(this.HaveTheBaby), "Re-have the baby.", AlarmType.AlwaysPersisted);
            }
            SetPregoBlendShape();
            if (mMom.SimDescription.IsVisuallyPregnant)
            {
                TryToGiveLeave();
            }
            return true;
        }

        public override void HourlyCallback()
        {
            if (!IsPaused)
            {
                mHourOfPregnancy++;
                if (mHourOfPregnancy == kHourToStartPregnantMotives)
                {
                    mMom.BuffManager.AddElement(BuffNames.Nauseous, Origin.FromUnknown);
                }
                if (mHourOfPregnancy < kHourToShowPregnantBuff && mHourOfPregnancy > kHourToStartPregnantMotives && RandomUtil.RandomChance01(kChanceOfNausea))
                {
                    mMom.BuffManager.AddElement(BuffNames.Nauseous, Origin.FromUnknown);
                }
                if (mMom.Household.IsTouristHousehold)
                {
                    ForeignVisitorsSituation foreignVisitorsSituation = ForeignVisitorsSituation.TryGetForeignVisitorsSituation(mMom);
                    if (mHourOfPregnancy == kForeignSimDisplaysTNS && foreignVisitorsSituation != null)
                    {
                        StyledNotification.Format format = new StyledNotification.Format(Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:ForeignBabyIsComingTNS", mMom), StyledNotification.NotificationStyle.kGameMessagePositive);
                        StyledNotification.Show(format, "glb_tns_baby_coming_r2");
                    }
                    if (mHourOfPregnancy == kForeignSimLeavesWorld)
                    {
                        if (foreignVisitorsSituation != null)
                        {
                            foreignVisitorsSituation.MakeGuestGoHome(mMom);
                        }
                        else if (mMom.SimDescription.AssignedRole != null)
                        {
                            mMom.SimDescription.AssignedRole.RemoveSimFromRole();
                        }
                    }
                    if (mHourOfPregnancy > kForeignSimLeavesWorld)
                    {
                        mHourOfPregnancy--;
                        return;
                    }
                }
                if (mHourOfPregnancy == kHourToShowPregnantBuff)
                {
                    mOriginalWeight = mMom.SimDescription.Weight;
                    InteractionInstance interactionInstance = ShowPregnancy.Singleton.CreateInstance(mMom, mMom, new InteractionPriority(InteractionPriorityLevel.ESRB), isAutonomous: false, cancellableByPlayer: false);
                    interactionInstance.Hidden = true;
                    mMom.InteractionQueue.AddNext(interactionInstance);
                    return;
                }
                if ((mUseFatness && mHourOfPregnancy >= kHourToShowPregnantBuff && mWeight >= (kFatnessMax * 0.5f)) || mHourOfPregnancy >= kHourToStartWalkingPregnant)
                {
                    ActiveTopic.AddToSim(mMom, "Pregnant", mMom.SimDescription);
                    RequestPregnantWalkStyle();
                }
                if (mHourOfPregnancy == kHourToStartContractions)
                {
                    for (int i = 0; i < Pregnancy.kNumberOfPuddlesForWaterBreak; i++)
                    {
                        PuddleManager.AddPuddle(mMom.PositionOnFloor);
                    }
                    if (mMom.IsSelectable)
                    {
                        StyledNotification.Format format2 = new StyledNotification.Format(Localization.LocalizeString("Gameplay/ActorSystems/Pregnancy:BabyIsComingTNS", mMom), StyledNotification.NotificationStyle.kGameMessageNegative);
                        StyledNotification.Show(format2, "glb_tns_baby_coming_r2");
                    }
                    mMom.BuffManager.RemoveElement(BuffNames.Pregnant);
                    mMom.BuffManager.AddElement(BuffNames.BabyIsComing, Origin.FromPregnancy);
                    if (mContractionBroadcast != null)
                    {
                        mContractionBroadcast.Dispose();
                    }
                    mContractionBroadcast = new ReactionBroadcaster(mMom, kContractionBroadcasterParams, StartReaction, CancelReaction);
                    mMom.AddInteraction(TakeToHospital.Singleton);
                    InteractionInstance entry = HaveContraction.Singleton.CreateInstance(mMom, mMom, new InteractionPriority(InteractionPriorityLevel.High, 10f), isAutonomous: false, cancellableByPlayer: false);
                    mMom.InteractionQueue.Add(entry);
                    mContractionsAlarm = mMom.AddAlarmRepeating(5f, TimeUnit.Minutes, TriggerContraction, 5f, TimeUnit.Minutes, "Trigger Contractions Alarm", AlarmType.AlwaysPersisted);
                    EventTracker.SendEvent(EventTypeId.kPregnancyContractionsStarted, mMom);
                }
                if (mHourOfPregnancy == kHoursOfPregnancy)
                {
                    HaveTheBaby();
                }
                SetPregoBlendShape();
            }
        }

        public new void SetPregoBlendShape()
        {
            int num = mHourOfPregnancy - kHourToShowPregnantBuff;
            if (num >= 0)
            {
                num = Math.Min(num, Pregnancy.kHoursToShowFullyPregnant);
                float num2 = (float)num / (float)Pregnancy.kHoursToShowFullyPregnant;
                if (num2 == 0f)
                {
                    num2 = 0.01f;
                }
                mMom.SimDescription.SetPregnancy(num2, false);
                if (mUseFatness)
                {
                    SimDescription mom = mMom.SimDescription;
                    mWeight = (kFatnessMax - mOriginalWeight) * (float)Math.Sqrt(num2) + mOriginalWeight;
                    mom.SetBodyShape(mWeight, mom.Fitness);
                }
                if (mMom.SimDescription.TeenOrAbove && RandomUtil.RandomChance01(Pregnancy.kChanceOfBackache))
                {
                    this.mMom.BuffManager.AddElement(BuffNames.Backache, Origin.FromPregnancy);
                }
            }
        }

        public override List<Sim> CreateNewborns(float bonusMoodPoints, bool interactive, bool homeBirth)
        {
            if (mUseFatness)
            {
                SimDescription simDescription = this.mMom.SimDescription;
                if (this.mOriginalWeight > 0f)
                {
                    simDescription.mCurrentShape.Fat = this.mOriginalWeight;
                    simDescription.mCurrentShape.Thin = 0f;
                }
                else
                {
                    simDescription.mCurrentShape.Fat = 0f;
                    simDescription.mCurrentShape.Thin = -this.mOriginalWeight;
                }
            }
            List<Sim> result = base.CreateNewborns(bonusMoodPoints, interactive, homeBirth);
            if (mUseFatness)
            {
                SimDescription simDescription = this.mMom.SimDescription;
                simDescription.mCurrentShape.Fat = kFatnessMax;
                simDescription.mCurrentShape.Thin = 0f;
            }
            return result;
        }

        public new static void StartFromMerge(Sim woman, Pregnancy incomingPreg)
        {
            PregnancyEx p = incomingPreg as PregnancyEx;
            if (p != null)
            {
                p.MergeAndStart(woman);
            }
            else
            {
                PregnancyEx.StartFromMerge(woman, incomingPreg, new PregnancyEx(woman, null));
            }
        }

        public static Pregnancy Start(Sim woman, SimDescription man, bool handlePlantSim)
        {
            if (handlePlantSim)
            {
                if ((SimTypes.IsSelectable(woman)) || (SimTypes.IsSelectable(man)))
                {
                    if (woman.SimDescription.IsPlantSim || man.IsPlantSim)
                    {
                        IGameObject obj2 = GlobalFunctions.CreateObjectOutOfWorld("forbiddenFruit", ProductVersion.EP9, "Sims3.Gameplay.Objects.Gardening.ForbiddenFruit", null);
                        if (obj2 != null)
                        {
                            Inventories.TryToMove(obj2, woman);
                            Audio.StartSound("sting_baby_conception");
                        }

                        return null;
                    }
                }
            }

            AgingManager.Singleton.CancelAgingAlarmsForSim(woman.SimDescription.AgingState);
            Pregnancy p = null;

            if (woman.IsHuman)
            {
                p = new PregnancyEx(woman, man);
            }
            else
            {
                p = new PetPregnancy(woman, man);
            }

            p.PreggersAlarm = woman.AddAlarmRepeating(1f, TimeUnit.Hours, p.HourlyCallback, 1f, TimeUnit.Hours, "Hourly Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
            woman.SimDescription.Pregnancy = p;

            EventTracker.SendEvent(new PregnancyEvent(EventTypeId.kGotPregnant, woman, man.CreatedSim, p, null));
            return p;
        }
    }
}