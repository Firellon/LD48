using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD48
{
    public class ObjectFollower : MonoBehaviour
    {
        public Transform target;
        public Vector3 targetOffset;
        private void Start()
        {
            targetOffset = transform.position - target.position;
        }
        void Update()
        {
            if (target)
            {
                transform.position = Vector3.Lerp(transform.position, target.position + targetOffset, 0.1f);
            }
        }
    }
}

