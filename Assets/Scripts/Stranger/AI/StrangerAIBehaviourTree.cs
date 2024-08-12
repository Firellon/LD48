using System.Collections.Generic;
using BehaviorTree;
using BehaviorTree.Common;
using Inventory;
using Stranger.AI.Actions;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Stranger.AI
{
    public class StrangerAIBehaviourTree : BaseBehaviorTree, IStrangerBehaviorTree
    {
        [Inject] private DiContainer diContainer;
        [Inject] private StrangerAiConfig aiConfig;
        [Inject] private Transform strangerTransform;
        [Inject] private IInventory inventory;

        private StrangerAICalculationState state = new();
        public StrangerAICalculationState State => state;

        public override Node CreateTree()
        {
            state = GetEmptyState();
            var stateArgs = new object[] {state};

            var tree = new Selector(new List<INode>
            {
                new Sequence(new List<INode>
                {
                    diContainer.Instantiate<IsThreatenedCheck>(stateArgs),
                    new Selector(new List<INode>
                    {
                        diContainer.Instantiate<SurrenderAction>(stateArgs),
                        diContainer.Instantiate<AttackAction>(stateArgs),
                        diContainer.Instantiate<FleeAction>(stateArgs),
                    })
                }),
                new Sequence(new List<INode>()
                {
                    diContainer.Instantiate<IsNightCheck>(stateArgs),
                    new Selector(new List<INode>
                    {
                        diContainer.Instantiate<SeekBonfireAction>(stateArgs),
                        diContainer.Instantiate<StartBonfireAction>(stateArgs),
                    })
                }),
                new Sequence(new List<INode>
                {
                    diContainer.Instantiate<NeedsWoodCheck>(stateArgs),
                    new Selector(new List<INode>
                    {
                        diContainer.Instantiate<GatherWoodAction>(stateArgs),
                        diContainer.Instantiate<RobAction>(stateArgs),
                    })
                }),
                diContainer.Instantiate<WanderAction>(stateArgs),
            });

            return tree;
        }

        private StrangerAICalculationState GetEmptyState()
        {
            return new StrangerAICalculationState
            {
                TargetAction = aiConfig.DefaultState,
                Transform = strangerTransform,
                Inventory = inventory,
                Threats = new List<GameObject>(),
                MaybeTarget = Maybe.Empty<Transform>()
            };
        }
    }
}