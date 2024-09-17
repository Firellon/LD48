using Dialogue;
using UnityEngine;
using Zenject;

namespace Hint
{
    public class HintInstaller : MonoInstaller<HintInstaller>
    {
        [SerializeField] private HintView hintView;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HintView>().FromInstance(hintView).AsSingle();
            Container.BindInterfacesAndSelfTo<HintController>().AsSingle();
        }
    }
}