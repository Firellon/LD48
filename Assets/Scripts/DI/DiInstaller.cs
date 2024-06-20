using System.Collections.Generic;
using Day;
using DITools;
using FunkyCode;
using Inventory;
using Map;
using Map.Actor;
using Signals;
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
        [SerializeField] private MapObjectRegistry mapObjectRegistry;
        [SerializeField] private ItemRegistry itemRegistry;
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private LightCycle lightCycle;

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
            Container.BindInterfacesTo<MapObjectRegistry>().FromInstance(mapObjectRegistry).AsSingle();
            Container.BindInterfacesTo<ItemRegistry>().FromInstance(itemRegistry).AsSingle();
            Container.BindInterfacesTo<DayNightCycle>().FromInstance(dayNightCycle).AsSingle();

            Container.Bind<ILightCycle>().FromInstance(lightCycle).AsSingle();
        }

        private void OnDisable()
        {
            Destroy(prefabPool);
        }
    }
}