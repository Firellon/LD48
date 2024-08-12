using Stranger.AI;
using UnityEngine;
using Zenject;

namespace Stranger
{
    public class StrangerDiInstaller : MonoInstaller
    {
        [SerializeField] private StrangerController strangerController;
        [SerializeField] private StrangerAiConfig config;
        
        public override void InstallBindings()
        {
            Container.Bind<Transform>().FromInstance(transform).AsSingle();
            Container.Bind<StrangerAiConfig>().FromInstance(config).AsSingle();
            Container.BindInterfacesAndSelfTo<StrangerController>().FromInstance(strangerController).AsSingle();

            var aiBehaviourTree = Container.Instantiate<StrangerAIBehaviourTree>();
            Container.BindInterfacesAndSelfTo<StrangerAIBehaviourTree>().FromInstance(aiBehaviourTree).AsSingle();
            aiBehaviourTree.InitTree();
        }
    }
}