using Journal.JournalCurrentEntry;
using Journal.JournalPanel;
using Journal.ToggleButton;
using UnityEngine;
using Zenject;

namespace Journal
{
    public class JournalInstaller : MonoInstaller<JournalInstaller>
    {
        [SerializeField] private JournalToggleButtonView journalButtonView;
        [SerializeField] private JournalPanelController journalPanelController;
        [SerializeField] private JournalCurrentEntryView journalCurrentEntryView;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<JournalModel>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<JournalToggleButtonView>()
                .FromInstance(journalButtonView);
            Container.BindInterfacesAndSelfTo<JournalToggleButtonController>().AsSingle();

            Container.BindInterfacesAndSelfTo<JournalCurrentEntryView>().FromInstance(journalCurrentEntryView).AsSingle();
            Container.BindInterfacesAndSelfTo<JournalCurrentEntryController>().AsSingle();
            
            Container.BindInterfacesTo<JournalPanelController>()
                .FromInstance(journalPanelController);
            
        }
    }
}