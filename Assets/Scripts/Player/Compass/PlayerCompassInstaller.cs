using UnityEngine;
using Zenject;

namespace Player.Compass
{
    public class PlayerCompassInstaller : MonoInstaller<PlayerCompassInstaller>
    {
        [SerializeField] private PlayerCompassView playerCompassView;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerCompassView>().FromInstance(playerCompassView).AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerCompassController>().AsSingle();
        }
    }
}