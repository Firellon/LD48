using System.Collections.Generic;
using DITools;
using Map;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.Random;
using Utilities.RandomService;
using Zenject;

namespace DI
{
    public class DiInstaller : MonoInstaller
    {
        [SerializeField] private PrefabPool prefabPool;
        [SerializeField] private MapActorRegistry mapActorRegistry;

        protected virtual void ConfigureServices()
        {
            Container.Configure(new List<ConfigureType>
            {
                new(typeof(IContainerConstructable), ScopeTypes.Singleton, false),
            });
        }

        public override void InstallBindings()
        {
            ConfigureServices();

            Container.Bind<IPrefabPool>().FromInstance(prefabPool).AsSingle().NonLazy();
            Container.Bind<IRandomService>().To<RandomService>().AsSingle().NonLazy();
            Container.Bind<Canvas>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<MapActorRegistry>().FromInstance(mapActorRegistry).AsSingle();
        }

        private void OnDisable()
        {
            Destroy(prefabPool);
        }
    }
}