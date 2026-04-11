using NRaas.CommonSpace.Helpers;
using NRaas.MasterControllerSpace.Helpers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using Sims3.UI;
using System;

namespace NRaas.MasterControllerSpace.Sims.Basic
{
	public class StartMalePregnancy : SimFromList, IBasicOption
	{
		public class ShowPregnancy : Interaction<Sim, Sim>
		{
			[DoesntRequireTuning]
			public class Definition : InteractionDefinition<Sim, Sim, ShowPregnancy>
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
				BuffMalePregnancyEx.ShowPregnancy(Actor.SimDescription);
				Actor.BuffManager.AddElement(BuffNames.MalePregnancy, Origin.FromPregnancy);
				return true;
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
			else if (!man.SimDescription.YoungAdultOrAdult)
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
