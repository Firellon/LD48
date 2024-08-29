﻿using Journal.ToggleButton;
using UnityEngine;
using Zenject;

namespace Journal
{
    public class JournalInstaller : MonoInstaller<JournalInstaller>
    {
        [SerializeField] private JournalToggleButtonView journalButtonView;
        [SerializeField] private JournalPanelController journalPanelController;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<JournalModel>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<JournalToggleButtonView>()
                .FromInstance(journalButtonView);
            Container.BindInterfacesAndSelfTo<JournalToggleButtonController>().AsSingle();
                
            Container.BindInterfacesTo<JournalPanelController>()
                .FromInstance(journalPanelController);
        }
    }
}