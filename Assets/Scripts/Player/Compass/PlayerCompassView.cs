using Map.Actor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Utilities.Monads;
using Zenject;

namespace Player.Compass
{
    public class PlayerCompassView : MonoBehaviour
    {
        [SerializeField] private Image compassBackground;
        [SerializeField] private Image compassHand;
        [SerializeField] private float defaultCompassHandRotation = 45f;

        public Image CompassBackground => compassBackground;
        public Image CompassHand => compassHand;

        private float CompassHandRotation
        {
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
                    var targetAngle = ToAngle(targetVector);
                    // Debug.Log($"Update > player.transform.position {targetVector} => {targetAngle}");
                    CompassHandRotation = targetAngle;
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