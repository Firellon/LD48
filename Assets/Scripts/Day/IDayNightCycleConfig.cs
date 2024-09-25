namespace Day
{
    public interface IDayNightCycleConfig
    {
        DayTimeToFloatDictionary DayTimeToLengthSeconds { get; }
        DayTimeToStringDictionary DayTimeMessages { get; }
    }
}