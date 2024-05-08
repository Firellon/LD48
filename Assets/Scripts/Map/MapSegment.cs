using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [Serializable]
    public class MapSegment
    {
        public Vector2Int Coordinates { get; set; }
        public List<GameObject> StaticObjects { get; set; }
    }
}