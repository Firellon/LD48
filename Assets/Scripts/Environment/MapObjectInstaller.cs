using Map;
using UnityEngine;
using Zenject;

namespace Environment
{
    public class MapObjectInstaller : MonoInstaller
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private MapObjectController mapObjectController;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SpriteRenderer>().FromInstance(spriteRenderer);
            Container.BindInterfacesAndSelfTo<MapObjectController>().FromInstance(mapObjectController);
        }
    }
}