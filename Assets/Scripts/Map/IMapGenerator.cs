using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public interface IMapGenerator
    {
        List<Vector2Int> PotentialKeySegments { get; }
        List<Vector2Int> PotentialExitSegments { get; }
        Vector2Int ConvertWorldPositionToSegmentPosition(Vector3 worldPosition);
        Vector3 ConvertSegmentPositionToWorldPosition(Vector2Int segmentPosition);
        bool KeyHasBeenSpawned { get; }
    }
}