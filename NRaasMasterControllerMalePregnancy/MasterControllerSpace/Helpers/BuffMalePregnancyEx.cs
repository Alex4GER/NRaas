using NRaas.CommonSpace.Helpers;
using NRaas.CommonSpace.Options;
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

namespace NRaas.MasterControllerSpace.Helpers
{
	public class BuffMalePregnancyEx : BuffMalePregnancy
	{
		private static BuffInstance sBuffInstance = null;

		public static bool AddBuff(Sim sim, Origin origin)
		{
			if (sBuffInstance != null)
			{
				return sim.BuffManager.AddBuff(sBuffInstance, BuffNames.MalePregnancy, int.MaxValue, float.MaxValue, false, MoodAxis.None, origin, true, false);
			}
			else
			{
				return false;
			}
		}

		public class BuffInstanceMalePregnancyEx : BuffMalePregnancy.BuffInstanceMalePregnancy, Common.IWorldLoadFinished
		{
			private DateAndTime mLastTimeUpdated;

			public void OnWorldLoadFinished()
			{
				if (sBuffInstance == null)
				{
					BuffInstance baseInstance = null;
					GenericManager<BuffNames, BuffInstance, BuffInstance>.sDictionary.TryGetValue((ulong)BuffNames.MalePregnancy, out baseInstance);
					if (baseInstance != null)
					{
						sBuffInstance = new BuffMalePregnancyEx(baseInstance.mBuff.mInfo).CreateBuffInstance();
					}
				}
			}

			public BuffInstanceMalePregnancyEx()
			{
			}

			public BuffInstanceMalePregnancyEx(Buff buff, BuffNames buffGuid, int effectValue, float timeoutCount)
				: base(buff, buffGuid, effectValue, timeoutCount)
			{
			}

			public override BuffInstance Clone()
			{
				return new BuffInstanceMalePregnancyEx(mBuff, mBuffGuid, mEffectValue, mTimeoutCount);
			}

			public override void OnTimeOutUpdated()
			{
				if (base.TimeoutCount > 1f)
				{
					if (SimClock.ElapsedTime(TimeUnit.Minutes, mLastTimeUpdated) >= (float)60)
					{
						if (RandomUtil.RandomChance01(Pregnancy.kChanceOfBackache))
						{
							Sim createdSim = TargetSim.CreatedSim;
							createdSim.AddAlarm(1f, TimeUnit.Seconds, () => createdSim.BuffManager.AddElement(BuffNames.Backache, Origin.FromPregnancy), "Add backage", AlarmType.DeleteOnReset);

						}
						mLastTimeUpdated = SimClock.CurrentTime();
					}
				}
				base.OnTimeOutUpdated();
			}
		}

		public override BuffInstance CreateBuffInstance()
		{
			return new BuffInstanceMalePregnancyEx(this, base.BuffGuid, base.EffectValue, base.TimeoutSimMinutes);
		}

		public BuffMalePregnancyEx(BuffData info)
			: base(info)
		{
		}
	}
}