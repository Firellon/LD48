using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Utilities.Monads;

namespace Player.Compass
{
    public class PlayerCompassView : MonoBehaviour
    {
        [SerializeField] private Image compassBackground;
        [SerializeField] private Image compassHand;
        [SerializeField] private float defaultCompassHandRotation = 45f;

        [SerializeField] private float targetHandRotationSpeed = 1f;
        [SerializeField] private float minimalTargetDistance = 10f;
        [SerializeField] private double angleEpsilon = 1f;
        public Image CompassBackground => compassBackground;
        public Image CompassHand => compassHand;

        private float CompassHandRotation
        {
            get => defaultCompassHandRotation - compassHand.rectTransform.eulerAngles.z;
            set =>
                compassHand.rectTransform.eulerAngles =
                    new Vector3(0, 0, defaultCompassHandRotation - value);
        }

        private IMaybe<Vector3> compassTarget = Maybe.Empty<Vector3>();

        [ShowInInspector, ReadOnly] public IMaybe<Vector3> CompassTarget
        {
            set
            {
                compassTarget = value;
                Debug.Log($"CompassTarget is > {value.ValueOrDefault()}");
            }
        }

        private IMaybe<PlayerController> maybePlayer;

        public IMaybe<PlayerController> MaybePlayer
        {
            set => maybePlayer = value;
        }
        
        private static float ToAngle(Vector3 vector)
        {
            return Vector3.SignedAngle(vector, Vector3.right, Vector3.forward);
        }

        private void Update()
        {
            if (compassTarget == null || maybePlayer == null) return;
            
            compassTarget.IfPresent(target =>
            {
                maybePlayer.IfPresent(player =>
                {
                    var targetVector = (target - player.transform.position);
                    if (targetVector.magnitude < minimalTargetDistance)
                    {
                        CompassHandRotation += targetHandRotationSpeed;
                        return;
                    }
                    var compassHandVector = new Vector3(
                        -Mathf.Cos(CompassHandRotation * Mathf.Deg2Rad), 
                        Mathf.Sin(CompassHandRotation * Mathf.Deg2Rad), 
                        0);
                    var targetRotationAngle = Vector3.SignedAngle(compassHandVector, targetVector, Vector3.forward);

                    if (Mathf.Abs(targetRotationAngle) > angleEpsilon && Mathf.Abs(180 - Mathf.Abs(targetRotationAngle)) > angleEpsilon)
                    {
                        Debug.Log($"Update > player.transform.position from {compassHandVector} to {targetVector} => {targetRotationAngle}");
                        CompassHandRotation += targetHandRotationSpeed * Mathf.Sign(targetRotationAngle);   
                    }
                }).IfNotPresent(() =>
                {
                    CompassHandRotation = 0;
                });
            }).IfNotPresent(() =>
            {
                CompassHandRotation = 0;
            });
        }
    }
}