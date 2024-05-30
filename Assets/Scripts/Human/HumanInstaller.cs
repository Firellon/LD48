using UnityEngine;
using Zenject;

namespace Human
{
    public class HumanInstaller : MonoInstaller<HumanInstaller>
    {
        [SerializeField] private HumanInventory inventory;
        [SerializeField] private HumanController humanController;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<HumanInventory>().FromInstance(inventory);
            Container.BindInterfacesAndSelfTo<HumanController>().FromInstance(humanController);
        }
    }
}