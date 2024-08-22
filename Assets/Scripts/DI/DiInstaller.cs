﻿using System.Collections.Generic;
using AudioTools.Sound;
using Day;
using DITools;
using FunkyCode;
using Inventory;
using LD48;
using LD48.AudioTool;
using Map;
using Map.Actor;
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
        [SerializeField] private MapGenerator mapGenerator;

        [SerializeField] private VisualsConfig visualsConfig; 

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

            Container.Bind<VisualsConfig>().FromInstance(visualsConfig).AsSingle();
            Container.Bind<MapGenerator>().FromInstance(mapGenerator).AsSingle();

            Container.BindInterfacesTo<MapActorRegistry>().FromInstance(mapActorRegistry).AsSingle();
            Container.BindInterfacesTo<MapObjectRegistry>().FromInstance(mapObjectRegistry).AsSingle();
            Container.BindInterfacesTo<ItemRegistry>().FromInstance(itemRegistry).AsSingle();
            Container.BindInterfacesTo<DayNightCycle>().FromInstance(dayNightCycle).AsSingle();

            Container.Bind<ISoundManager<SoundType>>().To<SoundManager>().AsSingle().NonLazy();

            Container.Bind<ILightCycle>().FromInstance(lightCycle).AsSingle();
        }

        private void OnDisable()
        {
            Destroy(prefabPool);
        }
    }
}