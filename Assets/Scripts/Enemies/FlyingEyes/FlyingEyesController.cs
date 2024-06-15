using System;
using System.Collections;
using Map;
using UnityEngine;
using Zenject;

namespace LD48.Enemies
{
    public class FlyingEyesController : MonoBehaviour
    {
        [SerializeField] private GameObject flyingEyesPrefab;

        [Inject] private IMapActorRegistry mapActorRegistry;

        private void Start()
        {
            
        }

        private IEnumerator ShowFlyingEyesProcess()
        {
            
        }
    }
}