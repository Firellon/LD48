namespace Day
{
    public class DayNightCycleUpdatedSignal
    {
        public DayTime DayCycle { get; }
        public int CurrentDay { get; }
        public DayNightCycleUpdatedSignal(DayTime newDayCycle, int currentDay)
        {
            DayCycle = newDayCycle;
            CurrentDay = currentDay;
        }
    }
}