using NRaas.CommonSpace.Helpers;
using NRaas.PregnancyControllerSpace.Helpers;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using System;

namespace NRaas.PregnancyControllerSpace.Interactions
{
    public class HaveBabyHospitalEx : Pregnancy.HaveBabyHospital, Common.IPreLoad, Common.IAddInteraction
    {
        public new class Definition : Pregnancy.HaveBabyHospital.Definition
        {
            public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop)
            {
                return base.GetInteractionName(actor, target, new InteractionObjectPair(sOldSingleton, target));
            }

            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                InteractionInstance result = new HaveBabyHospitalEx();
                result.Init(ref parameters);
                return result;
            }
        }

        static InteractionDefinition sOldSingleton;

        public void AddInteraction(Common.InteractionInjectorList interactions)
        {
            interactions.Replace<Sim, Pregnancy.HaveBabyHospital.Definition>(Singleton);
        }

        public void OnPreLoad()
        {
            Tunings.Inject<Sim, Pregnancy.HaveBabyHospital.Definition, Definition>(false);

            sOldSingleton = Singleton;
            Singleton = new Definition();
        }

        public override bool InRabbitHole()
        {
            bool hasOriginalWeight = false;
            float originalWeight = 0f;
            PregnancyEx pregancy = this.Actor.SimDescription.Pregnancy as PregnancyEx;
            if (pregancy != null)
            {
                if (pregancy.UseFatness)
                {
                    hasOriginalWeight = true;
                    originalWeight = pregancy.OriginalWeight;
                }
            }

            bool result = base.InRabbitHole();
            if (result == true && hasOriginalWeight == true)
            {
                this.Actor.SimDescription.SetBodyShape(originalWeight, this.Actor.SimDescription.Fitness);
            }
            return result;
        }
    }
}