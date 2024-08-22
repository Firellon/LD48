using UnityEngine;
using Zenject;

namespace Journal
{
    public class JournalInstaller : MonoInstaller<JournalInstaller>
    {
        [SerializeField] private JournalButtonController journalButton;
        [SerializeField] private JournalPanelController journalPanelController;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<JournalModel>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<JournalButtonController>()
                .FromInstance(journalButton);
            Container.BindInterfacesAndSelfTo<JournalPanelController>()
                .FromInstance(journalPanelController);
        }
    }
}