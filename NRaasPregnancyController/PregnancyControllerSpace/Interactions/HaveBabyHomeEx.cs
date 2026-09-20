using NRaas.CommonSpace.Helpers;
using NRaas.PregnancyControllerSpace.Helpers;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Pools;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI.Controller;
using System;

namespace NRaas.MasterControllerSpace.Interactions
{
    public class HaveBabyHomeEx : Pregnancy.HaveBabyHome, Common.IPreLoad, Common.IAddInteraction
    {
        public new class Definition : Pregnancy.HaveBabyHome.Definition
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldSingleton, target));
            }

            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                InteractionInstance result = new HaveBabyHomeEx();
                result.Init(ref parameters);
                return result;
            }
        }

        static InteractionDefinition sOldSingleton;

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Replace<Sim, Pregnancy.HaveBabyHome.Definition>(Singleton);
        }

        public void OnPreLoad()
        {
            Tunings.Inject<Sim, Pregnancy.HaveBabyHome.Definition, Definition>(false);

            sOldSingleton = Singleton;
            Singleton = new Definition();
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
            GetNewBorns();
            AcquirePregnancyStateMachine();
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
                PregnancyEx.InternalHaveBabyHome internalHaveBabyHome = PregnancyEx.InternalHaveBabyHome.Singleton.CreateInstance(sim, this.Actor, base.GetPriority(), base.Autonomous, base.CancellableByPlayer) as PregnancyEx.InternalHaveBabyHome;
                internalHaveBabyHome.TotalCount = this.mNewborns.Count;
                internalHaveBabyHome.BabyIndex = i + 1;
                internalHaveBabyHome.mCurrentStateMachine = this.mCurrentStateMachine;
                PregnancyEx p = this.Actor.SimDescription.Pregnancy as PregnancyEx;
                if (p != null)
                {
                    internalHaveBabyHome.OriginalWeight = p.OriginalWeight;
                    internalHaveBabyHome.UseFatness = p.UseFatness;
                }
                this.Actor.InteractionQueue.PushAsContinuation(internalHaveBabyHome, true);
            }
            TryDeliverImaginaryFriend();
            return true;
        }

        public override void Cleanup()
        {
            if (this.Actor.IsMale)
            {
                if (this.mNewborns != null)
                {
                    if (!this.Actor.HasBeenDestroyed)
                    {
                        this.Actor.SimDescription.Pregnancy.PregnancyComplete(this.mNewborns, null);
                    }
                }
                else
                {
                    this.Actor.AddAlarm(1f, TimeUnit.Minutes, new AlarmTimerCallback(this.Actor.SimDescription.Pregnancy.HaveTheBaby), "Re-have the baby.", AlarmType.AlwaysPersisted);
                }
            }
            base.Cleanup();
        }

        public override void AcquirePregnancyStateMachine()
        {
            if (Actor.IsMale)
            {
                this.mCurrentStateMachine = StateMachineClient.Acquire(this.Actor, "MalePregnancy");
            }
            else
            {
                this.mCurrentStateMachine = StateMachineClient.Acquire(this.Actor, "Pregnancy");
            }
        }
    }
}