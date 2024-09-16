using UnityEngine;
using Zenject;

namespace Dialogue
{
    public class DialogueInstaller : MonoInstaller<DialogueInstaller>
    {
        [SerializeField] private DialogueView dialogueView;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DialogueView>().FromInstance(dialogueView).AsSingle();
            Container.BindInterfacesAndSelfTo<DialogueController>().AsSingle();
        }
    }
}