using UnityEngine;

namespace Environment.Signals
{
    public class MapWaypointActivatedSignal
    {
        public Vector3 WaypointPosition { get; }

        public MapWaypointActivatedSignal(Vector3 position)
        {
            WaypointPosition = position;
        }
    }
}