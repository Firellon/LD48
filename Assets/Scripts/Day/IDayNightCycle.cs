namespace Day
{
    public interface IDayNightCycle
    {
        DayTime GetCurrentCycle();
        
        float TargetIntensity { get; }
    }
}