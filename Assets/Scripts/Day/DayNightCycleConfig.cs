using UnityEngine;

namespace Day
{
    [CreateAssetMenu(menuName = "LD48/Create DayNightCycleConfig", fileName = "DayNightCycleConfig", order = 0)]
    public class DayNightCycleConfig : ScriptableObject, IDayNightCycleConfig
    {
        [SerializeField] private DayTimeToFloatDictionary dayTimeToLengthSeconds = new()
        {
            {DayTime.Day, 60},
            {DayTime.NightComing, 15},
            {DayTime.Night, 60},
            {DayTime.DayComing, 15},
        };
        
        [SerializeField] private DayTimeToStringDictionary dayTimeMessages = new()
        {
            {DayTime.DayComing,"The Night has passed.\n Dawn of Day {0}"},
            {DayTime.Day, "The Day grows in power..."},
            {DayTime.NightComing, "The Sunset approaches..."},
            {DayTime.Night, "The Night arrived!\n Hide and cower, for its terrors are upon you!"},
        };

        public DayTimeToFloatDictionary DayTimeToLengthSeconds => dayTimeToLengthSeconds;
        public DayTimeToStringDictionary DayTimeMessages => dayTimeMessages;
    }
}