using Sims3.Gameplay;
using Sims3.Gameplay.Utilities;

namespace NRaas.LoadingScreenExtensionSpace.Helpers
{
    public class TravelArrivalHelper : Common.IWorldLoadFinished
    {
        public void OnWorldLoadFinished()
        {
            if (GameStates.IsTravelling && !GameStates.TravellingHome)
            {
                if (!Gameflow.sGameLoadedFromWorldFile && !TravellingLoadingScreenHelper.VacationWorldNames.Contains(GameStates.DestinationTravelWorld))
                {
                    float time = SimClock.HoursUntil(SimClockUtils.kInitialTimeOfDay);
                    long num = SimClock.ConvertToTicks(time, TimeUnit.Hours);
                    SimClock.TicksAdvanced += num;
                    AlarmManager.FixLoadAlarms(SimClock.CurrentTicks + num);
                }
            }
        }
    }
}
