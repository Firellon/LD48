using UnityEngine;
using Utilities;

namespace Equipment
{
    public class FlashlightController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 5f;

        [SerializeField] private Transform flashlight;

        private void LateUpdate()
        {
            var pointerPos = GetPointerInput();
            var direction = pointerPos - (Vector2)transform.position;

            flashlight.rotation = Quaternion.Lerp(flashlight.rotation, direction.ToQuaternion(), Time.deltaTime * rotationSpeed);            
        }

        public Vector2 GetPointerInput()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }
    }
}