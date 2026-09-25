using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;

namespace NRaas.PregnancyControllerSpace.Helpers
{
    public class MalePregnancy : PregnancyEx
    {
        public class MaleShowPregnancy : Pregnancy.ShowPregnancy
        {
            public new class Definition : Pregnancy.ShowPregnancy.Definition
            {
                public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
                {
                    InteractionInstance result = new MaleShowPregnancy();
                    result.Init(ref parameters);
                    return result;
                }
            }

            public static InteractionDefinition MaleShowPregnancySingleton = new Definition();

            public override bool Run()
            {
                SimDescription simDescription = this.Actor.SimDescription;
                simDescription.Pregnancy.mHasShownPregnancy = true;
                if (Actor.IsSelectable)
                {
                    PlumbBob.SelectActor(Actor);
                    Camera.FocusOnSim(Actor, Pregnancy.kShowPregnancyCameraLerp.Zoom, Pregnancy.kShowPregnancyCameraLerp.Pitch, Pregnancy.kShowPregnancyCameraLerp.Time, true, false);
                    Audio.StartSound("sting_lifetime_opp_success");
                }
                Actor.SimDescription.ShowPregnancy();
                Actor.BuffManager.AddElement(BuffNames.MalePregnancy, Origin.FromPregnancy);
                if (Actor.BuffManager.HasElement(BuffNames.Nauseous))
                {
                    Actor.BuffManager.GetElement(BuffNames.Nauseous).mBuffOrigin = Origin.FromPregnancy;
                }
                ActiveTopic.AddToSim(this.Actor, "Announce Pregnancy");
                if ((Actor.IsSelectable && (GameUtils.IsOnVacation() || GameUtils.IsUniversityWorld()) == false))
                {
                    Pregnancy p = simDescription.Pregnancy;
                    if (p != null)
                    {
                        p.TryToGiveLeave();
                    }
                    Occupation occupation = Actor.Occupation;
                    if (occupation != null)
                    {
                        occupation.RemoveAllJobs();
                    }
                }
                Actor.PlaySoloAnimation("a_alien_pregnancy_inspectStomach_x", ProductVersion.EP8);
                return true;
            }
        }

        public class InternalMaleHaveBabyHome : Pregnancy.InternalHaveBabyHome
        {
            public new class Definition : Pregnancy.InternalHaveBabyHome.Definition
            {
                public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
                {
                    InteractionInstance result = new InternalMaleHaveBabyHome();
                    result.Init(ref parameters);
                    return result;
                }
            }

            public float OriginalWeight = 0f;

            public bool UseFatness = false;

            public static InteractionDefinition InternalMaleHaveBabyHomeSingleton = new Definition();

            public override void Cleanup()
            {
                if (UseFatness)
                {
                    this.Actor.SimDescription.SetBodyShape(OriginalWeight, this.Actor.SimDescription.Fitness);
                }
                this.Actor.SimDescription.SetPregnancy(0f);
                base.Cleanup();
            }
        }

        BuffMalePregnancyEx.BuffInstanceMalePregnancy mBuffInstance;

        public MalePregnancy()
        {
        }

        public MalePregnancy(Sim man)
            : base(man, null)
        {
            SimDescription simDescription = mMom.SimDescription;
            List<SimDescription> list = new List<SimDescription>();
            foreach (SimDescription current in Household.AlienHousehold.SimDescriptions)
            {
                if (!simDescription.IsBloodRelated(current) && current.YoungAdultOrAbove && ((current.IsFemale && simDescription.CanAutonomouslyBeRomanticWithFemales) || (current.IsMale && simDescription.CanAutonomouslyBeRomanticWithMales)))
                {
                    list.Add(current);
                }
            }
            if (list.Count > 0)
            {
                DadDescriptionId = RandomUtil.GetRandomObjectFromList<SimDescription>(list).SimDescriptionId;
                simDescription = SimDescription.Find(DadDescriptionId);
                if (simDescription != null && simDescription.IsMale && simDescription.YoungAdultOrAbove)
                {
                    mDad = simDescription.CreatedSim;
                }
            }
        }

        public new float OriginalWeight
        {
            get
            {
                return mOriginalWeight;
            }
            set
            {
                mOriginalWeight = value;
            }
        }

        public BuffMalePregnancyEx.BuffInstanceMalePregnancy BuffInstance
        {
            get
            {
                return mBuffInstance;
            }
            set
            {
                mBuffInstance = value;
            }
        }

        public override void MergeAndStart(Sim woman)
        {
            MalePregnancy malePregnancy = new MalePregnancy(woman);
            StartFromMerge(woman, this, malePregnancy);
        }

        public override bool Continue(Sim mom, bool momJustCreated)
        {
            if (mom == null)
            {
                return false;
            }
            mMom = mom;
            mHasRequestedWalkStyle = false;
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
                mMom.BuffManager.AddElement(BuffNames.MalePregnancy, Origin.FromPregnancy);
                ActiveTopic.AddToSim(mMom, "Announce Pregnancy");
            }
            if (mHourOfPregnancy == kHoursOfPregnancy)
            {
                mMom.AddAlarm(1f, TimeUnit.Minutes, new AlarmTimerCallback(this.HaveTheBaby), "Re-have the baby.", AlarmType.AlwaysPersisted);
            }
            SetPregoBlendShape();
            if (mMom.SimDescription.IsVisuallyPregnant)
            {
                if ((mUseFatness && mWeight >= (kFatnessMax * 0.5f)) || mHourOfPregnancy >= kHourToStartWalkingPregnant)
                {
                    ActiveTopic.AddToSim(this.mMom, "Pregnant", mMom.SimDescription);
                    RequestPregnantWalkStyle();
                }
                TryToGiveLeave();
            }
            return true;
        }

        public override void ClearPregnancyData()
        {
            this.mMom.BuffManager.RemoveElement(BuffNames.MalePregnancy);
            base.ClearPregnancyData();
        }

        public override void PregnancyComplete(List<Sim> newborns, List<Sim> followers)
        {
            this.UnrequestPregnantWalkStyle();
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
            this.mMom.SimDescription.NullOutPregnancy();
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
                if (mHourOfPregnancy == kHourToShowPregnantBuff)
                {
                    OriginalWeight = mMom.SimDescription.Weight;
                    InteractionInstance interactionInstance = MaleShowPregnancy.MaleShowPregnancySingleton.CreateInstance(mMom, mMom, new InteractionPriority(InteractionPriorityLevel.ESRB), isAutonomous: false, cancellableByPlayer: false);
                    interactionInstance.Hidden = true;
                    mMom.InteractionQueue.AddNext(interactionInstance);
                    return;
                }
                if ((mUseFatness && mHourOfPregnancy >= kHourToShowPregnantBuff && mWeight >= (kFatnessMax * 0.5f)) || mHourOfPregnancy >= kHourToStartWalkingPregnant)
                {
                    ActiveTopic.AddToSim(this.mMom, "Pregnant", this.mMom.SimDescription);
                    RequestPregnantWalkStyle();
                }
                if (mHourOfPregnancy == kHoursOfPregnancy)
                {
                    HaveTheBaby();
                }
                SetPregoBlendShape();
            }
        }

        public override void HaveTheBaby()
        {
            mMom.RemoveAlarm(this.PreggersAlarm);
            InteractionInstance interactionInstance = AlienUtils.MaleHaveBabyHome.MaleHaveBabyHomeSingleton.CreateInstance(mMom.LotHome, mMom, new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false);
            (interactionInstance as AlienUtils.MaleHaveBabyHome).AlienParentID = this.DadDescriptionId;
            mMom.InteractionQueue.AddNext(interactionInstance);
        }

        public static MalePregnancy Start(Sim man)
        {
            AgingManager.Singleton.CancelAgingAlarmsForSim(man.SimDescription.AgingState);
            MalePregnancy p = null;
            if (man.IsHuman && man.IsMale)
            {
                p = new MalePregnancy(man);
            }
            p.PreggersAlarm = man.AddAlarmRepeating(1f, TimeUnit.Hours, p.HourlyCallback, 1f, TimeUnit.Hours, "Hourly Pregnancy Update Alarm", AlarmType.AlwaysPersisted);
            man.SimDescription.Pregnancy = p;

            return p;
        }
    }
}