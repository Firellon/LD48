using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
    public static class TilemapLineDrawer
    {
        public static void DrawLine(this Tilemap tilemap, RuleTile ruleTile, Vector2Int start, Vector2Int end)
        {
            // tilemap.SetTile(new TileChangeData(start, ruleTile, new Color(1,1,1,1)), true);
            tilemap.SetTile((Vector3Int)start, ruleTile);

            //Grid cells are 1.0 X 1.0.
            var x = Mathf.Floor(start.x);
            var y = Mathf.Floor(start.y);

            var diffX = end.x - start.x;
            var diffY = end.y - start.y;

            var stepX = Mathf.Sign(diffX);
            var stepY = Mathf.Sign(diffY);
  
            //Ray/Slope related maths.
            //Straight distance to the first vertical grid boundary.
            var xOffset = end.x > start.x ?
                (Mathf.Ceil(start.x) - start.x) :
                (start.x - Mathf.Floor(start.x));
            //Straight distance to the first horizontal grid boundary.
            var yOffset = end.y > start.y ?
                (Mathf.Ceil(start.y) - start.y) :
                (start.y - Mathf.Floor(start.y));
            //Angle of ray/slope.
            var angle = Mathf.Atan2(-diffY, diffX);
            //NOTE: These can be divide by 0's, but JS just yields Infinity! :)
            //How far to move along the ray to cross the first vertical grid cell boundary.
            var tMaxX = xOffset / Mathf.Cos(angle);
            //How far to move along the ray to cross the first horizontal grid cell boundary.
            var tMaxY = yOffset / Mathf.Sin(angle);
            //How far to move along the ray to move horizontally 1 grid cell.
            var tDeltaX = 1f / Mathf.Cos(angle);
            //How far to move along the ray to move vertically 1 grid cell.
            var tDeltaY = 1f / Mathf.Sin(angle);
  
            //Travel one grid cell at a time.
            var manhattanDistance = Mathf.Abs(Mathf.Floor(end.x) - Mathf.Floor(start.x)) +
                                    Mathf.Abs(Mathf.Floor(end.y) - Mathf.Floor(start.y));

            for (var t = 0; t <= manhattanDistance; ++t) {
                // drawSquare(x, y);
                tilemap.SetTile(new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), 0), ruleTile);
                //Only move in either X or Y coordinates, not both.
                if (Mathf.Abs(tMaxX) < Mathf.Abs(tMaxY)) {
                    tMaxX += tDeltaX;
                    x += stepX;
                } else {
                    tMaxY += tDeltaY;
                    y += stepY;
                }
            }
        }
    }
}