using UnityEngine;

namespace LD48
{
    public class PositionClamp: MonoBehaviour
    {
        private Vector2 levelSize;

        void Start()
        {
            var terrainGenerator = Camera.main.GetComponent<TerrainGenerator>();
            levelSize = terrainGenerator.levelSize;
        }
        void LateUpdate() {
            Vector3 pos = transform.position;

            // assuming map starts at (0, 0)
            pos.x = Mathf.Max(Mathf.Min(pos.x, levelSize.x + 5), 0 - 5);
            pos.y = Mathf.Max(Mathf.Min(pos.y, levelSize.y + 5), 0 - 5);

            // setting the transform position. Consider using local position when possible
            transform.position = pos;
        }
    }
}