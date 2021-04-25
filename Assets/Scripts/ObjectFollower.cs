using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD48
{
    public class ObjectFollower : MonoBehaviour
    {
        public Transform target;
        public Vector3 targetOffset;
        
        void Update()
        {
            if (target)
            {
                if (Vector2.Distance(transform.position, target.position) > 5f)
                {
                    transform.position = target.position + targetOffset;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, target.position + targetOffset, 0.1f);    
                }
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}

