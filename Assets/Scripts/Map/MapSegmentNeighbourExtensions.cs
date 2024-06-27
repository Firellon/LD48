using System;
using UnityEngine;

namespace Map
{
    public static class MapSegmentNeighbourExtensions
    {
        public static Vector2Int GetPosition(this MapSegmentNeighbour neighbour)
        {
            return neighbour switch
            {
                MapSegmentNeighbour.None => new Vector2Int(0, 0),
                MapSegmentNeighbour.Top => new Vector2Int(0, 1),
                MapSegmentNeighbour.TopRight => new Vector2Int(1, 1),
                MapSegmentNeighbour.Right => new Vector2Int(1, 0),
                MapSegmentNeighbour.BottomRight => new Vector2Int(1, -1),
                MapSegmentNeighbour.Bottom => new Vector2Int(0, -1),
                MapSegmentNeighbour.BottomLeft => new Vector2Int(-1, -1),
                MapSegmentNeighbour.Left => new Vector2Int(-1, 0),
                MapSegmentNeighbour.TopLeft => new Vector2Int(-1, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(neighbour), neighbour, null)
            };
        }

        public static MapSegmentNeighbour GetOpposite(this MapSegmentNeighbour neighbour)
        {
            return neighbour switch
            {
                MapSegmentNeighbour.None => MapSegmentNeighbour.None,
                MapSegmentNeighbour.Top => MapSegmentNeighbour.Bottom,
                MapSegmentNeighbour.TopRight => MapSegmentNeighbour.BottomLeft,
                MapSegmentNeighbour.Right => MapSegmentNeighbour.Left,
                MapSegmentNeighbour.BottomRight => MapSegmentNeighbour.TopLeft,
                MapSegmentNeighbour.Bottom => MapSegmentNeighbour.Top,
                MapSegmentNeighbour.BottomLeft => MapSegmentNeighbour.TopRight,
                MapSegmentNeighbour.Left => MapSegmentNeighbour.Right,
                MapSegmentNeighbour.TopLeft => MapSegmentNeighbour.BottomRight,
                _ => throw new ArgumentOutOfRangeException(nameof(neighbour), neighbour, $"GetOpposite > opposite for {neighbour} not found!")
            };
        }
    }
}