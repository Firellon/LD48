namespace Day
{
    public interface IDayNightCycle
    {
        DayTime GetCurrentCycle();
        
        float TargetIntensity { get; }
        int CurrentDay { get; }
        DayTime CurrentCycle { get; }
        float CurrentCycleLength { get; }
    }
}