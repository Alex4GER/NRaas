using NRaas.CommonSpace.Helpers;
using NRaas.PregnancyControllerSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;

namespace NRaas.MasterControllerSpace.Interactions
{
    public class MaleHaveBabyHomeEx : AlienUtils.MaleHaveBabyHome, Common.IPreLoad, Common.IAddInteraction
    {
        public new class Definition : AlienUtils.MaleHaveBabyHome.Definition
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldMaleHaveBabyHomeSingleton, target));
            }

            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                InteractionInstance result = new MaleHaveBabyHomeEx();
                result.Init(ref parameters);
                return result;
            }
        }

        static InteractionDefinition sOldMaleHaveBabyHomeSingleton;

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Replace<Sim, AlienUtils.MaleHaveBabyHome.Definition>(MaleHaveBabyHomeSingleton);
        }

        public void OnPreLoad()
        {
            InteractionTuning tuning = Tunings.Inject<Sim, AlienUtils.MaleHaveBabyHome.Definition, Definition>(false);

            sOldMaleHaveBabyHomeSingleton = MaleHaveBabyHomeSingleton;
            MaleHaveBabyHomeSingleton = new Definition();
        }

        public override bool Run()
        {
            if (this.Actor.LotCurrent != this.Target)
            {
                Vector3 vector = World.LotGetPtInside(this.Target.LotId);
                if (vector == Vector3.Invalid)
                {
                    return false;
                }
                if (!this.Actor.RouteToPointRadius(vector, 3f) && (!GlobalFunctions.PlaceAtGoodLocation(this.Actor, new World.FindGoodLocationParams(vector), false) || !this.Actor.IsPointInLotSafelyRoutable(this.Target, this.Actor.Position)))
                {
                    this.Actor.AttemptToPutInSafeLocation(true);
                }
            }
            if (this.Actor.Posture is SwimmingInPool)
            {
                SwimmingInPool swimmingInPool = this.Actor.Posture as SwimmingInPool;
                if (!swimmingInPool.ContainerPool.RouteToEdge(this.Actor))
                {
                    if (this.Actor.BridgeOrigin != null)
                    {
                        this.Actor.BridgeOrigin.MakeRequest();
                    }
                    this.Actor.PopPosture();
                    IGameObject gameObject = null;
                    this.Actor.FindRoutablePointInsideNearFrontDoor(this.Actor.Household.LotHome, out gameObject);
                    Vector3 position = gameObject.Position;
                    Terrain.TeleportMeHere teleportMeHere = Terrain.TeleportMeHere.Singleton.CreateInstance(Terrain.Singleton, this.Actor, new InteractionPriority(InteractionPriorityLevel.Pregnancy), false, false) as Terrain.TeleportMeHere;
                    teleportMeHere.SetAndReserveDestination(gameObject);
                    try
                    {
                        teleportMeHere.RunInteractionWithoutCleanup();
                    }
                    catch
                    {
                        this.Actor.SetPosition(position);
                    }
                    finally
                    {
                        teleportMeHere.Cleanup();
                    }
                    this.Actor.LoopIdle();
                }
            }
            this.GetNewBorns();
            this.AcquirePregnancyStateMachine();
            this.mCurrentStateMachine.SetActor("x", this.Actor);
            for (int i = 0; i < this.mNewborns.Count; i++)
            {
                Sim sim = this.mNewborns[i];
                Relationship relationship = Relationship.Get(this.Actor, sim, true);
                relationship.LTR.ForceChangeState(LongTermRelationshipTypes.Friend);
                if (sim.BridgeOrigin != null)
                {
                    sim.BridgeOrigin.MakeRequest();
                    sim.BridgeOrigin = null;
                }
                MalePregnancy.InternalMaleHaveBabyHome internalHaveBabyHome = MalePregnancy.InternalMaleHaveBabyHome.InternalMaleHaveBabyHomeSingleton.CreateInstance(sim, this.Actor, base.GetPriority(), base.Autonomous, base.CancellableByPlayer) as MalePregnancy.InternalMaleHaveBabyHome;
                internalHaveBabyHome.TotalCount = this.mNewborns.Count;
                internalHaveBabyHome.BabyIndex = i + 1;
                MalePregnancy p = this.Actor.SimDescription.Pregnancy as MalePregnancy;
                if (p != null)
                {
                    internalHaveBabyHome.OriginalWeight = p.OriginalWeight;
                    internalHaveBabyHome.UseFatness = p.UseFatness;
                }
                internalHaveBabyHome.mCurrentStateMachine = this.mCurrentStateMachine;
                this.Actor.InteractionQueue.PushAsContinuation(internalHaveBabyHome, true);
            }
            this.TryDeliverImaginaryFriend();
            return true;
        }

        public override void GetNewBorns()
        {
            this.mNewborns = new List<Sim>();
            Sims3.Gameplay.Gameflow.Singleton.DisableSave(this, "Gameplay/ActorSystems/Pregnancy:DisableSave");
            bool flag = PregnancyController.Settings.mKeepInactiveAlienBaby;
            if (/*this.Actor.Household.NumActorMembersCountingPregnancy < 8 && */this.Actor.IsSelectable)
            {
                flag = TwoButtonDialog.Show(AlienUtils.LocalizeString("MalePregnancyConfirmationDialog", new object[] {
                    this.Actor
                }), AlienUtils.LocalizeString("MalePregnancyConfirmationDialogAccept", new object[0]), AlienUtils.LocalizeString("MalePregnancyConfirmationDialogReject", new object[0]));
            }
            else
            {
                this.Actor.ShowTNSIfSelectable(AlienUtils.LocalizeString("MalePregnancyHouseholdFullTNS", new object[] {
                    this.Actor
                }), StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, this.Actor.ObjectId);
            }
            CASAgeGenderFlags gender = RandomUtil.CoinFlip() ? CASAgeGenderFlags.Male : CASAgeGenderFlags.Female;
            SimDescription simDescription = SimDescription.Find(this.AlienParentID);
            SimDescription simDescription2 = Genetics.MakeAlien(CASAgeGenderFlags.Baby, gender, GameUtils.GetCurrentWorld(), 1f, false);
            simDescription2.LastName = this.Actor.LastName;
            if (flag)
            {
                simDescription2.FirstName = string.Empty;
            }
            Genetics.AssignTraits(simDescription2, null, this.Actor.SimDescription, flag, (float)this.Actor.MoodManager.MoodValue, new Random());
            this.Actor.Genealogy.AddChild(simDescription2.Genealogy);
            if (simDescription != null)
            {
                simDescription.Genealogy.AddChild(simDescription2.Genealogy);
            }
            if (flag)
            {
                this.Actor.Household.Add(simDescription2);
            }
            else
            {
                Household.AlienHousehold.Add(simDescription2);
            }
            Sim sim = simDescription2.Instantiate(Vector3.Empty);
            sim.GreetSimOnLot(this.Actor.LotCurrent);
            sim.SetPosition(this.Actor.Position);
            Pregnancy.TotallyHideBaby(sim);
            this.mNewborns.Add(sim);
        }

        public override void Cleanup()
        {
            if (!this.Actor.HasBeenDestroyed)
            {
                this.Actor.SimDescription.Pregnancy.PregnancyComplete(null, null);
            }
            base.Cleanup();
        }
    }
}