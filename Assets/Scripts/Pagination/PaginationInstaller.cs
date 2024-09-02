using UnityEngine;
using Zenject;

namespace Pagination
{
    public class PaginationInstaller : MonoInstaller<PaginationInstaller>
    {
        [SerializeField] private PaginationView view;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PaginationView>().FromInstance(view).AsSingle();
            Container.BindInterfacesTo<PaginationController>().AsSingle();
        }
    }
}